using System.Windows;

namespace Bitclout
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                (DataContext as MainWindowViewModel).chromeWorker.BitcloutChromeDriver.Quit();
            }
            catch (System.Exception)
            {
            }
        }
    }
}
