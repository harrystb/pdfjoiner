#nullable enable
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pdfjoiner
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
            _GenerateDocument = new DelegateCommand(OnGenerateDocumentButton);
            _AddDocument = new DelegateCommand(OnAddDocumentButton);
            _AddPages = new DelegateCommand(OnAddPagesButton);
            _ShowTerminal = new DelegateCommand(OnShowTerminalButton);
            _CancelGeneration = new DelegateCommand(OnCancelGenerationButton);
            _ResetForm = new DelegateCommand(OnResetFormButton);
            DocGenerator = new DocumentGenerator();
            DocGenerator.SetStatusChangedCallback(StatusChanged);
            StatusText = "Welcome, please add a document to get started.";
        }
        #endregion

        #region Properties

        private readonly DocumentGenerator DocGenerator;

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


        private ObservableCollection<string> _DocumentItemList = new ObservableCollection<string>();
        public ObservableCollection<string> DocumentItemList
        {
            get => _DocumentItemList;
            set => SetProperty(ref _DocumentItemList, value);
        }

        private string _SelectedDocument = string.Empty;
        public string SelectedDocument
        { 
            get
            { 
                return _SelectedDocument;
            }

            set
            {
                //set the selected document before trying to use it
                _SelectedDocument = value;
                //get the ID of the document
                string id = _SelectedDocument.Split(':')[0];
                //Update the title, path and num pages based on the new selection
                DocumentItem? selectedDocument = DocGenerator.GetDocument(id);
                if (selectedDocument == null)
                {
                    MessageBox.Show("Unknown document selected. Suggest resetting the document list.");
                    return;
                }
                //update the text boxes with the document information.
                FilenameText = selectedDocument.Filename;
                PathText = selectedDocument.Path;
                NumPagesText = selectedDocument.NumberOfPages;
            }
        }

        #endregion

        #region Methods

        
        void StatusChanged(string newStatus)
        {
            StatusText = newStatus;
        }

        /// <summary>
        /// Event to start the document generation process
        /// </summary>
        public ICommand GenerateDocument => _GenerateDocument;
        private readonly DelegateCommand _GenerateDocument;
        private void OnGenerateDocumentButton(object commandParameter)
        {
            DocGenerator.Generate(GenerationText);
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
                StatusText = "No file selected.";
                return;
            }
            string? key = DocGenerator.AddDocumentToList(FileDialog1.FileName);
            if (string.IsNullOrEmpty(key))
            {
                StatusText = "Document is already in the list.";
                return;
            }
            string listText = key + ": " + DocGenerator.GetDocument(key)?.Filename ?? "Unknown";
            DocumentItemList.Add(listText);
            SelectedDocument = listText;
            StatusText = "Document sucessfully added.";
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
                return;
            }
            
            //Get the document ID
            var id = _SelectedDocument.Split(':')[0];

            foreach (var segment in AddPageText.Split(','))
            {
                if (string.IsNullOrEmpty(GenerationText))
                    GenerationText = id + segment;
                else
                    GenerationText = $"{GenerationText},{id}{segment}";
            }
        }

        /// <summary>
        /// Event to show/hide the terminal
        /// </summary>
        public ICommand ShowTerminal => _ShowTerminal;
        private readonly DelegateCommand _ShowTerminal;
        private void OnShowTerminalButton(object commandParameter)
        {
            DocGenerator.ToggleProcessWindowVisibility();
        }

        /// <summary>
        /// Event to cancel the currently active generation
        /// </summary>
        public ICommand CancelGeneration => _CancelGeneration;
        private readonly DelegateCommand _CancelGeneration;
        private void OnCancelGenerationButton(object commandParameter)
        {
            MessageBox.Show("Cancel Button not implemented.");
        }

        /// <summary>
        /// Event to reset all fieds.
        /// </summary>
        public ICommand ResetForm => _ResetForm;
        private readonly DelegateCommand _ResetForm;
        private void OnResetFormButton(object commandParameter)
        {
            MessageBox.Show("Reset Button not implemented.");
        }

        #endregion

        #region Helpers

        private bool ValidateAddPageString()
        {

            foreach (var segment in AddPageText.Split(','))
            {
                HashSet<char> AllowedChars = new HashSet<char> {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-'};
                if (!AddPageText.All(AllowedChars.Contains))
                    return false;
            }
            return true;
        }

        #endregion

    }
}
