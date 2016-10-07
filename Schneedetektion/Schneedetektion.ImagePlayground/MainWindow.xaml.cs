using System.Windows;
using System.Windows.Input;

namespace Schneedetektion.ImagePlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            recordCategories.HandleKey(e);
        }
    }
}
