using System;
using System.Windows;

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
        }

        // private async void Experiment()
        // {
        //     ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
        //     IScheduler scheduler = await schedulerFactory.GetScheduler();
        //     SchedulerModel schedulerModel = new SchedulerModel(scheduler);
        //     
        //     Player player = new Player(schedulerModel);
        //     player.Show();
        // }
        
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
    }
}