using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace pdfjoiner.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _ViewModel;

        public MainWindow()
        {
            _ViewModel = new MainViewModel();
            DataContext = _ViewModel;

            InitializeComponent();


        }


        #region Handlers
        private void DragDropEventHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                _ViewModel.AddMultipleDocuments(files);
            }
        }
        private static readonly Regex _NonnumericRegex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            if (_NonnumericRegex.IsMatch(e.Text))
                e.Handled = true;

        }
        public void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((MainViewModel)DataContext).SelectedItemChangedEventHandler(sender, e);
        }
        #endregion
    }
}
