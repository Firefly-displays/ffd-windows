using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using deamon.Models;
using Hardcodet.Wpf.TaskbarNotification;
using Quartz;
using Quartz.Impl;

namespace deamon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Hide();
            NotifyIcon.Icon = new System.Drawing.Icon(@"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Icon.ico");
            
            Experiment();
        }

        private async void Experiment()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();
            SchedulerModel schedulerModel = new SchedulerModel(scheduler);
            
            Player player = new Player(schedulerModel);
            player.Show();
        }
        
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Hidden;
            Close();
        }

        private void OpenSettingClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}