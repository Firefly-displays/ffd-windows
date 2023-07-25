using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using deamon.Entities;
using deamon.Models;
using Quartz;
using Quartz.Impl;
using Queue = deamon.Models.Queue;

namespace deamon;

public sealed partial class PlayerController
{
    private async void InitScheduler(SchedulerEntity schedulerEntity, EventHandler? schedulerInitialized)
    {
        IScheduler scheduler = await new StdSchedulerFactory().GetScheduler();
        _scheduler = scheduler;
            
        foreach (var queueTriggerPair in schedulerEntity.QueueTriggerPairs)
        {
            try
            {
                var job = CreateJob(
                    queueTriggerPair.Queue,
                    queueTriggerPair.Duration,
                    queueTriggerPair.Priority);
                var trigger = CreateTrigger(new TriggerConfig(
                    Guid.NewGuid().ToString(),
                    queueTriggerPair.Cron,
                    queueTriggerPair.EmitTime,
                    queueTriggerPair.Cron != null
                        ? TriggerConfig.TriggerType.Cron
                        : TriggerConfig.TriggerType.OneTime));
                await scheduler.ScheduleJob(job, trigger);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Logger.Log(e.ToString());
            }
        }
        
        await scheduler.Start();
        if (schedulerInitialized != null)
        {
            schedulerInitialized.Invoke(null, EventArgs.Empty);
        }
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
    
    private IJobDetail CreateJob(Queue queue, int duration, int priority)
    {
        var context = new Dictionary<string, object>();
            
        context.Add("queue", queue);
        context.Add("CurrentQueues", CurrentQueues);
        context.Add("duration", duration);
        context.Add("priority", priority);
        
        var job = JobBuilder.Create<ShowContentJob>()
            .WithIdentity("VideoJob_" + Guid.NewGuid().ToString("N"), "Video")
            .SetJobData(new JobDataMap((IDictionary)context))
            .Build();

        return job;
    }
}

public class ShowContentJob: IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        
        Queue? queue = dataMap["queue"] as Queue;
        ObservableCollection<QueueWithPriority>? currentQueues = 
            dataMap["CurrentQueues"] as ObservableCollection<QueueWithPriority>;
        int duration = (int) dataMap["duration"];
        int priority = (int) dataMap["priority"];

        if (currentQueues != null)
        {
            var queueWithPriority = new QueueWithPriority(queue, priority, true);
            Debug.WriteLine(DateTime.Now + " - ContentJob is running " + queue?.Name); 
            Logger.Log(DateTime.Now + " - ContentJob is running " + queue?.Name);
            currentQueues.Add(queueWithPriority);
            System.Threading.Thread.Sleep(1000 * duration);
            Debug.WriteLine(DateTime.Now + " - ContentJob ended " + queue?.Name);
            Logger.Log(DateTime.Now + " - ContentJob ended " + queue?.Name);
            currentQueues.Remove(queueWithPriority);
        }

        return Task.CompletedTask;
    }
}