#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use eframe::egui;
use eframe::egui::widgets::{Button, Label};
use eframe::egui::{
    Align, Color32, Context, CursorIcon, Frame, Id, Image, ImageData, LayerId, Layout, Order, Pos2,
    Rect, RichText, Sense, SidePanel, Slider, TextStyle, TextureHandle, TopBottomPanel, Ui, Vec2,
    Window,
};
use egui_extras::image::RetainedImage;
use rfd::FileDialog;
use std::collections::btree_map::BTreeMap;
use std::collections::HashMap;
use std::ops::RangeInclusive;
use std::path::PathBuf;

fn main() {
    let mut options = eframe::NativeOptions::default();
    options.min_window_size = Some((900.0, 600.0).into());
    options.drag_and_drop_support = true;
    //options.max_window_size = Some((150.0,370.0).into());
    let app = PdfJoinerApp::default();
    eframe::run_native("PDFJoiner", options, Box::new(|_cc| Box::new(app)));
}

struct PdfFile {
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
        Self { title, data }
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
    header_img: RetainedImage,
}

impl Default for PdfJoinerApp {
    fn default() -> Self {
        #[cfg(windows)]
        let header_banner_image = RetainedImage::from_image_bytes(
            "header-banner.svg",
            include_bytes!("resources\\bannerlogo.png"),
        )
        .unwrap();
        #[cfg(not(windows))]
        let header_banner_image = RetainedImage::from_image_bytes(
            "header-banner.svg",
            include_bytes!("resources/bannerlogo.png"),
        )
        .unwrap();
        PdfJoinerApp {
            version: env!("CARGO_PKG_VERSION").to_owned(),
            pdfs: HashMap::new(),
            current_id: 0,
            selected_pdf: None,
            msg_boxes: vec![],
            from_page: 1,
            to_page: 1,
            segments: HashMap::new(),
            segment_order: vec![],
            current_segment_id: 0,
            source_index: 0,
            drop_index: 0,
            header_img: header_banner_image,
        }
    }
}

impl eframe::App for PdfJoinerApp {
    fn update(&mut self, ctx: &Context, _frame: &mut eframe::Frame) {
        let mut style = (*ctx.style()).clone();
        style.visuals.override_text_color = Some(SELECTED_FG_COLOUR);
        style.visuals.widgets.inactive.bg_fill = BUTTON_ACTIVE_COLOUR;
        style.visuals.widgets.hovered.bg_fill = BUTTON_HOVER_COLOUR;
        style.visuals.widgets.active.bg_fill = BUTTON_HOVER_COLOUR;
        style.visuals.widgets.hovered.bg_stroke.color = BUTTON_HOVER_STROKE_COLOUR;
        style.visuals.widgets.active.fg_stroke.color = BUTTON_TEXT_COLOUR;
        style.visuals.widgets.inactive.fg_stroke.color = BUTTON_TEXT_COLOUR;
        style.visuals.widgets.hovered.fg_stroke.color = BUTTON_TEXT_COLOUR;
        style.visuals.widgets.noninteractive.fg_stroke.color = SELECTED_FG_COLOUR;
        let mut button_style = style.text_styles.get_mut(&TextStyle::Button).unwrap();
        button_style.size = 16.;
        ctx.set_style(style);
        for dropped_file in ctx.input().raw.dropped_files.iter() {
            if let Some(file) = &dropped_file.path {
                self.add_pdf_file(file);
            }
        }
        self.render_msgboxes(ctx);
        self.render_header(ctx);
        self.render_footer(ctx);
        if self.pdfs.len() == 0 {
            self.render_starting_panel(ctx);
        } else {
            self.render_active_panels(ctx);
        }
    }
}
const BG_GRAY: Color32 = Color32::from_rgb(201, 199, 204);
const BG_LIGHT_GRAY: Color32 = Color32::from_rgb(235, 235, 235);
const BUTTON_ACTIVE_COLOUR: Color32 = Color32::from_rgb(98, 69, 199);
const BUTTON_HOVER_COLOUR: Color32 = Color32::from_rgb(78, 49, 159);
const BUTTON_HOVER_STROKE_COLOUR: Color32 = Color32::from_rgb(98, 69, 199);
const BUTTON_INACTIVE_COLOUR: Color32 = Color32::from_rgb(78, 49, 159);
const BUTTON_TEXT_COLOUR: Color32 = Color32::from_rgb(196, 190, 199);

const HEADER_FOOTER_BG_COLOUR: Color32 = Color32::from_rgb(201, 199, 204);
const MAIN_BG_COLOUR: Color32 = Color32::from_rgb(212, 212, 212);
const SELECTED_BG_COLOUR: Color32 = Color32::from_rgb(100, 103, 105);
const SELECTED_FG_COLOUR: Color32 = Color32::from_rgb(10, 13, 15);
const LIGHT_FG_COLOUR: Color32 = Color32::from_rgb(150, 153, 155);

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
            if self.pdfs.len() > 0 {
                //short version
                ui.vertical(|ui| {
                    ui.add_space(5.0);
                    ui.label(
                        RichText::new("PDFJoiner")
                            .color(Color32::DARK_RED)
                            .heading()
                            .strong()
                            .italics(),
                    );
                    ui.add_space(5.0);
                });
            } else {
                //long version
                ui.vertical_centered(|ui| {
                    ui.add_space(5.0);
                    self.header_img.show(ui);
                    ui.add_space(5.0);
                });
            }
        });
    }

    fn render_starting_panel(&mut self, ctx: &Context) {
        let mut frame = egui::Frame::default();
        frame.fill = MAIN_BG_COLOUR;
        frame.stroke.color = Color32::BLACK;
        frame.stroke.width = 1.0;
        egui::CentralPanel::default().frame(frame).show(ctx, |ui| {
            let mut layout = Layout::default();
            layout.with_cross_align(Align::Center);
            layout.with_main_align(Align::Center);
            ui.vertical_centered(|ui| {
                ui.add_space((ui.available_height() - 60.) / 2.);
                ui.heading("Add a PDF document to start...");
                ui.label("You can drag and drop or press the button below for a file dialog.");
                if ui
                    .add_sized([200., 20.], Button::new("Add Files"))
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
            });
        });
    }

    fn render_active_panels(&mut self, ctx: &Context) {
        self.render_left_panel(ctx);
        self.render_right_panel(ctx);
        let mut frame = egui::Frame::default();
        frame.fill = MAIN_BG_COLOUR;
        frame.stroke.color = Color32::BLACK;
        frame.stroke.width = 1.0;
        frame.inner_margin.left = 3.0;
        egui::CentralPanel::default().frame(frame).show(ctx, |ui| {
            ui.vertical(|ui| {
                ui.heading("Page Selection").on_hover_text(
                    RichText::new("Select the page range to add to the output document.")
                        .italics()
                        .color(LIGHT_FG_COLOUR),
                );
                ui.add_space(4.0);
                match &self.selected_pdf {
                    None => {
                        ui.label(RichText::new("Select a document to start...").italics());
                    }
                    Some(selected_pdf_id) => match self.pdfs.get_mut(selected_pdf_id) {
                        None => {
                            ui.label(format!(
                                "Pdf with id '{}' not found in list.",
                                selected_pdf_id
                            ));
                        }
                        Some(pdf_file) => {
                            ui.horizontal(|ui| {
                                ui.label(RichText::new("Filename:"));
                                ui.label(RichText::new(&pdf_file.title).italics());
                            });
                            let page_count = pdf_file.data.get_pages().len();
                            ui.horizontal(|ui| {
                                ui.label(RichText::new("Total pages in document:"));
                                ui.label(RichText::new(page_count.to_string()).italics());
                            });
                            let resp = ui.add(
                                Slider::new(
                                    &mut self.from_page,
                                    RangeInclusive::new(1, page_count),
                                )
                                .text("From"),
                            );
                            if resp.clicked() || resp.dragged() || resp.lost_focus() {
                                if self.from_page > self.to_page {
                                    self.to_page = self.from_page;
                                }
                            }
                            let resp = ui.add(
                                Slider::new(&mut self.to_page, RangeInclusive::new(1, page_count))
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

    fn render_left_panel(&mut self, ctx: &Context) {
        let mut frame = egui::Frame::default();
        frame.fill = MAIN_BG_COLOUR;
        frame.stroke.color = Color32::BLACK;
        frame.stroke.width = 1.0;
        frame.inner_margin.left = 3.0;
        SidePanel::left("files").frame(frame).show(ctx, |ui| {
            ui.set_width(ctx.available_rect().width() / 3.);
            ui.vertical(|ui| {
                ui.heading("Input Documents").on_hover_text(
                    RichText::new("Select a document which will be used for the output document. More input documents can be added by drag and dropping or pressing the button below.")
                        .italics()
                        .color(LIGHT_FG_COLOUR),
                );
                ui.add_space(4.0);
                let mut pdfs_names: Vec<(usize, String)> = self
                    .pdfs
                    .iter()
                    .map(|(id, v)| (id.to_owned(), v.title.to_owned()))
                    .collect();
                pdfs_names.sort_by_key(|v| v.0);
                for (id, pdf_title) in pdfs_names {
                    ui.with_layout(Layout::right_to_left(Align::TOP), |ui| {
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
                        // show delete button only on hovered item
                        let mut hover_rect = ui.max_rect();
                        hover_rect.set_height(18.);
                        if ui.rect_contains_pointer(hover_rect) {
                            if ui.add(Button::new("X").small()).clicked() {
                                self.pdfs.remove(&id);
                                if is_selected {
                                    self.selected_pdf = None;
                                }
                                let mut segment_ids_to_remove = vec![];
                                for (seg_id, (pdf_id, _,_))in self.segments.iter() {
                                    if *pdf_id == id {
                                        segment_ids_to_remove.push(seg_id.clone());
                                    }
                                }
                                for segment_id in segment_ids_to_remove {
                                    self.segments.remove(&segment_id);
                                    self.segment_order.retain(|v| *v != segment_id);
                                }
                            }
                        }
                        if ui
                            .interact(
                                hover_rect,
                                Id::new(format!("hoverrect_{}", id)),
                                Sense::click(),
                            )
                            .clicked()
                        {
                            self.from_page = 1;
                            self.to_page = match self.pdfs.get(&id) {
                                None => 1,
                                Some(pdffile) => pdffile.data.get_pages().len(),
                            };
                            self.selected_pdf = Some(id);
                        }
                        // add pdf title label
                        frame.show(ui, |ui| {
                            ui.with_layout(Layout::left_to_right(Align::TOP), |ui| {
                                let l = match is_selected {
                                    true => Label::new(
                                        RichText::new(format!("{}. {}", id + 1, pdf_title))
                                            .color(SELECTED_FG_COLOUR),
                                    ),
                                    false => Label::new(format!("{}. {}", id + 1, pdf_title))
                                        .sense(Sense::click()),
                                };
                                ui.add(l);
                            });
                        });
                    });
                }
            });
            ui.vertical_centered(|ui| {
                if ui
                    .add_sized([120., 20.], Button::new("Add More Files"))
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
            })
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
        let mut frame = egui::Frame::default();
        frame.fill = MAIN_BG_COLOUR;
        frame.stroke.color = Color32::BLACK;
        frame.stroke.width = 1.0;
        frame.inner_margin.left = 3.0;
        SidePanel::right("generation").frame(frame).show(ctx, |ui| {
            ui.set_width(ctx.available_rect().width() / 2.);
            ui.vertical(|ui| {
                ui.heading("Output Section").on_hover_text(
                    RichText::new("Generate the output document from the page sections shown below by pressing the generate button. The order of the sections can be changed by dragging and dropping or they can be removed by pressing the x button.")
                        .italics()
                        .color(LIGHT_FG_COLOUR),
                );
                ui.add_space(4.0);
                let mut segments_to_remove = vec![];
                for (segment_id_index, segment_id) in self.segment_order.clone().iter().enumerate()
                {
                    if let Some(temp_val) = self.segments.get(&segment_id) {
                        let (id, from, to) = temp_val.to_owned();
                        let item_id = Id::new(segment_id);
                        ui.with_layout(Layout::right_to_left(Align::TOP), |ui| {
                            let mut hover_rect = ui.max_rect();
                            hover_rect.set_height(36.);
                            if ui.rect_contains_pointer(hover_rect) {
                                if ui.add(Button::new("X").small()).clicked() {
                                    segments_to_remove.push(segment_id.to_owned());
                                }
                            }
                            if PdfJoinerApp::draggable_item(ui, item_id, |ui| {
                                let response = match self.pdfs.get(&id) {
                                    None => ui.label(
                                        RichText::new(format!("Id {} no longer found.", id))
                                            .color(Color32::RED),
                                    ),
                                    Some(pdffile) => {
                                        ui.with_layout(Layout::left_to_right(Align::TOP), |ui| {
                                            ui.label(format!(
                                                "Source Document ID: {}\nFilename: {}\nPages {} to {}.",
                                                id+1, pdffile.title, from, to
                                            ));
                                        })
                                        .response
                                    }
                                };
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
                        });
                    }
                }
                let something_is_being_dragged = ui.memory().is_anything_being_dragged();
                for segment_id in segments_to_remove {
                    // remove from segments and the segment order vec
                    self.segment_order.retain(|v| *v != segment_id);
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
            });
            ui.vertical_centered(|ui| {
                if ui.add_sized([120., 20.], Button::new("Generate")).clicked() {
                    self.generate_pdf();
                }
                if ui.add_sized([80., 20.], Button::new("Clear")).clicked() {
                    self.segment_order.clear();
                    self.segments.clear();
                }
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

    fn generate_pdf(&mut self) {
        let mut max_id = 1;
        // Collect all Documents Objects grouped by a map
        let mut documents_pages: BTreeMap<lopdf::ObjectId, lopdf::Object> = BTreeMap::new();
        let mut documents_objects: BTreeMap<lopdf::ObjectId, lopdf::Object> = BTreeMap::new();
        let mut document = lopdf::Document::with_version("1.5");
        let mut pdf_index_being_joined: Vec<usize> = self.segments.values().map(|v| v.0).collect();
        pdf_index_being_joined.sort();
        pdf_index_being_joined.dedup();
        let mut pdfs_being_joined = HashMap::new();
        for index in pdf_index_being_joined {
            match self.pdfs.get(&index) {
                None => {
                    self.msg_boxes.push(MsgBox::new(
                        "Cannot generate document",
                        format!("Invalid document index {}.", index).as_str(),
                    ));
                    return;
                }
                Some(pdf) => {
                    let mut doc = pdf.data.clone();
                    // renumber the objects in the documents so we can join it
                    doc.renumber_objects_with(max_id);
                    max_id = doc.max_id + 1;
                    pdfs_being_joined.insert(index, doc);
                }
            }
        }
        for segment_id in &self.segment_order {
            match self.segments.get(segment_id) {
                None => {
                    self.msg_boxes.push(MsgBox::new(
                        "Cannot generate document",
                        format!("Invalid segment id {}.", segment_id).as_str(),
                    ));
                    return;
                }
                Some((doc_id, from, to)) => match pdfs_being_joined.get(doc_id) {
                    None => {
                        self.msg_boxes.push(MsgBox::new(
                            "Cannot generate document",
                            format!("Invalid document id {}.", doc_id).as_str(),
                        ));
                        return;
                    }
                    Some(doc) => {
                        documents_pages.extend(
                            doc.get_pages()
                                .into_iter()
                                .filter(|(i, _)| *i as usize >= *from && *i as usize <= *to)
                                .map(|(_, object_id)| {
                                    // change page object ID so the order is as per the segment ordering
                                    max_id += 1;
                                    (
                                        (max_id - 1, object_id.1),
                                        doc.get_object(object_id).unwrap().to_owned(),
                                    )
                                }),
                        );
                    }
                },
            }
        }
        pdfs_being_joined.iter().for_each(|(_, doc)| {
            documents_objects.extend(
                doc.objects
                    .iter()
                    .map(|(k, v)| (k.to_owned(), v.to_owned())),
            );
        });
        let mut catalog_object: Option<(lopdf::ObjectId, lopdf::Object)> = None;
        let mut pages_object: Option<(lopdf::ObjectId, lopdf::Object)> = None;
        // Process all objects except "Page" type
        for (object_id, object) in documents_objects.iter() {
            // We have to ignore "Page" (as are processed later), "Outlines" and "Outline" objects
            // All other objects should be collected and inserted into the main Document
            match object.type_name().unwrap_or("") {
                "Catalog" => {
                    // Collect a first "Catalog" object and use it for the future "Pages"
                    catalog_object = Some((
                        if let Some((id, _)) = catalog_object {
                            id
                        } else {
                            *object_id
                        },
                        object.clone(),
                    ));
                }
                "Pages" => {
                    // Collect and update a first "Pages" object and use it for the future "Catalog"
                    // We have also to merge all dictionaries of the old and the new "Pages" object
                    if let Ok(dictionary) = object.as_dict() {
                        let mut dictionary = dictionary.clone();
                        if let Some((_, ref object)) = pages_object {
                            if let Ok(old_dictionary) = object.as_dict() {
                                dictionary.extend(old_dictionary);
                            }
                        }

                        pages_object = Some((
                            if let Some((id, _)) = pages_object {
                                id
                            } else {
                                *object_id
                            },
                            lopdf::Object::Dictionary(dictionary),
                        ));
                    }
                }
                "Page" => {}     // Ignored, processed later and separately
                "Outlines" => {} // Ignored, not supported yet
                "Outline" => {}  // Ignored, not supported yet
                _ => {
                    document.objects.insert(*object_id, object.clone());
                }
            }
        }

        // If no "Pages" found abort
        if pages_object.is_none() {
            self.msg_boxes.push(MsgBox::new(
                "Cannot generate document",
                "Error generating the document - could not find page root.",
            ));
            return;
        }

        // Iter over all "Page" and collect with the parent "Pages" created before
        for (object_id, object) in documents_pages.iter() {
            if let Ok(dictionary) = object.as_dict() {
                let mut dictionary = dictionary.clone();
                dictionary.set("Parent", pages_object.as_ref().unwrap().0);

                document
                    .objects
                    .insert(*object_id, lopdf::Object::Dictionary(dictionary));
            }
        }

        // If no "Catalog" found abort
        if catalog_object.is_none() {
            self.msg_boxes.push(MsgBox::new(
                "Cannot generate document",
                "Error generating the document - could not find catelog root.",
            ));
            return;
        }

        let catalog_object = catalog_object.unwrap();
        let pages_object = pages_object.unwrap();

        // Build a new "Pages" with updated fields
        if let Ok(dictionary) = pages_object.1.as_dict() {
            let mut dictionary = dictionary.clone();

            // Set new pages count
            dictionary.set("Count", documents_pages.len() as u32);

            // Set new "Kids" list (collected from documents pages) for "Pages"
            dictionary.set(
                "Kids",
                documents_pages
                    .into_iter()
                    .map(|(object_id, _)| lopdf::Object::Reference(object_id))
                    .collect::<Vec<_>>(),
            );

            document
                .objects
                .insert(pages_object.0, lopdf::Object::Dictionary(dictionary));
        }

        // Build a new "Catalog" with updated fields
        if let Ok(dictionary) = catalog_object.1.as_dict() {
            let mut dictionary = dictionary.clone();
            dictionary.set("Pages", pages_object.0);
            dictionary.remove(b"Outlines"); // Outlines not supported in merged PDFs

            document
                .objects
                .insert(catalog_object.0, lopdf::Object::Dictionary(dictionary));
        }

        document.trailer.set("Root", catalog_object.0);

        // Update the max internal ID as wasn't updated before due to direct objects insertion
        document.max_id = document.objects.len() as u32;

        //prune unused pages
        document.prune_objects();

        // Reorder all new Document objects
        document.renumber_objects();

        //Set any Bookmarks to the First child if they are not set to a page
        document.adjust_zero_pages();

        //Set all bookmarks to the PDF Object tree then set the Outlines to the Bookmark content map.
        if let Some(n) = document.build_outline() {
            if let Ok(x) = document.get_object_mut(catalog_object.0) {
                if let lopdf::Object::Dictionary(ref mut dict) = x {
                    dict.set("Outlines", lopdf::Object::Reference(n));
                }
            }
        }

        document.compress();

        match rfd::FileDialog::new()
            .add_filter("PDF", &["pdf"])
            .save_file()
        {
            None => (), // do nothing
            Some(p) => match document.save(p.as_path()) {
                Err(e) => {
                    self.msg_boxes.push(MsgBox::new(
                        "Cannot generate document",
                        format!("Error saving the document - {:?}.", e).as_str(),
                    ));
                }
                _ => (),
            },
        }
    }
}
