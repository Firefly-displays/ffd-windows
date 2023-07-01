using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Quartz;
using Queue = deamon.Models.Queue;
using QueueTriggerPair = deamon.Models.QueueTriggerPair;

namespace deamon;

[DisallowConcurrentExecution]
public sealed partial class PlayerController: INotifyPropertyChanged
{
    event EventHandler SchedulerInitialized;
    public PlayerController (Display display)
    {
        Display = display;
        State = PlayerState.Playing;
        CurrentQueues = new ObservableCollection<QueueWithPriority>();
        var schedulerConfigs = new EntityModel<SchedulerEntity>().Data
            .Where(x => x.Id == display.SchedulerEntityId);

        if (!schedulerConfigs.Any())
        {
            return;
            // var basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Resources");
            // var queues = new List<Queue>()
            // {
            //     new Queue("first", new List<Content>()
            //     {
            //         // new (
            //         //     Content.ContentType.Image,
            //         //     basePath + @"\Images\123.jpg",
            //         //     1),
            //         new ("12",
            //             Content.ContentType.Image,
            //             basePath + @"\Images\1.jpg",
            //             null, 1),
            //         new Content("123",
            //             Content.ContentType.Video,
            //             basePath + @"\Video\1.mp4", null
            //         ),
            //         // new Content(
            //         //     Content.ContentType.Video,
            //         //     basePath + @"\Video\3.mp4"
            //         // )
            //     }),
            //     new Queue("second", new List<Content>()
            //     {
            //         new Content("1234",
            //             Content.ContentType.Video,
            //             basePath + @"\Video\a1.mp4", null
            //         )
            //     })
            // };
            //
            // schedulerConfig = new SchedulerConfig("default", "default",
            //     new List<QueueTriggerPair>()
            //     {
            //         new QueueTriggerPair(queues[0], new List<TriggerConfig>()
            //         {
            //             new TriggerConfig("some_date", DateTime.Now.AddSeconds(5))
            //         }, 1*60, 1),
            //         // new QueueTriggerPair(queues[1], new List<TriggerConfig>()
            //         // {
            //         //     new TriggerConfig("some_date2", DateTime.Now.AddSeconds(7))
            //         // }, 10, 0),
            //         // new QueueTriggerPair(queues[1], new List<TriggerConfig>()
            //         // {
            //         //     new TriggerConfig("every_10_sec", "0 0/2 * * * ?"),
            //         //     new TriggerConfig("every_15_sec", "0 0/3 * * * ?")
            //         // }, 1, 2)
            //     }, new Queue("defaultQueue", new List<Content>()
            //     {
            //         new ("789",
            //             Content.ContentType.Image,
            //             basePath + @"\Images\1623540080_32-phonoteka_org-p-abstraktsiya-karandashom-oboi-krasivo-32.jpg",
            //             null, 5)
            //     }));
        }
        
        var schedulerConfig = schedulerConfigs.First();
        DefaultQueue = schedulerConfig.DefaultQueue;
        CurrentQueue = DefaultQueue;
        if (CurrentQueue != null)
        {
            CurrentContent = CurrentQueue.ContentList[0];
        }
        
        CurrentQueues.CollectionChanged += OnCurrentQueuesChanged;
        ContentIsDone += PickContent;
        
        SchedulerInitialized += (sender, e) => Play();
        InitScheduler(schedulerConfig, SchedulerInitialized);
    }

    private bool IsQueueChanged = true;
    private void OnCurrentQueuesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IsQueueChanged = true;
        if (CurrentQueues != null && CurrentQueues.Count != 0)
        {
            try
            {
                var t  = CurrentQueues
                    .Where(el => el != null && el.Priority != null)
                    .OrderBy(el => el.Priority)
                    .First();

                if (t.Queue.Id != CurrentQueue.Id)
                {
                    CurrentQueue = t.Queue;
                    if (t.IsForceUpdate || CurrentQueues.Count == 1)
                    {
                        PickContent();
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                Debug.WriteLine(JsonConvert.SerializeObject(CurrentQueues));
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
        if (!CurrentContentIsVideo && State == PlayerState.Paused) return;
        
        CurrentContent = IsQueueChanged 
            ? CurrentQueue.ContentList[0] 
            : CurrentQueue.ContentList[(CurrentQueue.ContentList.IndexOf(CurrentContent) + 1) % CurrentQueue.ContentList.Count];
        IsQueueChanged = false;
        
        Debug.WriteLine("PickContent");
        Debug.WriteLine(JsonConvert.SerializeObject(CurrentContent.Path));
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

    public void Resume ()
    {
        State = PlayerState.Playing;
        if (!CurrentContentIsVideo) PickContent();
    }

    public void Stop()
    {
        PlayerView.Close();
    }

    public void SkipContent()
    {
        PickContent();
    }
}