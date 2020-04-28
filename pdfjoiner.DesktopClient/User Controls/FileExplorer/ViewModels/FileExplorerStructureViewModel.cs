using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace pdfjoiner.DesktopClient.UserControls.ViewModels
{
    public class FileExplorerStructureViewModel : BaseViewModel
    {

        #region Properties

        private ObservableCollection<FileExplorerItemViewModel> _Items;
        public ObservableCollection<FileExplorerItemViewModel> Items {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }

        #endregion

        #region Constructor

        public FileExplorerStructureViewModel()
        {
            var children = FileExplorerHelpers.GetLogicalDrives();

            Items = new ObservableCollection<FileExplorerItemViewModel>(children.Select(child => new FileExplorerItemViewModel(child.FullPath, child.Type, child.Name)));
        }

        #endregion

    }
}
