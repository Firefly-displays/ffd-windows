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
    public Player(Display display, PlayerController.ContentIsDoneHandler contentIsDone)
    {
        InitializeComponent();

        WindowState = WindowState.Normal;
        Left = display.Bounds[1];
        Top = display.Bounds[0];
        Width = 500;
        Height = 300;
        // Width = display.Bounds[2];
        // Height = display.Bounds[3];

        // var controller = new CoreAudioController();
        // controller.SetDefaultDevice(audioDevice);
        
        VideoElement.MediaEnded += (o, e) =>
        {
            contentIsDone.Invoke();
        };

        DataContextChanged += (o, e) =>
        {
            if (DataContext is PlayerController)
            {
                (DataContext as PlayerController)!.PropertyChanged += (o, a) =>
                {
                    if (a.PropertyName == "CurrentContentSrc")
                    {
                        // var content = ((DataContext as PlayerController)!).CurrentContent;
                        // TextBlock.Text = content.Path;
                        // Debug.WriteLine("Ставим новую пластинку - " + ((DataContext as PlayerController)!).CurrentContentSrc);
                        // Thread.Sleep(((DataContext as PlayerController)!).CurrentContentDuration * 1000);
                        // Debug.WriteLine("пластинку надо поменять");
                        // contentIsDone.Invoke();

                        // switch (content.Type)
                        // {
                        //     case Content.ContentType.Video:
                        //         VideoElement.Source = new Uri(content.Path);
                        //         VideoElement.Play();
                        //         break;
                        //     case Content.ContentType.Image:
                        //         ImageElement.Source = new BitmapImage(new Uri(content.Path));
                        //         break;
                        //     case Content.ContentType.Audio:
                        //         // AudioElement.Source = new Uri(content.Path);
                        //         // AudioElement.Play();
                        //         break;
                        // }
                    }
                };
            }
        };

        // VideoElement.LoadedBehavior = MediaState.Manual;
        // VideoElement.MediaEnded += (o, a) => Play();
    }
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