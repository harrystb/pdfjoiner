using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace pdfjoiner.DesktopClient.UserControls.ViewModels
{
    public class FileExplorerItemViewModel : BaseViewModel
    {
        #region Constructor
        public FileExplorerItemViewModel(string fullpath, FileExplorerItemType type, string name)
        {
            FullPath = fullpath;
            Type = type;
            Name = name;

            ClearChildren();

            ExpandCommand = new RelayCommand(Expand);
        }
        #endregion

        #region Properties
        private FileExplorerItemType _Type;
        public FileExplorerItemType Type 
        {
            get => _Type;
            set => SetProperty(ref _Type, value);
        }

        private string _FullPath;
        public string FullPath
        {
            get => _FullPath;
            set => SetProperty(ref _FullPath, value);
        }

        public string ImageName
        {
            get
            {
                if (Type == FileExplorerItemType.Drive)
                {
                    return "drive";
                }
                else if (Type == FileExplorerItemType.Folder)
                {
                    if (IsExpanded)
                        return "folder-open";
                    else
                        return "folder-closed";
                }
                else
                {
                    return "file";
                }
            }
        }

        private string _Name;
        public string Name 
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        private ObservableCollection<FileExplorerItemViewModel> _Children;
        public ObservableCollection<FileExplorerItemViewModel> Children 
        {
            get => _Children;
            set => SetProperty(ref _Children, value);
        }

        public bool CanExpand { get { return Type != FileExplorerItemType.File; } }

        public bool IsExpanded
        {
            get => Children?.Count(f => f != null) > 0;
            set 
            {
                if (value == true)
                {
                    Expand(null);
                    SendPropertyChangedEvent(nameof(ImageName));
                }
                else
                {
                    ClearChildren();
                    SendPropertyChangedEvent(nameof(ImageName));
                }
            }
        }
        #endregion

        #region Methods

        private void ClearChildren()
        {
            Children = new ObservableCollection<FileExplorerItemViewModel>();

            // Put in a dummy child for expand arror
            if (Type != FileExplorerItemType.File)
                Children.Add(null);
        }

        private void Expand(object p)
        {
            if (Type == FileExplorerItemType.File)
                return;

            //Get the children

            var newChildren = FileExplorerHelpers.GetFolderContents(FullPath);
            Children = new ObservableCollection<FileExplorerItemViewModel>(
                newChildren.Select(child => new FileExplorerItemViewModel(child.FullPath, child.Type, child.Name)));

        }
        #endregion

        #region Commands

        public ICommand ExpandCommand { get; set; }
        

        #endregion
    }
}
