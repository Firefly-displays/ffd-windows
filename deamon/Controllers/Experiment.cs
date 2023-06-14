using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Windows.Forms;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Microsoft.Win32;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore;
using Quartz.Simpl;
using Quartz.Xml.JobSchedulingData20;

namespace deamon;

public static class Experiment
{
    public static async void GetDeviceInfo()
    {
        // DirectSchedulerFactory.Instance.CreateScheduler(
        //     "scheduler", 
        //     "instance", new ZeroSizeThreadPool(), new JobStoreTX()
        // );
        //
        // var scheduler = await DirectSchedulerFactory.Instance.GetScheduler("scheduler");

        // Assembly asm = typeof(triggerType).Assembly;
        // var names = asm.GetTypes().Select(t => t.Name);
        // Console.WriteLine(string.Join("\n", names));

        // schedulers.Add(scheduler);
        // await scheduler.Start();
        // Console.WriteLine(JsonConvert.SerializeObject(scheduler));
        
        Console.WriteLine("Starting");
        var c = new SchedulersController();
        c.Experiment();
        // c.CreateScheduler("first");
        // c.RestoreSchedulers();



        // var SchedulersDM = new EntityModel<SchedulerEntity>();
        // var Schedulers = SchedulersDM.Data;
        // Console.WriteLine(JsonConvert.SerializeObject(Schedulers));
        
        Console.ReadKey();
    }
}
