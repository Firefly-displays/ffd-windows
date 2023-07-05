using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        var schedulerConfigs = EntityModel<SchedulerEntity>.GetInstance().Data
            .Where(x => x.Id == display.SchedulerEntityId);

        if (!schedulerConfigs.Any()) { return; }
        
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
                Logger.Log(exception.ToString());
                Debug.WriteLine(JsonConvert.SerializeObject(CurrentQueues));
                Logger.Log(JsonConvert.SerializeObject(CurrentQueues));
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
        Logger.Log("PickContent");
        Debug.WriteLine(JsonConvert.SerializeObject(CurrentContent.Path));
        Logger.Log(JsonConvert.SerializeObject(CurrentContent.Path));
        
        RemoteClient.GetInstance().WSSend(new JObject()
        {
            { "type", "currMediaChanged" },
            { "displayId", Display.Id },
            { "contentName", CurrentContent.Name },
            { "contentId", CurrentContent.Id },
            { "queueName", CurrentQueue.Name },
            {  "queueId", CurrentQueue.Id }
        }.ToString());
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