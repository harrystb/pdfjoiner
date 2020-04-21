#nullable enable
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using pdfjoiner.Core.Generator;
using pdfjoiner.Core.Models;

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
            _AddPages = new DelegateCommand(OnAddPagesButton);
            _CancelGeneration = new DelegateCommand(OnCancelGenerationButton);
            _ResetForm = new DelegateCommand(OnResetFormButton);

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

        private string _AddPageText = string.Empty;
        /// <summary>
        /// String for the pages which are to be added to the generation string.
        /// </summary>
        public string AddPageText
        {
            get => _AddPageText;
            set => SetProperty(ref _AddPageText, value);
        }

        private string _GenerationText = string.Empty;
        /// <summary>
        /// String for the pages which are to be added to the generation string.
        /// </summary>
        public string GenerationText
        {
            get => _GenerationText;
            set => SetProperty(ref _GenerationText, value);
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


        private ObservableCollection<DocumentModel> _DocumentList;
        private ObservableCollection<DocumentSegmentModel> DocumentSegments;

        public ObservableCollection<DocumentModel> DocumentList
        {
            get => _DocumentList;
            set => SetProperty(ref _DocumentList, value);
        }

        private DocumentModel? _SelectedDocument;
        public DocumentModel? SelectedDocument
        {
            get
            {
                return _SelectedDocument;
            }

            set
            {
                //set the selected document before trying to use it
                SetProperty(ref _SelectedDocument, value);
                //Clear the AddPageText field
                AddPageText = "";
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
                NumPagesText = _SelectedDocument.NumPages.ToString();
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
            SetStatusTextboxContent("Not working yet..", "Red");
        }

        /// <summary>
        /// Event to add a document to the list
        /// </summary>
        public ICommand AddDocument => _AddDocument;
        private readonly DelegateCommand _AddDocument;
        private void OnAddDocumentButton(object commandParameter)
        {
            OpenFileDialog FileDialog1 = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Select a PDF File"
            };
            FileDialog1.ShowDialog();
            if (FileDialog1.FileName == "")
            {
                SetStatusTextboxContent("No file selected.", "Orange");
                return;
            }
            var Document = new DocumentModel(FileDialog1.FileName);
            DocumentList.Add(Document);
            SelectedDocument = Document;
            SetStatusTextboxContent("Document sucessfully added.", "Green");
        }

        /// <summary>
        /// Event to add a group of pages to the generation string.
        /// </summary>
        public ICommand AddPages => _AddPages;
        private readonly DelegateCommand _AddPages;
        private void OnAddPagesButton(object commandParameter)
        {
            //Validate the page string
            if (!ValidateAddPageString())
            {
                SetStatusTextboxContent("Invalid character entered. Valid Example: 1,2-3,5-,-6", "Red");
                return;
            }

            //Get the document ID
            var id = "TODO";

            foreach (var segment in AddPageText.Split(','))
            {
                if (string.IsNullOrEmpty(GenerationText))
                    GenerationText = id + segment;
                else
                    GenerationText = $"{GenerationText},{id}{segment}";
            }
            SetStatusTextboxContent("Pages added to the generation string.", "Green");
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
        public ICommand ResetForm => _ResetForm;
        private readonly DelegateCommand _ResetForm;
        private void OnResetFormButton(object commandParameter)
        {
            //Clear the document lists
            DocumentList.Clear();
            DocumentSegments.Clear();
            //Reset the selected document panel
            SelectedDocument = null;
            //Reset the document generator
            GenerationText = "";
        }

        #endregion

        #region Helpers

        private bool ValidateAddPageString()
        {

            foreach (var segment in AddPageText.Split(','))
            {
                HashSet<char> AllowedChars = new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
                if (!segment.All(AllowedChars.Contains))
                    return false;
            }
            return true;
        }

        #endregion

    }
}
