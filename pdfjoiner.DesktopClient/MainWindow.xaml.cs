using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
        private void LessThanMaxPage(object sender, TextCompositionEventArgs e)
        {
            //by default ignore the character
            e.Handled = true;
            if (_NonnumericRegex.IsMatch(e.Text))
                return;

            int pagenumber;
            int max = ((MainViewModel)DataContext).GetMaxSelectedPageNumber();
            if (max == -1)
            {
                ((TextBox)sender).Text = "";
                return;
            }
            if (int.TryParse(((TextBox)sender).Text + e.Text,out pagenumber) && pagenumber > max)
            {
                ((TextBox)sender).Text = max.ToString();
                return;
            }
            //Only allow it to be added if it has passed all the above checks
            e.Handled = false;
        }

        public void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((MainViewModel)DataContext).SelectedItemChangedEventHandler(sender, e);
        }
        #endregion
    }
}
