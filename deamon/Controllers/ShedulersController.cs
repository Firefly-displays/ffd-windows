using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using deamon.Entities;
using deamon.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Simpl;
using Quartz.Xml;
using Quartz.Xml.JobSchedulingData20;
using Queue = deamon.Models.Queue;

namespace deamon;

public class SchedulersController
{
    private EntityModel<SchedulerEntity> SchedulersDM;
    private ObservableCollection<SchedulerEntity> Schedulers;
    
    private List<IScheduler> schedulers;
    private readonly StdSchedulerFactory _factory;

    public SchedulersController()
    {
        this.schedulers = new();
        SchedulersDM = new EntityModel<SchedulerEntity>();
        Schedulers = SchedulersDM.Data;
        
        _factory = new StdSchedulerFactory();
    }

    public async void RestoreSchedulers()
    {
        Console.WriteLine(JsonConvert.SerializeObject(Schedulers));
        foreach (var schedulerEntity in Schedulers)
        {
            IScheduler scheduler = await _factory.GetScheduler();
            
            foreach (var queueTriggerPair in schedulerEntity.QueueTriggerPairs)
            {
                var job = CreateJob(queueTriggerPair.Queue);
                
                foreach (ITrigger trigger in queueTriggerPair.Triggers)
                {
                    await scheduler.ScheduleJob(job, trigger);
                }
            }
            
            schedulers.Add(scheduler);
            await scheduler.Start();
            Console.WriteLine(JsonConvert.SerializeObject(scheduler));
        }
    }

    public async void Experiment()
    {
        IScheduler scheduler = await _factory.GetScheduler();
       
        
        var job = CreateJob(new Queue("some"));;
        var trigger = CreateTrigger(5);

        var str = JsonConvert.SerializeObject(trigger);
        var restored = JsonConvert.DeserializeObject<ITrigger>(str);
        
        Console.WriteLine(JsonConvert.SerializeObject(trigger));


        // await scheduler.ScheduleJob(job, trigger);
        // Console.WriteLine(JsonConvert.SerializeObject(await scheduler.Get()));
    }
    
    // public void CreateScheduler(string name)
    // {
    //     var someTrigger = CreateTrigger(5) as triggerType;
    //     var someTrigger2 = CreateTrigger(10) as triggerType;
    //     
    //     Schedulers.Add(new SchedulerEntity(name, new List<QueueTriggerPair>()
    //     {
    //         new (new Queue("some"), new List<triggerType>() { someTrigger, someTrigger2 }),
    //         new (new Queue("other"), new List<triggerType>() { someTrigger }),
    //     }));
    // }

    private ITrigger CreateTrigger(int interval)    
    {
        var trigger = TriggerBuilder.Create()
            .StartNow()
            .StartAt(DateTimeOffset.Now.AddSeconds(5))
            // .WithCalendarIntervalSchedule(x => x.)
                // .WithSimpleSchedule(x => x.
                // .WithIntervalInSeconds(interval)
                // .RepeatForever())
            .Build();

        return trigger;
    }
    
    private IJobDetail CreateJob(Queue queue)
    {
        var context = new Dictionary<string, object>();
        context.Add("data", "Hello world");
        // context.Add("currentQueue", queue);
        // context.Add("parentSchedulerModel", this);
        
        var job = JobBuilder.Create<ConsoleLoggerJob>()
            .WithIdentity("VideoJob", "Video")
            .SetJobData(new JobDataMap((IDictionary)context))
            .Build();

        return job;
    }
}

// public class ShowContentJob: IJob
// {
//     public Task Execute(IJobExecutionContext context)
//     {
//         JobDataMap dataMap = context.JobDetail.JobDataMap;
//         ((SchedulerModel)(dataMap["parentSchedulerModel"]))
//             .CurrentQueue = dataMap["currentQueue"] as Queue;
//         
//         Debug.WriteLine("Hello from ShowContentJob!" + DateTime.Now + (dataMap["currentQueue"] as Queue)?.Name) ;
//         return Task.CompletedTask;
//     }
// }

public class ConsoleLoggerJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        
        Console.WriteLine("Hello from ConsoleLoggerJob! -> " + dataMap["data"]) ;
        return Task.CompletedTask;
    }
}
