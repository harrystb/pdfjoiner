cargo build --release &&
rm -rf ../mac_release &&
mkdir ../mac_release &&
mkdir ../mac_release/pdfjoiner.app &&
mkdir ../mac_release/pdfjoiner.app/Contents &&
cp ../target/release/pdfjoiner ../mac_release/pdfjoiner.app/Contents &&
cp mac_misc/Info.plist ../mac_release/pdfjoiner.app/Contents &&
ln -s /Applications ../mac_release/Applications &&
hdiutil create -volname pdfjoiner -srcfolder ../mac_release -ov ../mac_release/pdfjoiner.dmg
