use eframe::egui;
use eframe::egui::widgets::{Button, Label};
use eframe::egui::{
    Align, Color32, Context, Frame, Layout, RichText, Sense, SidePanel, TopBottomPanel, Window,
};
use rfd::FileDialog;
use std::cmp::Ordering;
use std::collections::HashMap;
use std::path::PathBuf;

fn main() {
    let mut options = eframe::NativeOptions::default();
    options.min_window_size = Some((150.0, 370.0).into());
    options.drag_and_drop_support = true;
    //options.max_window_size = Some((150.0,370.0).into());
    let mut app = PdfJoinerApp::default();
    eframe::run_native("PDFJoiner", options, Box::new(|_cc| Box::new(app)));
}

struct PdfFile {
    path: PathBuf,
    data: lopdf::Document,
}

impl PdfFile {
    fn new(path: PathBuf, data: lopdf::Document) -> Self {
        Self { path, data }
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
    pdfs: HashMap<String, PdfFile>,
    selected_pdf: Option<String>,
    msg_boxes: Vec<MsgBox>,
}

impl Default for PdfJoinerApp {
    fn default() -> Self {
        PdfJoinerApp {
            version: env!("CARGO_PKG_VERSION").to_owned(),
            pdfs: HashMap::new(),
            selected_pdf: None,
            msg_boxes: vec![],
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
                ui.heading("Page Selection");
                match &self.selected_pdf {
                    None => (),
                    Some(selected_pdf) => {
                        ui.label(selected_pdf);
                        match self.pdfs.get_mut(selected_pdf) {
                            None => {
                                ui.label(format!("Pdf '{}' no found in list.", selected_pdf));
                            }
                            Some(pdf_file) => {
                                ui.label(format!("Pages: {}", pdf_file.data.get_pages().len()));
                            }
                        }
                    }
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
                let title = file
                    .file_name()
                    .unwrap_or(file.as_os_str())
                    .to_string_lossy()
                    .to_string();
                match lopdf::Document::load(file.as_path()) {
                    Err(e) => self.msg_boxes.push(MsgBox::new(
                        "Error while loading pdf".to_owned(),
                        format!("Could load pdf '{}'.\n{:?}.\nContact Harrison St Baker with this error if you believe this is a valid pdf.", title, e),
                    )),
                    Ok(d) => {
                        self.pdfs.insert(title, PdfFile::new(file.to_owned(), d));
                    }
                }
            }
        }
        let resp = SidePanel::left("files").show(ctx, |ui| {
            ui.vertical(|ui| {
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
                        let title = file
                            .file_name()
                            .unwrap_or(file.as_os_str())
                            .to_string_lossy()
                            .to_string();
                        match lopdf::Document::load(file.as_path()) {
                            Err(e) => self.msg_boxes.push(MsgBox::new(
                                "Error while loading pdf".to_owned(),
                                format!("Could load pdf '{}'.\n{:?}.\nContact Harrison St Baker (harry.stbaker@gmail.com) with this error if you believe this is a valid pdf.", title, e),
                            )),
                            Ok(d) => {
                                self.pdfs.insert(title, PdfFile::new(file, d));
                            }
                        }
                    }
                }
                ui.separator();
                let mut pdfs_names: Vec<String> = self.pdfs.keys().map(|v| v.to_owned()).collect();
                for pdf in pdfs_names {
                    ui.with_layout(Layout::right_to_left(Align::TOP), |ui| {
                        if ui.add(Button::new("X").small()).clicked() {
                            self.pdfs.remove(&pdf);
                        }
                        let mut is_selected = false;
                        if let Some(selected) = &self.selected_pdf {
                            if selected == &pdf {
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
                                    true => {
                                        Label::new(RichText::new(&pdf).color(SELECTED_FG_COLOUR))
                                    }
                                    false => Label::new(&pdf).sense(Sense::click()),
                                };
                                if ui.add(l).clicked() {
                                    self.selected_pdf = Some(pdf);
                                }
                            })
                        });
                    });
                }
            });
        });
    }

    fn render_right_panel(&mut self, ctx: &Context) {
        SidePanel::right("generation").show(ctx, |ui| {
            ui.vertical(|ui| {
                ui.add_space(4.0);
                if ui
                    .add_sized([ui.available_width(), 20.], Button::new("Generate"))
                    .clicked()
                {}
                ui.separator();
            });
        });
    }
}
