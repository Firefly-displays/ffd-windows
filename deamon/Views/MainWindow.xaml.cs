using System;
using System.Windows;
using deamon.Models;

namespace deamon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker _bw;

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            NotifyIcon.Icon = new System.Drawing.Icon(@"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Icon.ico");

            _bw = new BackgroundWorker();

            // var mvm = new MyViewModel();
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Hidden;
            _bw.Stop();
            Close();
        }

        private void RunClient(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        
        private void Pause(object sender, RoutedEventArgs e)
        {
            var displayId = _bw.API.GET<Display>()[0].Id;
            _bw.API.PauseDisplay(displayId);
        }
        
        private void Resume(object sender, RoutedEventArgs e)
        {
            var displayId = _bw.API.GET<Display>()[0].Id;
            _bw.API.ResumeDisplay(displayId);
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}