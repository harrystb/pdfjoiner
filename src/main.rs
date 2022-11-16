use eframe::egui::widgets::{Button, Label};
use eframe::egui::{
    Align, Color32, Context, CursorIcon, Frame, Id, LayerId, Layout, Order, Response, RichText,
    Sense, Shape, SidePanel, Slider, TopBottomPanel, Ui, Vec2, Window,
};
use eframe::{egui, epaint};
use rfd::FileDialog;
use std::cmp::Ordering;
use std::collections::HashMap;
use std::ops::RangeInclusive;
use std::path::PathBuf;

fn main() {
    let mut options = eframe::NativeOptions::default();
    options.min_window_size = Some((900.0, 600.0).into());
    options.drag_and_drop_support = true;
    //options.max_window_size = Some((150.0,370.0).into());
    let mut app = PdfJoinerApp::default();
    eframe::run_native("PDFJoiner", options, Box::new(|_cc| Box::new(app)));
}

struct PdfFile {
    path: PathBuf,
    title: String,
    data: lopdf::Document,
}

impl PdfFile {
    fn new(path: PathBuf, data: lopdf::Document) -> Self {
        let title = path
            .file_name()
            .unwrap_or(path.as_os_str())
            .to_string_lossy()
            .to_string();
        Self { path, title, data }
    }
}

struct MsgBox {
    msg: String,
    title: String,
    open: bool,
}

impl MsgBox {
    fn new<T: Into<String>>(title: T, msg: T) -> Self {
        Self {
            msg: msg.into(),
            title: title.into(),
            open: true,
        }
    }

    fn show(&mut self, ctx: &Context) -> bool {
        Window::new(&self.title)
            .open(&mut self.open)
            .show(ctx, |ui| {
                ui.label(&self.msg);
            })
            .is_none()
    }
}

struct PdfJoinerApp {
    version: String,
    pdfs: HashMap<usize, PdfFile>,
    current_id: usize,
    selected_pdf: Option<usize>,
    msg_boxes: Vec<MsgBox>,
    from_page: usize,
    to_page: usize,
    segments: HashMap<usize, (usize, usize, usize)>,
    segment_order: Vec<usize>,
    current_segment_id: usize,
    source_index: usize,
    drop_index: usize,
}

impl Default for PdfJoinerApp {
    fn default() -> Self {
        PdfJoinerApp {
            version: env!("CARGO_PKG_VERSION").to_owned(),
            pdfs: HashMap::new(),
            current_id: 0,
            selected_pdf: None,
            msg_boxes: vec![],
            from_page: 0,
            to_page: 0,
            segments: HashMap::new(),
            segment_order: vec![],
            current_segment_id: 0,
            source_index: 0,
            drop_index: 0,
        }
    }
}

impl eframe::App for PdfJoinerApp {
    fn update(&mut self, ctx: &Context, _frame: &mut eframe::Frame) {
        self.render_msgboxes(ctx);
        self.render_header(ctx);
        self.render_footer(ctx);
        self.render_left_panel(ctx);
        self.render_right_panel(ctx);
        egui::CentralPanel::default().show(ctx, |ui| {
            ui.vertical(|ui| {
                ui.heading("2. Select Pages and Add to Generator");
                ui.separator();
                match &self.selected_pdf {
                    None => (),
                    Some(selected_pdf_id) => match self.pdfs.get_mut(selected_pdf_id) {
                        None => {
                            ui.label(format!(
                                "Pdf with id '{}' not found in list.",
                                selected_pdf_id
                            ));
                        }
                        Some(pdf_file) => {
                            ui.label(&pdf_file.title);
                            let page_count = pdf_file.data.get_pages().len();
                            ui.label(format!("Page count: {}", page_count));
                            let resp = ui.add(
                                Slider::new(
                                    &mut self.from_page,
                                    RangeInclusive::new(0, page_count),
                                )
                                .text("From"),
                            );
                            if resp.clicked() || resp.dragged() || resp.lost_focus() {
                                if self.from_page > self.to_page {
                                    self.to_page = self.from_page;
                                }
                            }
                            let resp = ui.add(
                                Slider::new(&mut self.to_page, RangeInclusive::new(0, page_count))
                                    .text("To"),
                            );
                            if resp.clicked() || resp.dragged() || resp.lost_focus() {
                                if self.to_page < self.from_page {
                                    self.from_page = self.to_page;
                                }
                            }
                            if ui.button("Add").clicked() {
                                self.segments.insert(
                                    self.current_segment_id.to_owned(),
                                    (
                                        selected_pdf_id.to_owned(),
                                        self.from_page.to_owned(),
                                        self.to_page.to_owned(),
                                    ),
                                );
                                self.segment_order.push(self.current_segment_id);
                                self.current_segment_id += 1;
                            }
                        }
                    },
                }
            });
        });
    }
}

const HEADER_FOOTER_BG_COLOUR: Color32 = Color32::from_rgb(60, 63, 65);
const SELECTED_BG_COLOUR: Color32 = Color32::from_rgb(100, 103, 105);
const SELECTED_FG_COLOUR: Color32 = Color32::from_rgb(10, 13, 15);

impl PdfJoinerApp {
    fn render_msgboxes(&mut self, ctx: &Context) {
        let mut closed_msgboxes = vec![];
        for (index, msgbox) in self.msg_boxes.iter_mut().enumerate() {
            if msgbox.show(ctx) {
                closed_msgboxes.push(index);
            }
        }
        closed_msgboxes.reverse();
        for index in closed_msgboxes {
            self.msg_boxes.remove(index);
        }
    }

    fn render_footer(&self, ctx: &Context) {
        let mut frame = egui::Frame::default();
        frame.fill = HEADER_FOOTER_BG_COLOUR;
        TopBottomPanel::bottom("footer")
            .frame(frame)
            .show(ctx, |ui| {
                ui.vertical_centered(|ui| {
                    ui.add_space(5.0);
                    ui.add(Label::new("Harrison St Baker"));
                    ui.add(Label::new(format!("Version: {}", self.version)));
                    ui.add_space(5.0);
                });
            });
    }
    fn render_header(&self, ctx: &Context) {
        let mut frame = egui::Frame::default();
        frame.fill = HEADER_FOOTER_BG_COLOUR;
        TopBottomPanel::top("header").frame(frame).show(ctx, |ui| {
            ui.vertical_centered(|ui| {
                ui.add_space(5.0);
                ui.heading("PDFJoiner");
                ui.add_space(5.0);
            });
        });
    }

    fn render_left_panel(&mut self, ctx: &Context) {
        for dropped_file in ctx.input().raw.dropped_files.iter() {
            if let Some(file) = &dropped_file.path {
                self.add_pdf_file(file);
            }
        }
        let resp = SidePanel::left("files").show(ctx, |ui| {
            ui.vertical(|ui| {
                ui.heading("1. Select Documents");
                ui.add_space(4.0);
                if ui
                    .add_sized([ui.available_width(), 20.], Button::new("Add Files"))
                    .clicked()
                {
                    let files = FileDialog::new()
                        .add_filter("pdf", &["pdf"])
                        .pick_files()
                        .unwrap_or(vec![]);
                    for file in files {
                        self.add_pdf_file(&file);
                    }
                }
                ui.separator();
                let mut pdfs_names: Vec<(usize, String)> = self
                    .pdfs
                    .iter()
                    .map(|(id, v)| (id.to_owned(), v.title.to_owned()))
                    .collect();
                pdfs_names.sort_by_key(|v| v.0);
                for (id, pdf_title) in pdfs_names {
                    ui.with_layout(Layout::right_to_left(Align::TOP), |ui| {
                        if ui.add(Button::new("X").small()).clicked() {
                            self.pdfs.remove(&id);
                        }
                        let mut is_selected = false;
                        if let Some(selected) = &self.selected_pdf {
                            if *selected == id {
                                is_selected = true;
                            }
                        }
                        let frame = match is_selected {
                            false => Frame::default(),
                            true => Frame::default().fill(SELECTED_BG_COLOUR),
                        };
                        frame.show(ui, |ui| {
                            ui.with_layout(Layout::left_to_right(Align::TOP), |ui| {
                                let mut l = match is_selected {
                                    true => Label::new(
                                        RichText::new(format!("({}) {}", id, pdf_title))
                                            .color(SELECTED_FG_COLOUR),
                                    ),
                                    false => Label::new(format!("({}) {}", id, pdf_title))
                                        .sense(Sense::click()),
                                };
                                if ui.add(l).clicked() {
                                    self.from_page = 0;
                                    self.to_page = match self.pdfs.get(&id) {
                                        None => 0,
                                        Some(pdffile) => pdffile.data.get_pages().len(),
                                    };
                                    self.selected_pdf = Some(id);
                                }
                            })
                        });
                    });
                }
            });
        });
    }

    // Dragging based on eGui example https://github.com/emilk/egui/blob/master/crates/egui_demo_lib/src/demo/drag_and_drop.rs
    fn draggable_item(ui: &mut Ui, item_id: Id, item_ui: impl FnOnce(&mut Ui)) -> bool {
        let is_being_dragged = ui.memory().is_being_dragged(item_id);
        if !is_being_dragged {
            let response = ui.scope(item_ui).response;
            let response = ui.interact(response.rect, item_id, Sense::drag());
            if response.hovered() {
                ui.output().cursor_icon = CursorIcon::Grab;
            }
        } else {
            ui.output().cursor_icon = CursorIcon::Grabbing;
            let layer_id = LayerId::new(Order::Tooltip, item_id);
            let response = ui.with_layer_id(layer_id, item_ui).response;
            // move item to be under cursor
            if let Some(pointer_pos) = ui.ctx().pointer_interact_pos() {
                let delta = pointer_pos - response.rect.center();
                ui.ctx().translate_layer(layer_id, delta.clone());
            }
        }
        is_being_dragged
    }

    fn render_right_panel(&mut self, ctx: &Context) {
        SidePanel::right("generation").show(ctx, |ui| {
            ui.vertical(|ui| {
                ui.heading("3. Generation Document");
                ui.add_space(4.0);
                if ui
                    .add_sized([ui.available_width(), 20.], Button::new("Generate"))
                    .clicked()
                {}
                ui.separator();
                let mut segments_to_remove = vec![];
                for (segment_id_index, segment_id) in self.segment_order.clone().iter().enumerate()
                {
                    if let Some(temp_val) = self.segments.get(&segment_id) {
                        let (id, from, to) = temp_val.to_owned();
                        let item_id = Id::new(segment_id);
                        if PdfJoinerApp::draggable_item(ui, item_id, |ui| {
                            let response = ui
                                .with_layout(Layout::right_to_left(Align::TOP), |ui| {
                                    if ui.add(Button::new("X").small()).clicked() {
                                        segments_to_remove.push(segment_id.to_owned());
                                    }
                                    match self.pdfs.get(&id) {
                                        None => {
                                            ui.label(
                                                RichText::new(format!(
                                                    "Id {} no longer found.",
                                                    id
                                                ))
                                                .color(Color32::RED),
                                            );
                                        }
                                        Some(pdffile) => {
                                            ui.with_layout(
                                                Layout::left_to_right(Align::TOP),
                                                |ui| {
                                                    ui.label(format!(
                                                        "({}){}\nFrom: {} To: {}",
                                                        id, pdffile.title, from, to
                                                    ));
                                                },
                                            );
                                        }
                                    }
                                })
                                .response;
                            if response.hovered() {
                                if let Some(pointer_pos) = ui.ctx().pointer_interact_pos() {
                                    let delta = pointer_pos - response.rect.center();
                                    if delta.y < 0.0 {
                                        // put before
                                        self.drop_index = segment_id_index.clone();
                                    } else {
                                        //put after
                                        self.drop_index = segment_id_index.clone() + 1;
                                    }
                                }
                            }
                        }) {
                            self.source_index = segment_id_index.clone();
                        }
                    }
                }
                let mut something_is_being_dragged = ui.memory().is_anything_being_dragged();
                // TODO: Remove if I don't run into issues without it. It is a rectangle to allow putting the item at the end but the changes to check relative position seem to do it better.
                // if something_is_being_dragged {
                //     let where_to_put_background = ui.painter().add(Shape::Noop);
                //     let (rect, response) =
                //         ui.allocate_at_least(Vec2::new(ui.available_width(), 20.), Sense::hover());
                //     let style = ui.visuals().widgets.active;
                //     ui.painter().set(
                //         where_to_put_background,
                //         eframe::epaint::RectShape {
                //             rounding: style.rounding,
                //             fill: style.bg_fill,
                //             stroke: style.bg_stroke,
                //             rect,
                //         },
                //     );
                //     if response.hovered() {
                //         self.drop_index = self.segment_order.len();
                //     }
                // }
                for segment_id in segments_to_remove {
                    self.segments.remove(&segment_id);
                }
                if something_is_being_dragged && ui.input().pointer.any_released() {
                    // re-arrange
                    if self.source_index != self.drop_index {
                        let removed_segment_id = self.segment_order.remove(self.source_index);
                        let mut target_index = self.drop_index;
                        // if the source is less than the drop, the index will shift when we remove the item
                        if self.source_index < self.drop_index {
                            target_index -= 1;
                        }
                        self.segment_order.insert(target_index, removed_segment_id);
                    }
                }
                println!(
                    "{},{},{},{}",
                    self.source_index,
                    self.drop_index,
                    something_is_being_dragged,
                    ui.input().pointer.any_released()
                );
            });
        });
    }

    fn add_pdf_file(&mut self, file: &PathBuf) {
        match lopdf::Document::load(file.as_path()) {
            Err(e) => self.msg_boxes.push(MsgBox::new(
                "Error while loading pdf".to_owned(),
                format!("Could load pdf '{}'.\n{:?}.\nContact Harrison St Baker (harry.stbaker@gmail.com) with this error if you believe this is a valid pdf.", file.display(), e),
            )),
            Ok(d) => {
                self.pdfs.insert(self.current_id, PdfFile::new(file.to_owned(), d));
                self.current_id += 1;
            }
        }
    }
}
