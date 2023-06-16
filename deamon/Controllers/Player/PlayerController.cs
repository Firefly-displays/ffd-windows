using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using deamon.Models;
using Newtonsoft.Json;
using Quartz;
using Queue = deamon.Models.Queue;

namespace deamon;

[DisallowConcurrentExecution]
public sealed partial class PlayerController: INotifyPropertyChanged
{
    event EventHandler SchedulerInitialized;
    public PlayerController (Display display, SchedulerConfig? schedulerConfig)
    {
        Display = display;
        State = PlayerState.Playing;
        CurrentQueues = new ObservableCollection<QueueWithPriority>();

        if (schedulerConfig == null)
        {
            var queues = new List<Queue>()
            {
                new Queue("first", new List<Content>()
                {
                    new (
                        Content.ContentType.Image,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Images\123.jpg",
                        5),
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\1.mp4"
                    ),
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\3.mp4"
                    )
                }),
                new Queue("second", new List<Content>()
                {
                    new Content(
                        Content.ContentType.Video,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\a1.mp4"
                    )
                })
            };
            
            schedulerConfig = new SchedulerConfig("default", "default",
                new List<QueueTriggerPair>()
                {
                    new QueueTriggerPair(queues[0], new List<TriggerConfig>()
                    {
                        new TriggerConfig("some_date", DateTime.Now.AddSeconds(5))
                    }, 1*60, 1),
                    new QueueTriggerPair(queues[1], new List<TriggerConfig>()
                    {
                        new TriggerConfig("some_date2", DateTime.Now.AddSeconds(7))
                    }, 10, 2),
                    // new QueueTriggerPair(queues[1], new List<TriggerConfig>()
                    // {
                    //     new TriggerConfig("every_10_sec", "0 0/2 * * * ?"),
                    //     new TriggerConfig("every_15_sec", "0 0/3 * * * ?")
                    // }, 1, 2)
                }, new Queue("defaultQueue", new List<Content>()
                {
                    new (
                        Content.ContentType.Image,
                        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Images\1623540080_32-phonoteka_org-p-abstraktsiya-karandashom-oboi-krasivo-32.jpg",
                        5)
                }));
        }
        DefaultQueue = schedulerConfig.DefaultQueue;
        CurrentQueue = DefaultQueue;
        CurrentContent = CurrentQueue.ContentList[0];
        
        CurrentQueues.CollectionChanged += OnCurrentQueuesChanged;
        ContentIsDone += PickContent;
        
        SchedulerInitialized += (sender, e) => Play();
        InitScheduler(schedulerConfig, SchedulerInitialized);
    }

    private bool IsQueueChanged = true;
    private void OnCurrentQueuesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsQueueChanged = true;
        if (CurrentQueues.Count != 0)
        {
            var t  = CurrentQueues
                .OrderBy(el => el.Priority)
                .First();

            if (t.Queue != CurrentQueue)
            {
                CurrentQueue = t.Queue;
                if (t.IsForceUpdate || CurrentQueues.Count == 1)
                {
                    PickContent();
                }
            }
        }
        else
        {
            CurrentQueue = DefaultQueue;
            PickContent();
        }
    }
    
    private void PickContent()
    {
        // Debug.WriteLine("PickContent");
        // Debug.WriteLine(JsonConvert.SerializeObject(CurrentQueue));
        CurrentContent = IsQueueChanged 
            ? CurrentQueue.ContentList[0] 
            : CurrentQueue.ContentList[(CurrentQueue.ContentList.IndexOf(CurrentContent) + 1) % CurrentQueue.ContentList.Count];
        IsQueueChanged = false;
    }
    
    public delegate void ContentIsDoneHandler();
    event ContentIsDoneHandler ContentIsDone;
    

    public void Play()
    {
        this.PlayerView = new Player(Display, ContentIsDone);
        PlayerView.DataContext = this;
        PlayerView.Show();
    }

    private Player PlayerView { get; set; }

    public void Pause() => State = PlayerState.Paused;
    public void Resume () =>  State = PlayerState.Playing;

    public void Stop()
    {
        PlayerView.Close();
    }

    public void SkipContent()
    {
        PickContent();
    }
}