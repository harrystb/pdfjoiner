use eframe::{egui, Frame, Storage};
use eframe::egui::{Context, Color32, TopBottomPanel, SidePanel, Layout, Align};
use eframe::egui::widgets::{Label, Button};


fn main() {
    let mut options = eframe::NativeOptions::default();
    options.min_window_size = Some((150.0,370.0).into());
    //options.max_window_size = Some((150.0,370.0).into());
    let mut app = PdfJoinerApp::default();
    eframe::run_native(
        "PDFJoiner",
        options,
        Box::new(|_cc| Box::new(app)),
    );
}

struct PdfJoinerApp {
    version: String,
}

impl Default for PdfJoinerApp {
    fn default() -> Self {
        PdfJoinerApp {
            version: "0.1".to_owned(),
        }
    }
}

impl eframe::App for PdfJoinerApp {
    fn update(&mut self, ctx: &Context, _frame: &mut Frame) {
        self.render_header(ctx);
        self.render_footer(ctx);
        self.render_left_panel(ctx);
        egui::CentralPanel::default().show(ctx, |ui| {ui.label("Hello");});
    }
}

const HEADER_FOOTER_BG_COLOUR: Color32 = Color32::from_rgb(60,63,65,);

impl PdfJoinerApp {
    fn render_footer(&self, ctx: &Context) {
        let mut frame = egui::Frame::default();
        frame.fill = HEADER_FOOTER_BG_COLOUR;
        TopBottomPanel::bottom("footer").frame(frame).show(ctx, |ui| {
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
        SidePanel::left("files").show(ctx, |ui| {
            ui.vertical(|ui|{
                ui.add_space(4.0);
                if ui.add_sized([ui.available_width(), 20.], Button::new("Add Files")).clicked() {
                    // do something...
                }
                ui.separator();
                ui.with_layout(Layout::right_to_left(Align::TOP), |ui| {
                    ui.add(Button::new("X").small());
                    ui.with_layout(Layout::left_to_right(Align::TOP), |ui| {ui.label("A file??");});
                });
            });
        });
    }

    fn render_right_panel(&mut self, ctx: &Context) {
        SidePanel::right("generation").show(ctx, |ui| {
            ui.vertical(|ui|{
                ui.add_space(4.0);
                if ui.add_sized([ui.available_width(), 20.], Button::new("Add Files")).clicked() {
                    // do something...
                }
                ui.separator();
                ui.with_layout(Layout::right_to_left(Align::TOP), |ui| {
                    ui.add(Button::new("X").small());
                    ui.with_layout(Layout::left_to_right(Align::TOP), |ui| {ui.label("A file??");});
                });
            });
        });
    }
}
