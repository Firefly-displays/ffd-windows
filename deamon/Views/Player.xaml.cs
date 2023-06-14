using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using deamon.Models;
using Application = System.Windows.Application;
using Timer = System.Threading.Timer;

namespace deamon;

// using WpfScreenHelper;

// using AudioSwitcher.AudioApi.CoreAudio;

public partial class Player : Window
{
    public Player(Display display, object dataContext)
    {
        InitializeComponent();
        
        WindowState = WindowState.Normal;
        // Left = screen.WorkingArea.Left;
        // Top = screen.WorkingArea.Top;
        // Width = screen.WorkingArea.Width;
        // Height = screen.WorkingArea.Height;
        
        Left = display.Bounds[1];
        Top = display.Bounds[0];
        Width = 100;
        Height = 100;
        // Width = display.Bounds[2];
        // Height = display.Bounds[3];

        DataContext = dataContext;

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