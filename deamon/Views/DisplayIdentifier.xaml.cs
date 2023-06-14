using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using deamon.Models;

namespace deamon.Views;

public partial class DisplayIdentifier : Window
{
    DispatcherTimer timer;
    public DisplayIdentifier(Display display)
    {
        InitializeComponent();
        
        Text.Foreground = Brushes.White;
        Text.Text = display.Name;
        Text.FontSize = 48;
        
        WindowState = WindowState.Normal;
        Top = display.Bounds[0];
        Left = display.Bounds[1];
        Width = 600;
        Height = 100;
        
        WindowStyle = WindowStyle.None;
        
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        timer.Tick += CloseWindowHandler;
        timer.Start();
    }

    private void CloseWindowHandler(object? sender, EventArgs e)
    {
        timer.Stop();
        Close();
    }
}