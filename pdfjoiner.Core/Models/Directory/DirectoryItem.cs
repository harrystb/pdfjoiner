namespace pdfjoiner.Core.Models
{
    public class DirectoryItem
    {
        public DirectoryItemType Type { get; set; }

        public string FullPath { get; set; }
        public string Name { get { return Type == DirectoryItemType.Drive ? FullPath : DirectoryHelpers.GetFileFolderName(FullPath); } }
    }
}
