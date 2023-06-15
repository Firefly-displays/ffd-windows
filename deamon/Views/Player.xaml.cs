using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using deamon.Models;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Timer = System.Threading.Timer;

namespace deamon;

// using WpfScreenHelper;

// using AudioSwitcher.AudioApi.CoreAudio;

public partial class Player : Window
{
    public Player(Display display)
    {
        
        // if (DataContext != null)
        // {
        //     (DataContext as PlayerDataContext)!.CurrentQueues.CollectionChanged += (sender, args) =>
        //     {
        //         Dispatcher.Invoke(() =>
        //         {
        //             // var queue = (DataContext as PlayerDataContext).CurrentQueues[0];
        //             // var name = queue.Queue.Name;
        //             var contextString = JsonConvert.SerializeObject(DataContext);
        //             TextBlock.Text = contextString;
        //             Debug.WriteLine("CurrentQueues changed");
        //         });
        //     };
        // }

        InitializeComponent();
        
        WindowState = WindowState.Normal;
        // Left = screen.WorkingArea.Left;
        // Top = screen.WorkingArea.Top;
        // Width = screen.WorkingArea.Width;
        // Height = screen.WorkingArea.Height;
        
        Left = display.Bounds[1];
        Top = display.Bounds[0];
        Width = 500;
        Height = 300;
        // Width = display.Bounds[2];
        // Height = display.Bounds[3];

        // var controller = new CoreAudioController();
        // controller.SetDefaultDevice(audioDevice);

        // VideoElement.LoadedBehavior = MediaState.Manual;
        // VideoElement.MediaEnded += (o, a) => Play();
    }

    // public Player(SchedulerModel schedulerModel)
    // {
    //     this.SchedulerModel = schedulerModel;
    //     this.DataContext = schedulerModel;
    //     InitializeComponent();
    // }
    //
    // public SchedulerModel SchedulerModel { get; set; }
}

public class SomeConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Debug.WriteLine("SomeConverter");
        return (value as IEnumerable<QueueWithPriority>).Select(el => JsonConvert.SerializeObject(el));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}