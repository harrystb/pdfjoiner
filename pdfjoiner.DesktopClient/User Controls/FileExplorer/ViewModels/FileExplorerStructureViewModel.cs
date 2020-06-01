using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

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

        private RelayCommand _AddPathCommand;
        public RelayCommand AddPathCommand
        { 
            get
            {
                return _AddPathCommand ?? (_AddPathCommand = 
                    new RelayCommand(AddPath));
            }
        }

        private void AddPath(object param)
        {
            if (!(param is string path))
                return;

            AddItemPath(path);
        }

        #endregion

        #region Constructor

        public FileExplorerStructureViewModel()
        {
            //var children = FileExplorerHelpers.GetLogicalDrives();
            //Items = new ObservableCollection<FileExplorerItemViewModel>(children.Select(child => new FileExplorerItemViewModel(child.FullPath, child.Type, child.Name)));

            Items = new ObservableCollection<FileExplorerItemViewModel>();

        }

        #endregion

        #region Public Methods

        public void AddItem(FileExplorerItemViewModel item)
        {
            Items.Add(item);
        }


        public void AddItemPath(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("Path provided does not exist.", nameof(path));

            FileAttributes attr = File.GetAttributes(path);
            FileExplorerItemType itemType;

            if (attr.HasFlag(FileAttributes.Directory))
                itemType = FileExplorerItemType.Folder;
            else
                itemType = FileExplorerItemType.File;

            Items.Add(new FileExplorerItemViewModel(path, itemType, FileExplorerHelpers.GetFileFolderName(path)));
        }

        #endregion

    }
}
