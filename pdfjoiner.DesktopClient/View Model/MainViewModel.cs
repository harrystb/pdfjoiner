#nullable enable
using pdfjoiner.Core.Generator;
using pdfjoiner.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
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
            _AddBrowseDocument = new DelegateCommand(OnBrowseFileButton);
            _AddBrowseFolder = new DelegateCommand(OnBrowseFolderButton);
            _AddPages = new DelegateCommand(OnAddPagesButton);
            _ClearDocumentList = new DelegateCommand(OnClearDocumentListButton);
            _ClearSegmentList = new DelegateCommand(OnClearSegmentListButton);
            _DeleteSegment = new DelegateCommand(OnDeleteSegmentButton);
            _PDFIsInvalid = false;
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


        private string _StartHintText = string.Empty;
        /// <summary>
        /// The hint text that is shown in the from page number text box
        /// </summary>
        public string StartHintText
        {
            get => _StartHintText;
            set => SetProperty(ref _StartHintText, value);
        }

        /// <summary>
        /// The hint text that is shown in the to page number text box
        /// </summary>
        private string _EndHintText = string.Empty;
        public string EndHintText
        {
            get => _EndHintText;
            set => SetProperty(ref _EndHintText, value);
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
                //update the text boxes with the document information.
                FilenameText = _SelectedItem?.Name ?? "";
                PathText = _SelectedItem?.FullPath ?? "";
                NumPagesText = _SelectedItem?.Document?.NumPages.ToString() ?? "";
                IsDocumentSelected = _SelectedItem?.Document != null;
                StartHintText = _SelectedItem?.Document != null ? "1" : "";
                EndHintText = _SelectedItem?.Document?.NumPages.ToString() ?? "";
                PDFIsInvalid = _SelectedItem?.IsAnInvalidPdf ?? false;
            }
        }

        private bool _PDFIsInvalid;
        public bool PDFIsInvalid
        {
            get => _PDFIsInvalid;
            set => SetProperty(ref _PDFIsInvalid, value);
        }

        private bool _IsDocumentSelected = false;
        public bool IsDocumentSelected
        {
            get => _IsDocumentSelected;
            set => SetProperty(ref _IsDocumentSelected, value);
        }
 
        private bool _HasDocumentSegments = false;
        public bool HasDocumentSegments
        {
            get => _HasDocumentSegments;
            set => SetProperty(ref _HasDocumentSegments, value);
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
            //if cancelled then don't continue
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

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

            //Do nothing if the dialog box is cancelled
            if (folderBrowser.ShowDialog() != DialogResult.OK)
                return;

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

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog
            {
                Description = "Select a folder to add."
            };

            //If the dialog was cancelled then cancel
            if (folderBrowser.ShowDialog() != DialogResult.OK)
                return;

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
                ShowMessage("File or Folder name is invalid.", "Error");
                return null;
            }
            catch (PathTooLongException)
            {
                ShowMessage("File or Folder path is too long.", "Error");
                return null;
            }
            catch (NotSupportedException)
            {
                ShowMessage("File or Folder name is invalid.", "Error");
                return null;
            }
            catch (FileNotFoundException)
            {
                ShowMessage("File or Folder not found.", "Error");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                ShowMessage("File or Folder not found.", "Error");
                return null;
            }
            catch (IOException)
            {
                ShowMessage("File in use by another process.", "Error");
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                ShowMessage("Not authorised to access file or folder.", "Error");
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
        }
        /// <summary>
        /// Event to add a group of pages to the generation string.
        /// </summary>
        public ICommand AddPages => _AddPages;
        private readonly DelegateCommand _AddPages;
        private void OnAddPagesButton(object commandParameter)
        {
            //Make sure there is a document selected
            if (SelectedItem == null || SelectedItem.Document == null)
                return;
            int startIndex;
            int endIndex;
            // Add whole document if page strings are left empty
            if (StartPageText == string.Empty)
            {
                startIndex = 0;
            } else
            {
                if (!int.TryParse(StartPageText, out startIndex))
                {
                    ShowMessage("Please provide a valid start page.", "Error");
                    return;
                }
                //convert page numbers into page index
                startIndex -= 1;
            }
            if (EndPageText == string.Empty)
            {
                endIndex = SelectedItem.Document.LastPageIndex;
            }
            else
            {
                if (!int.TryParse(EndPageText, out endIndex))
                {
                    ShowMessage("Please provide a valid end page.", "Error");
                    return;
                }
                //convert page numbers into page index
                endIndex -= 1;
            }
            var newSegment = new DocumentSegmentModel(SelectedItem.Document, startIndex, endIndex);
            DocumentSegments.Add(newSegment);
            HasDocumentSegments = DocumentSegments?.Count > 0;
        }

        /// <summary>
        /// Event to clear the document list.
        /// </summary>
        public ICommand ClearDocumentList => _ClearDocumentList;
        private readonly DelegateCommand _ClearDocumentList;
        private void OnClearDocumentListButton(object commandParameter)
        {
            //Clear the document list
            Items.Clear();
            SelectedItem = null;
            //Clear the segment lists
            DocumentSegments.Clear();
            HasDocumentSegments = DocumentSegments?.Count > 0;
        }

        /// <summary>
        /// Event to clear the segment list.
        /// </summary>
        public ICommand ClearSegmentList => _ClearSegmentList;
        private readonly DelegateCommand _ClearSegmentList;
        private void OnClearSegmentListButton(object commandParameter)
        {
            //Clear the segment lists
            DocumentSegments.Clear();
            HasDocumentSegments = DocumentSegments?.Count > 0;
        }

        /// <summary>
        /// Event to delete a document segment
        /// </summary>
        public ICommand DeleteSegment => _DeleteSegment;
        private readonly DelegateCommand _DeleteSegment;
        private void OnDeleteSegmentButton(object segment)
        {
            //Delete the segment
            if (segment is DocumentSegmentModel segmentModel)
                DocumentSegments.Remove(segmentModel);

        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the last indext of the selected document or -1 if there is no valid document selected.
        /// </summary>
        /// <returns>Last index or -1</returns>
        public int GetMaxSelectedPageNumber()
        {
            //Return the page number of the last item (10 pages -> page 10 is that last one at index 9)
            return SelectedItem?.Document?.NumPages ?? -1;
        }

        private void ShowMessage(string message, string caption)
        {
            System.Windows.MessageBox.Show(message, caption);
        }

        #endregion

    }
}
