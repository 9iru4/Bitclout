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
                (DataContext as MainWindowViewModel).chromeWorker.DiamondChromeDriver.Quit();
            }
            catch (System.Exception)
            {
            }
            try
            {
                (DataContext as MainWindowViewModel).chromeWorker.PostChromeDriver.Quit();
            }
            catch (System.Exception)
            {
            }
        }
    }
}
