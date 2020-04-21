using pdfjoiner.DesktopClient.UserControls.ViewModels;
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
            DataContext = new FileExplorerStructureViewModel();
        }
    }
}
