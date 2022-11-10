namespace pdfjoiner.DesktopClient.UserControls
{
    public class FileExplorerItem
    {
        public FileExplorerItemType Type { get; set; }

        public string FullPath { get; set; }
        public string Name { get { return Type == FileExplorerItemType.Drive ? FullPath : FileExplorerHelpers.GetFileFolderName(FullPath); } }
    }
}
