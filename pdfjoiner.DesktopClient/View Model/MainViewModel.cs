#nullable enable
using Microsoft.Win32;
using pdfjoiner.Core.Generator;
using pdfjoiner.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace pdfjoiner.DesktopClient
{
    /// <summary>
    /// View model class for the main view of PDFJoiner.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {

        #region Default Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainViewModel()
        {
            _Items = new ObservableCollection<DirectoryItemViewModel>();
            _DocumentSegments = new ObservableCollection<DocumentSegmentModel>();

            //register all of the button functions as delegate commands
            _GenerateDocument = new DelegateCommand(OnGenerateDocumentButton);
            _AddDocument = new DelegateCommand(OnAddDocumentButton);
            _AddBrowseDocument = new DelegateCommand(OnBrowseFileButton);
            _AddBrowseFolder = new DelegateCommand(OnBrowseFolderButton);
            _AddPages = new DelegateCommand(OnAddPagesButton);
            _ResetDocumentList = new DelegateCommand(OnResetFormButton);

        }

        #endregion

        #region Properties

        private string _FilenameText = string.Empty;
        /// <summary>
        /// Currently Selected Filename
        /// </summary>
        public string FilenameText
        {
            get => _FilenameText;
            set => SetProperty(ref _FilenameText, value);
        }


        private string _PathText = string.Empty;
        /// <summary>
        /// Path to the currently selected file
        /// </summary>
        public string PathText
        {
            get => _PathText;
            set => SetProperty(ref _PathText, value);
        }

        private string _NumPagesText = string.Empty;
        /// <summary>
        /// Number of pages in the currently selected file
        /// </summary>
        public string NumPagesText
        {
            get => _NumPagesText;
            set => SetProperty(ref _NumPagesText, value);
        }


        private string _StartPageText = string.Empty;
        /// <summary>
        /// The bound property for the start index of the document segment to be added.
        public string StartPageText
        {
            get => _StartPageText;
            set => SetProperty(ref _StartPageText, value);
        }

        private string _EndPageText = string.Empty;
        /// <summary>
        /// The bound property for the end index of the document segment to be added.
        /// </summary>
        public string EndPageText
        {
            get => _EndPageText;
            set => SetProperty(ref _EndPageText, value);
        }


        private string _StatusText = string.Empty;
        /// <summary>
        /// Status text which is to be displayed to the user.
        /// </summary>
        public string StatusText
        {
            get => _StatusText;
            set => SetProperty(ref _StatusText, value);
        }



        private Brush _StatusBrush = (Brush)new BrushConverter().ConvertFromString("Green");
        public Brush StatusBrush
        {
            get
            {
                return _StatusBrush;
            }
            set
            {
                SetProperty(ref _StatusBrush, value);
            }
        }


        private ObservableCollection<DocumentSegmentModel> _DocumentSegments;
        public ObservableCollection<DocumentSegmentModel> DocumentSegments
        {
            get => _DocumentSegments;
            set => SetProperty(ref _DocumentSegments, value);
        }

        private ObservableCollection<DirectoryItemViewModel> _Items;
        public ObservableCollection<DirectoryItemViewModel> Items
        {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }

        private DirectoryItemViewModel? _SelectedItem;
        public DirectoryItemViewModel? SelectedItem
        {
            get => _SelectedItem;

            set
            {
                if (SelectedItem == value)
                    return;
                //unselect the previous
                if (_SelectedItem != null)
                    _SelectedItem.IsSelected = false;
                //set the selected document before trying to use it
                SetProperty(ref _SelectedItem, value);
                //Set the new one as selected as selected
                if (_SelectedItem != null)
                    _SelectedItem.IsSelected = true;
                //Clear the index text boxes
                StartPageText = "";
                EndPageText = "";
                //if the selected document is empty, clear out document panel
                if (_SelectedItem == null || _SelectedItem.Document == null)
                {
                    FilenameText = "";
                    PathText = "";
                    NumPagesText = "";
                    return;
                }
                //update the text boxes with the document information.
                FilenameText = _SelectedItem.Name;
                PathText = _SelectedItem.FullPath;
                NumPagesText = _SelectedItem.Document.NumPages.ToString();
            }
        }


        private bool _IsDocumentSelected = false;
        public bool IsDocumentSelected
        {
            get => _IsDocumentSelected;
            set => SetProperty(ref _IsDocumentSelected, value);
        }

        #endregion

        #region Methods

        private void SetStatusTextboxContent(string newStatus, string colour)
        {
            StatusText = newStatus;
            if (colour == "Green")
            {
                StatusBrush = (Brush)new BrushConverter().ConvertFromString("Green");
            }
            else if (colour == "Red")
            {
                StatusBrush = (Brush)new BrushConverter().ConvertFromString("Red");
            }
            else if (colour == "Orange")
            {
                StatusBrush = (Brush)new BrushConverter().ConvertFromString("Orange");
            }
            else
            {
                StatusBrush = (Brush)new BrushConverter().ConvertFromString("Grey");
            }
        }

        /// <summary>
        /// Event to start the document generation process
        /// </summary>
        public ICommand GenerateDocument => _GenerateDocument;
        private readonly DelegateCommand _GenerateDocument;
        private void OnGenerateDocumentButton(object commandParameter)
        {
            if (DocumentSegments.Count == 0)
            {
                SetStatusTextboxContent("No documents segments selected.", "Yellow");
                return;
            }
            var generator = new PdfGenerator(DocumentSegments.AsEnumerable());
            generator.GenerateDocument();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files | *.pdf",
                Title = "Save the PDF File"
            };
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                generator.SaveGeneratedDocument(saveFileDialog.FileName);
                SetStatusTextboxContent("Document generated successfully.", "Green");
            }
        }

        /// <summary>
        /// Event to add a document to the list
        /// </summary>
        public ICommand AddBrowseDocument => _AddBrowseDocument;
        private readonly DelegateCommand _AddBrowseDocument;
        private void OnBrowseFileButton(object commandParameter)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog
            {
                CheckPathExists = false,
                CheckFileExists = false,
                ValidateNames = false,
                Title = "Select a PDF file or folder.",
                FileName = "\r",
                Multiselect = true
            };

            folderBrowser.ShowDialog();
            if (folderBrowser.FileNames.Length == 0)
            {
                SetStatusTextboxContent("No file selected.", "Orange");
                return;
            }
            foreach (var name in folderBrowser.FileNames)
            {
                var newItem = CreateDirectoryItemViewModelFromPath(name);
                if (newItem == null)
                    return;
                AddDirectoryItem(newItem);
                SelectedItem = newItem;
            }
            SetStatusTextboxContent("Document sucessfully added.", "Green");
        }

        /// <summary>
        /// Event to add a document to the list
        /// </summary>
        public ICommand AddBrowseFolder => _AddBrowseFolder;
        private readonly DelegateCommand _AddBrowseFolder;
        private void OnBrowseFolderButton(object commandParameter)
        {

            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder to add."
            };

            folderBrowser.ShowDialog();
            if (string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                SetStatusTextboxContent("No file selected.", "Orange");
                return;
            }
            var newItem = CreateDirectoryItemViewModelFromPath(folderBrowser.SelectedPath);
            if (newItem == null)
            {
                SetStatusTextboxContent("Error adding the folder.", "Red");
                return;
            }
            AddDirectoryItem(newItem);
            SelectedItem = newItem;
            SetStatusTextboxContent("Folder sucessfully added.", "Green");
        }

        private DirectoryItemViewModel? CreateDirectoryItemViewModelFromPath(string path)
        {
            try
            {
                FileAttributes fileAttributes = File.GetAttributes(path);
                return new DirectoryItemViewModel(path,
                    fileAttributes.HasFlag(FileAttributes.Directory) ? DirectoryItemType.Folder : DirectoryItemType.File,
                    DirectoryHelpers.GetFileFolderName(path));
            }
            catch (ArgumentException)
            {
                MessageBox.Show("File or Folder name is invalid.", "Error");
                return null;
            }
            catch (PathTooLongException)
            {
                MessageBox.Show("File or Folder path is too long.", "Error");
                return null;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show("File or Folder name is invalid.", "Error");
                return null;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File or Folder not found.", "Error");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("File or Folder not found.", "Error");
                return null;
            }
            catch (IOException)
            {
                MessageBox.Show("File in use by another process.", "Error");
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Not authorised to access file or folder.", "Error");
                return null;
            }
        }

        private void AddDirectoryItem(DirectoryItemViewModel newDirectoryItem)
        {
            //Don't add if it is null
            if (newDirectoryItem == null)
                return;

            Items.Add(newDirectoryItem);
            SelectedItem = newDirectoryItem;
        }

        /// <summary>
        /// Event to add a document to the list
        /// </summary>
        public ICommand AddDocument => _AddDocument;
        private readonly DelegateCommand _AddDocument;
        private void OnAddDocumentButton(object commandParameter)
        {
            throw new NotImplementedException();
        }

        public void AddMultipleDocuments(string[] files)
        {
            foreach (var file in files)
            {
                var newItem = CreateDirectoryItemViewModelFromPath(file);
                if (newItem != null)
                {
                    AddDirectoryItem(newItem);
                    SelectedItem = newItem;
                }
            }
            SetStatusTextboxContent("Documents sucessfully added.", "Green");
        }

        /// <summary>
        /// Event Handler for the treeview selectionchanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectedItemChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = ((DirectoryItemViewModel)e.NewValue);
            IsDocumentSelected = SelectedItem.Document != null;
        }
        /// <summary>
        /// Event to add a group of pages to the generation string.
        /// </summary>
        public ICommand AddPages => _AddPages;
        private readonly DelegateCommand _AddPages;
        private void OnAddPagesButton(object commandParameter)
        {
            int startIndex;
            int endIndex;
            try
            {
                //index = page number - 1
                startIndex = int.Parse(StartPageText) - 1;
                endIndex = int.Parse(EndPageText) - 1;
            }
            catch
            {
                SetStatusTextboxContent("invalid page numbers provided", "Red");
                return;
            }
            //Make sure there is a document selected
            if (SelectedItem == null || SelectedItem.Document == null)
                return;
            var newSegment = new DocumentSegmentModel(SelectedItem.Document, startIndex, endIndex);
            DocumentSegments.Add(newSegment);

            SetStatusTextboxContent("Document segment added.", "Green");

        }

        /// <summary>
        /// Event to reset all fieds.
        /// </summary>
        public ICommand ResetDocumentList => _ResetDocumentList;
        private readonly DelegateCommand _ResetDocumentList;
        private void OnResetFormButton(object commandParameter)
        {
            //Clear the document lists
            DocumentSegments.Clear();
            //Reset the selected document panel
            SelectedItem = null;
        }

        #endregion

        #region Helpers

        #endregion

    }
}
