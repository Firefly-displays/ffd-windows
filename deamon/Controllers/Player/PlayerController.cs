using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents.DocumentStructures;
using deamon.Models;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Queue = deamon.Models.Queue;

namespace deamon;

[DisallowConcurrentExecution]
public class PlayerController
{
    public PlayerController (Display display, SchedulerConfig? schedulerConfig)
    {
        Display = display;
        State = PlayerState.Playing;
        
        PlayerDataContext = new PlayerDataContext();

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
                    }, 10),
                    new QueueTriggerPair(queues[1], new List<TriggerConfig>()
                    {
                        new TriggerConfig("every_10_sec", "0 0/2 * * * ?"),
                        new TriggerConfig("every_15_sec", "0 0/3 * * * ?")
                    }, 1)
                });
        }
        InitScheduler(schedulerConfig);
    }

    private async void InitScheduler(SchedulerConfig schedulerConfig)
    {
        IScheduler scheduler = await new StdSchedulerFactory().GetScheduler();
            
        foreach (var queueTriggerPair in schedulerConfig.QueueTriggerPairs)
        {
            foreach (var triggerConfig in queueTriggerPair.Triggers)
            {
                var job = CreateJob(queueTriggerPair.Queue, queueTriggerPair.Duration);
                var trigger = CreateTrigger(triggerConfig);
                await scheduler.ScheduleJob(job, trigger);
            }
        }
        
        await scheduler.Start();
    }

    private IScheduler _scheduler;
    public PlayerState State { get; set; }
    public Display Display { get; set; }
    public PlayerDataContext PlayerDataContext { get; set; }

    public enum PlayerState
    {
        Playing,
        Paused
    }
    
    public void Play()
    {
        var playerView = new Player(Display, PlayerDataContext);
        playerView.Show();
    }
    public void Pause()
    {
        State = PlayerState.Paused;
    }
    public void Resume ()
    {
        State = PlayerState.Playing;
    }

    private ITrigger CreateTrigger(TriggerConfig triggerConfig)
    {
        return triggerConfig.Type switch
        {
            TriggerConfig.TriggerType.OneTime => TriggerBuilder.Create()
                .StartAt(new DateTimeOffset(triggerConfig.ExecuteTime ?? DateTime.Now))
                .Build(),
            TriggerConfig.TriggerType.Cron => TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule(triggerConfig.CronExpression!)
                .Build(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private IJobDetail CreateJob(Queue queue, int duration)
    {
        var context = new Dictionary<string, object>();
            
        context.Add("queue", queue);
        context.Add("playerDataContext", PlayerDataContext);
        context.Add("duration", duration);
        
        var job = JobBuilder.Create<ShowContentJob>()
            .WithIdentity("VideoJob_" + Guid.NewGuid().ToString("N"), "Video")
            .SetJobData(new JobDataMap((IDictionary)context))
            .Build();

        return job;
    }
}