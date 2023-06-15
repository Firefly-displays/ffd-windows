using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Threading;
using deamon.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Queue = deamon.Models.Queue;

namespace deamon;

[DisallowConcurrentExecution]
public sealed partial class PlayerController: INotifyPropertyChanged
{
    public PlayerController (Display display, SchedulerConfig? schedulerConfig)
    {
        Display = display;
        State = PlayerState.Playing;
        CurrentQueues = new ObservableCollection<QueueWithPriority>();

        if (schedulerConfig == null)
        {
            var queues = new List<Queue>()
            {
                new Queue(new List<Content>()
                {
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\1.mp4"
                    ),
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\2.mp4"
                    ),
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\3.mp4"
                    )
                }, "first"),
                new Queue(new List<Content>()
                {
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\a1.mp4"
                    )
                }, "second")
            };
            
            schedulerConfig = new SchedulerConfig("default", "default",
                new List<QueueTriggerPair>()
                {
                    new QueueTriggerPair(queues[0], new List<TriggerConfig>()
                    {
                        new TriggerConfig("some_date", DateTime.Now.AddSeconds(5))
                    }, 10, 1),
                    new QueueTriggerPair(queues[1], new List<TriggerConfig>()
                    {
                        new TriggerConfig("some_date2", DateTime.Now.AddSeconds(7))
                    }, 10, 0),
                    // new QueueTriggerPair(queues[1], new List<TriggerConfig>()
                    // {
                    //     new TriggerConfig("every_10_sec", "0 0/2 * * * ?"),
                    //     new TriggerConfig("every_15_sec", "0 0/3 * * * ?")
                    // }, 1, 2)
                });
        }
        InitScheduler(schedulerConfig);

        CurrentQueues.CollectionChanged += (sender, args) =>
        {
            if (CurrentQueues.Count != 0)
            {
                CurrentQueueName = CurrentQueues
                    .OrderBy(el => el.Priority)
                    .First().Queue.Name;
            }
            else
            {
                CurrentQueueName = "No queues";
            }
        };
    }

    private IScheduler _scheduler;
    public PlayerState State { get; set; }
    public Display Display { get; set; }
    
    private ObservableCollection<QueueWithPriority> CurrentQueues = new();    
    // public PlayerDataContext PlayerDataContext { get; set; }

    public enum PlayerState
    {
        Playing,
        Paused
    }
    
    public void Play()
    {
        var playerView = new Player(Display);
        playerView.DataContext = this;
        playerView.Show();
    }
    
    private string _currentQueueName = "";
    public string CurrentQueueName
    {
        get => _currentQueueName;
        set
        {
            _currentQueueName = value;
            OnPropertyChanged(nameof(CurrentQueueName));
        }
    }

    public void Pause()
    {
        State = PlayerState.Paused;
    }
    public void Resume ()
    {
        State = PlayerState.Playing;
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}