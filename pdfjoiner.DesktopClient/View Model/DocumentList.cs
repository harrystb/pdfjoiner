using System.Windows.Input;

namespace pdfjoiner
{
    public class Test : BaseViewModel
    {
        private string textTest;
        public string TextTest {
            get => textTest; 
            set => SetProperty(ref textTest, value);
        }


        private readonly DelegateCommand _changeTextTestCommand;
        public ICommand ChangeTextTestCommand => _changeTextTestCommand;

        public Test()
        {
            _changeTextTestCommand = new DelegateCommand(OnChangeTextTest);
        }

        private void OnChangeTextTest(object commandParameter)
        {
            TextTest = "Changed :)";
        }
    }
}
