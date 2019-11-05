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
            _ChangeDocumentSelected = new DelegateCommand(OnDocumentSelectedChange);
            _GenerateDocument = new DelegateCommand(OnGenerateDocumentButton);
            _AddDocument = new DelegateCommand(OnAddDocumentButton);
            _AddPages = new DelegateCommand(OnAddPagesButton);
            _ShowTerminal = new DelegateCommand(OnShowTerminalButton);
            _CancelGeneration = new DelegateCommand(OnCancelGenerationButton);
            _ResetForm = new DelegateCommand(OnResetFormButton);
        }
        #endregion

        #region Properties
        private string _FilenameText;
        /// <summary>
        /// Currently Selected Filename
        /// </summary>
        public string FilenameText
        {
            get => _FilenameText;
            set => SetProperty(ref _FilenameText, value);
        }


        private string _PathText;
        /// <summary>
        /// Path to the currently selected file
        /// </summary>
        public string PathText
        {
            get => _PathText;
            set => SetProperty(ref _PathText, value);
        }

        private string _NumPagesText;
        /// <summary>
        /// Number of pages in the currently selected file
        /// </summary>
        public string NumPagesText
        {
            get => _NumPagesText;
            set => SetProperty(ref _NumPagesText, value);
        }

        private string _AddPageText;
        /// <summary>
        /// String for the pages which are to be added to the generation string.
        /// </summary>
        public string AddPageText
        {
            get => _AddPageText;
            set => SetProperty(ref _AddPageText, value);
        }

        private string _GenerationText;
        /// <summary>
        /// String for the pages which are to be added to the generation string.
        /// </summary>
        public string GenerationText
        {
            get => _GenerationText;
            set => SetProperty(ref _GenerationText, value);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Event call to indicate the selected document has been changed.
        /// </summary>
        public ICommand ChangeDocumentSelected => _ChangeDocumentSelected;
        private readonly DelegateCommand _ChangeDocumentSelected;
        private void OnDocumentSelectedChange(object commandParameter)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// Event to start the document generation process
        /// </summary>
        public ICommand GenerateDocument => _GenerateDocument;
        private readonly DelegateCommand _GenerateDocument;
        private void OnGenerateDocumentButton(object commandParameter)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Event to add a document to the list
        /// </summary>
        public ICommand AddDocument => _AddDocument;
        private readonly DelegateCommand _AddDocument;
        private void OnAddDocumentButton(object commandParameter)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Event to add a group of pages to the generation string.
        /// </summary>
        public ICommand AddPages => _AddPages;
        private readonly DelegateCommand _AddPages;
        private void OnAddPagesButton(object commandParameter)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Event to show/hide the terminal
        /// </summary>
        public ICommand ShowTerminal => _ShowTerminal;
        private readonly DelegateCommand _ShowTerminal;
        private void OnShowTerminalButton(object commandParameter)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Event to cancel the currently active generation
        /// </summary>
        public ICommand CancelGeneration => _CancelGeneration;
        private readonly DelegateCommand _CancelGeneration;
        private void OnCancelGenerationButton(object commandParameter)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Event to reset all fieds.
        /// </summary>
        public ICommand ResetForm => _ResetForm;
        private readonly DelegateCommand _ResetForm;
        private void OnResetFormButton(object commandParameter)
        {
            throw new System.NotImplementedException();
        }

        #endregion


    }
}
