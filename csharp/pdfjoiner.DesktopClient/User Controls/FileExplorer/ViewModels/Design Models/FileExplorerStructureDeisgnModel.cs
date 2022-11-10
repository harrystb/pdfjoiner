using pdfjoiner.DesktopClient.UserControls.ViewModels;
using System.Collections.ObjectModel;

namespace pdfjoiner.DesktopClient.UserControls
{
    class FileExplorerStructureDeisgnModel : FileExplorerStructureViewModel
    {
        public static FileExplorerStructureDeisgnModel Instance => new FileExplorerStructureDeisgnModel();

        public FileExplorerStructureDeisgnModel()
        {
            Items = new ObservableCollection<FileExplorerItemViewModel>();
            var newItem = new FileExplorerItemViewModel("C:\\", FileExplorerItemType.Drive, "C:\\");
            newItem.Children = new ObservableCollection<FileExplorerItemViewModel>
            {
                new FileExplorerItemViewModel("C:\\Test Folder", FileExplorerItemType.Folder, "Test Folder"),
                new FileExplorerItemViewModel("C:\\Test Folder2", FileExplorerItemType.Folder, "Test Folder2"),
                new FileExplorerItemViewModel("C:\\Test File", FileExplorerItemType.File, "Test File")
            };
            newItem.IsExpanded = true;
            Items.Add(newItem);
            newItem = new FileExplorerItemViewModel("B:\\", FileExplorerItemType.Drive, "B:\\");
            newItem.Children = new ObservableCollection<FileExplorerItemViewModel>
            {
                new FileExplorerItemViewModel("B:\\Test Folder", FileExplorerItemType.Folder, "Test Folder"),
                new FileExplorerItemViewModel("B:\\Test Folder2", FileExplorerItemType.Folder, "Test Folder2"),
                new FileExplorerItemViewModel("B:\\Test File", FileExplorerItemType.File, "Test File"),
                new FileExplorerItemViewModel("B:\\Test File", FileExplorerItemType.File, "Test File"),
                new FileExplorerItemViewModel("B:\\Test File", FileExplorerItemType.File, "Test File"),
                new FileExplorerItemViewModel("B:\\Test File", FileExplorerItemType.File, "Test File"),
                new FileExplorerItemViewModel("B:\\Test File", FileExplorerItemType.File, "Test File")
            };
            newItem.IsExpanded = true;
            Items.Add(newItem);
            newItem = new FileExplorerItemViewModel("Folder\\", FileExplorerItemType.Drive, "Folder");
            newItem.Children = new ObservableCollection<FileExplorerItemViewModel>
            {
                new FileExplorerItemViewModel("Folder\\Test Folder", FileExplorerItemType.Folder, "Test Folder"),
                new FileExplorerItemViewModel("Folder\\Test Folder2", FileExplorerItemType.Folder, "Test Folder2"),
                new FileExplorerItemViewModel("Folder\\Test File", FileExplorerItemType.File, "Test File")
            };
            newItem.IsExpanded = true;
            Items.Add(newItem);
        }
    }
}
