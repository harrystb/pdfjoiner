using pdfjoiner.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace pdfjoiner.DesktopClient
{
    public class DirectoryItemViewModel : BaseViewModel
    {
        #region Constructor
        public DirectoryItemViewModel(string fullpath, DirectoryItemType type, string name)
        {
            FullPath = fullpath;
            Type = type;
            Name = name;

            //only load in the document when it is added to the 
            Document = null;

            HasPdfExtension = DirectoryHelpers.HasPdfFileExtension(FullPath);

            ClearChildren();

            ExpandCommand = new RelayCommand(Expand);
        }
        #endregion

        #region Properties

        private readonly bool HasPdfExtension;

        private DocumentModel _Document;
        public DocumentModel Document {
            get
            {
                if (_Document == null && HasPdfExtension)
                    _Document = new DocumentModel(FullPath);
                return _Document;
            }
            set
            {
                _Document = value;
            } 
        }

        private DirectoryItemType _Type;
        public DirectoryItemType Type 
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
                if (Type == DirectoryItemType.Drive)
                {
                    return "drive";
                }
                else if (Type == DirectoryItemType.Folder)
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

        private ObservableCollection<DirectoryItemViewModel> _Children;
        public ObservableCollection<DirectoryItemViewModel> Children 
        {
            get => _Children;
            set => SetProperty(ref _Children, value);
        }

        public bool CanExpand { get { return Type != DirectoryItemType.File; } }

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
            Children = new ObservableCollection<DirectoryItemViewModel>();

            // Put in a dummy child for expand arror
            if (Type != DirectoryItemType.File)
                Children.Add(null);
        }

        private void Expand(object p)
        {
            if (Type == DirectoryItemType.File)
                return;

            //Get the children

            var newChildren = DirectoryHelpers.GetFolderContents(FullPath);
            Children = new ObservableCollection<DirectoryItemViewModel>(
                newChildren.Select(child => new DirectoryItemViewModel(child.FullPath, child.Type, child.Name)));

        }
        #endregion

        #region Commands

        public ICommand ExpandCommand { get; set; }
        

        #endregion
    }
}
