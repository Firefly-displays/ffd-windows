using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using deamon.Models;
using deamon.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using Application = System.Windows.Application;

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
            try
            {
                InitializeComponent();
                Hide();
                NotifyIcon.Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icon.ico"));

                _bw = BackgroundWorker.GetInstance();
                
                var creds = File.ReadAllText(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "credentials.txt"))
                    .Split("\r\n");
                
                HostId.Header = "ID: " + creds[0];
                HostPassword.Header = "Пароль: " + creds[1];
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
            }
            
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Hidden;
            _bw.Stop();
            Close();
            Application.Current.Shutdown();
        }
        
        private void Pause(object sender, RoutedEventArgs e)
        {
            var displayId = _bw.API.GET<Display>()[0].Id;
            _bw.API.PauseDisplay(displayId);
        }
        
        private void AddContent(object sender, RoutedEventArgs e)
        {
            AddContent dlg = new AddContent();
            dlg.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri){ UseShellExecute = true });
            e.Handled = true;
        }
    }
}