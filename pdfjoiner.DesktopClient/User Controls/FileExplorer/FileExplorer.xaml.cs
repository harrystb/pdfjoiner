using pdfjoiner.DesktopClient.UserControls.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace pdfjoiner.DesktopClient.UserControls
{
    /// <summary>
    /// Interaction logic for FileExplorer.xaml
    /// </summary>
    public partial class FileExplorer : UserControl
    {
        public FileExplorer()
        {
            InitializeComponent();
            LayoutRoot.DataContext = new FileExplorerStructureViewModel();
        }

        /// <summary>
        /// The path that has been selected in the tree view
        /// </summary>
        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValuePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(FileExplorer), new PropertyMetadata(""));

        public void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedPath = ((FileExplorerItemViewModel)e.NewValue).FullPath;
        }
    }
}
