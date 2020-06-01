#nullable enable
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using pdfjoiner.Core.Generator;
using pdfjoiner.Core.Models;
using System.Windows;
using System.Collections.Specialized;
using System;
using System.Windows.Navigation;

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
            DocumentList = new ObservableCollection<DocumentModel>();
            DocumentSegments = new ObservableCollection<DocumentSegmentModel>();

            //register all of the button functions as delegate commands
            _GenerateDocument = new DelegateCommand(OnGenerateDocumentButton);
            _AddDocument = new DelegateCommand(OnAddDocumentButton);
            _AddBrowseDocument = new DelegateCommand(OnBrowseButton);
            _AddPages = new DelegateCommand(OnAddPagesButton);
            _CancelGeneration = new DelegateCommand(OnCancelGenerationButton);
            _ResetDocumentList = new DelegateCommand(OnResetFormButton);

        }

        private void DocumentList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
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


        private string _SelectedDocumentPath = string.Empty;
        /// <summary>
        /// The path selected by the explorer control.
        /// </summary>
        public string SelectedDocumentPath
        {
            get => _SelectedDocumentPath;
            set => SetProperty(ref _SelectedDocumentPath, value);
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
        private ObservableCollection<DocumentModel> _DocumentList;

        public ObservableCollection<DocumentModel> DocumentList
        {
            get => _DocumentList;
            set => SetProperty(ref _DocumentList, value);
        }

        private ObservableCollection<DirectoryItemViewModel> _Items;
        public ObservableCollection<DirectoryItemViewModel> Items
        {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }

        private DirectoryItemViewModel? _SelectedDocument;
        public DirectoryItemViewModel? SelectedDocument
        {
            get => _SelectedDocument;

            set
            {
                //set the selected document before trying to use it
                SetProperty(ref _SelectedDocument, value);
                //Clear the index text boxes
                StartPageText = "";
                EndPageText = "";
                //if the selected document is empty, clear out document panel
                if (_SelectedDocument == null)
                {
                    FilenameText = "";
                    PathText = "";
                    NumPagesText = "";
                    return;
                }
                //update the text boxes with the document information.
                FilenameText = _SelectedDocument.Name;
                PathText = _SelectedDocument.FullPath;
                NumPagesText = _SelectedDocument.Document.NumPages.ToString();
            }
        }



        #endregion

        #region Methods

        private void SetStatusTextboxContent(string newStatus, string colour)
        {
            StatusText = newStatus;
            if (colour == "Green")
            {
                    StatusBrush = (Brush)new BrushConverter().ConvertFromString("Green");
            } else if (colour == "Red")
            {
                    StatusBrush = (Brush)new BrushConverter().ConvertFromString("Red");
            } else if (colour == "Orange")
            {
                    StatusBrush = (Brush)new BrushConverter().ConvertFromString("Orange");
            } else
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
        private void OnBrowseButton(object commandParameter)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

            OpenFileDialog FileDialog1 = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Select a PDF File or a folder",
                Multiselect = true,
                
            };
            FileDialog1.ShowDialog();
            if (FileDialog1.FileName == "")
            {
                SetStatusTextboxContent("No file selected.", "Orange");
                return;
            }
            var a = FileDialog1.
            var newDirectoryItem = new DirectoryItemViewModel()
            var Document = new DocumentModel(FileDialog1.FileName);
            DocumentList.Add(Document);
            SelectedDocument = Document;
            SetStatusTextboxContent("Document sucessfully added.", "Green");
        }
        /// <summary>
        /// Event to add a document to the list
        /// </summary>
        public ICommand AddDocument => _AddDocument;
        private readonly DelegateCommand _AddDocument;
        private void OnAddDocumentButton(object commandParameter)
        {
            DocumentModel Document = null;
            try
            {
                Document = new DocumentModel(SelectedDocumentPath);
            }
            catch
            {
                SetStatusTextboxContent("Failed to open the selected document. Is it a PDF?.", "Red");
                return;
            }
            DocumentList.Add(Document);
            SelectedDocument = Document;
            SetStatusTextboxContent("Document sucessfully added.", "Green");
        }

        public void AddMultipleDocuments(string[] files)
        {
            foreach (var file in files)
            {
                var Document = new DocumentModel(file);
                DocumentList.Add(Document);
                SelectedDocument = Document;
            }
            SetStatusTextboxContent("Documents sucessfully added.", "Green");
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
            var newSegment = new DocumentSegmentModel(SelectedDocument, startIndex, endIndex);
            DocumentSegments.Add(newSegment);

            SetStatusTextboxContent("Document segment added.", "Green");

        }

        /// <summary>
        /// Event to cancel the currently active generation
        /// </summary>
        public ICommand CancelGeneration => _CancelGeneration;
        private readonly DelegateCommand _CancelGeneration;
        private void OnCancelGenerationButton(object commandParameter)
        {
            //Tell the document generator to stop
            SetStatusTextboxContent("Not implemented yet.", "Red");
            //set status text based on whether the termination was successful
            //if (success)
            //{
            //    SetStatusTextboxContent("Document Generation stopped.", "Green");
            //    return;
            //}
            //SetStatusTextboxContent("Document Generation failed to stop.", DocumentGenerator.StatusColourState.Red);
        }

        /// <summary>
        /// Event to reset all fieds.
        /// </summary>
        public ICommand ResetDocumentList => _ResetDocumentList;
        private readonly DelegateCommand _ResetDocumentList;
        private void OnResetFormButton(object commandParameter)
        {
            //Clear the document lists
            DocumentList.Clear();
            DocumentSegments.Clear();
            //Reset the selected document panel
            SelectedDocument = null;
        }

        #endregion

        #region Helpers

        #endregion

    }
}
