using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Forms;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Simpl;
using Quartz.Xml.JobSchedulingData20;
using QueueTriggerPair = deamon.Entities.QueueTriggerPair;

namespace deamon;

public static class Experiment
{
    public static void GetDeviceInfo()
    {
        var d = new Dictionary<string, object>();

        d.Add("Display", EntityModel<Display>.GetInstance());
        d.Add("Content", EntityModel<Content>.GetInstance());
        d.Add("Queue", EntityModel<Queue>.GetInstance());
        d.Add("SchedulerEntity", EntityModel<SchedulerEntity>.GetInstance());

        var API = DeamonAPI.GetInstance();

        // var queues = API.GET<Queue>();
        //
        // var schedulerEntities = new List<SchedulerEntity>()
        // {
        //     new SchedulerEntity("1", new List<QueueTriggerPair>()
        //     {
        //         new(queues[0], "0 15,30,45 * ? * *", null, 10, 1),
        //         new(queues[1], "0 0 10-18 ? 1-1 3#2 *", null, 60 * 20, 2),
        //         new(queues[2], "0 0 */2 ? * *", null, 60 * 60 * 2, 3),
        //     }),
        //     new SchedulerEntity("2", new List<QueueTriggerPair>()
        //     {
        //         new(queues[0], "0 15,30,45 * ? * *", null, 10, 1),
        //         new(queues[1], "0 0 10-18 ? 1-1 3#2 *", null, 60 * 20, 2),
        //         new(queues[2], "0 0 */2 ? * *", null, 60 * 60 * 2, 3),
        //     }),
        // };
        //
        // foreach (SchedulerEntity schedulerEntity in schedulerEntities)
        // {
        //     API.POST(schedulerEntity);
        // }
        //
        // Debug.WriteLine(JsonConvert.SerializeObject(API.GET<SchedulerEntity>()));
        // Debug.WriteLine("========================================");
        //
        // var scheduler = API.GET<SchedulerEntity>().First();
        // scheduler.Name = "Основной планировщик";
        // scheduler.QueueTriggerPairs.Add(new(queues[0], "0 45 * ? * *", null, 10, 1));
        //
        // var someId = scheduler.QueueTriggerPairs.First().Id;
        // var qtp = scheduler.QueueTriggerPairs.First(el => el.Id == someId);
        // qtp.Cron = null;
        // qtp.EmitTime = DateTime.Now;
        // qtp.Priority = 100;
        //
        // Debug.WriteLine(someId);
        // Debug.WriteLine("========================================");
        //
        // API.UPDATE(scheduler);
        
        // var first = API.GET<SchedulerEntity>().First();
        // API.DELETE<SchedulerEntity>(first.Id);
        //
        // Debug.WriteLine(JsonConvert.SerializeObject(API.GET<SchedulerEntity>()));

        var someList = new List<int>() { 1, 2, 3, 4, 5 };
        
        var element = someList.First(el => el == 3);
        var index = someList.FindIndex(el => el == 3);    
        someList.RemoveAt(index);
        someList.Insert(index-1, element);
        
        Debug.WriteLine(JsonConvert.SerializeObject(someList));
    }
}

