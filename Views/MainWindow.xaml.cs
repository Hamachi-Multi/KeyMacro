using SimpleKeyMacro.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gma.System.MouseKeyHook;
using SimpleKeyMacro.Services;

namespace SimpleKeyMacro.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            DataContext = mainViewModel;
            InitializeComponent();
        }

        private void MacroKeyTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                string pressedKey = e.Key.ToString();

                viewModel.KeyStrokeText = pressedKey;
            }

            e.Handled = true;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is UIElement clickedElement && clickedElement.GetType() != typeof(System.Windows.Controls.TextBox))
            {
                FocusableTextBox.Focus();
            }
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            FocusableTextBox.Focus();
        }
    }
}
