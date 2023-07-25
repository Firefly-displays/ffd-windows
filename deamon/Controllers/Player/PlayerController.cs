using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            SendStatus();
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
        if (!CurrentContentIsVideo && State != PlayerState.Playing) return;
        
        CurrentContent = IsQueueChanged 
            ? CurrentQueue.ContentList[0] 
            : CurrentQueue.ContentList[(CurrentQueue.ContentList.IndexOf(CurrentContent) + 1) % CurrentQueue.ContentList.Count];
        IsQueueChanged = false;
        
        Logger.Log("PickContent");
        Logger.Log(JsonConvert.SerializeObject(CurrentContent.Path));
        SendStatus();
    }

    private void SendStatus()
    {
        string status = new JObject()
        {
            { "type", "currMediaChanged" },
            { "displayId", Display.Id },
            { "queueId", CurrentQueue.Id },
            { "queueName", CurrentQueue.Name },
            { "id", CurrentContent.Id },
            { "name", CurrentContent.Name },
            { "duration", CurrentContent.Duration.ToString() }
        }.ToString();
        
        RemoteClient.GetInstance().WSSend(status);
        LocalClient.GetInstance().WSSend(status);
    }

    public delegate void ContentIsDoneHandler();
    event ContentIsDoneHandler ContentIsDone;
    

    public void Play()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            this.PlayerView = new Player(Display, ContentIsDone);
            PlayerView.DataContext = this;
            PlayerView.Show();
        });
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
        _scheduler.Shutdown();
        _scheduler = null;
        PlayerView.Dispatcher.Invoke(() => PlayerView.Close());
    }

    public void SkipContent()
    {
        PickContent();
    }

    public async Task SafeRefresh()
    {
        var schedulerConfigs = EntityModel<SchedulerEntity>.GetInstance().Data
            .Where(x => x.Id == Display.SchedulerEntityId);

        if (!schedulerConfigs.Any()) return;
        
        var schedulerConfig = schedulerConfigs.First();
        DefaultQueue = schedulerConfig.DefaultQueue;

        if (CurrentQueue?.Id == DefaultQueue?.Id)
        {
            CurrentQueue = DefaultQueue;
        }

        await _scheduler.Shutdown();
        _scheduler = null;
        
        InitScheduler(schedulerConfig, null);
    }
}