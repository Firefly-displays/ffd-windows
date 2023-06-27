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

namespace deamon;

public static class Experiment
{
    public static void GetDeviceInfo()
    {
        var d = new Dictionary<string, object>();
        
        d.Add("Display", new EntityModel<Display>());
        d.Add("Content", new EntityModel<Content>());
        d.Add("Queue", new EntityModel<Queue>());
        d.Add("SchedulerEntity", new EntityModel<SchedulerEntity>());
        
        var API = new DeamonAPI(d, new DisplaysController());
        
        var queues = new List<Queue>()
        {
            new Queue("123", new List<Content>() {API.GET<Content>()[0]}),
            new Queue("3211", API.GET<Content>()),
        };
        foreach (Queue queue in queues)
        {
            API.POST(queue);
        }

        // var data = API.GET<Queue>();

        // var m = new EntityModel<Queue>();
        //
        // Debug.WriteLine(JsonConvert.SerializeObject(m));

        // var m = new EntityModel<Queue>();
        // var q = new Queue("first");
        // var q2 = new Queue("second");

        // var q = new Queue("хуй");
        // var s = JsonConvert.SerializeObject(q);
        // Debug.WriteLine(s);
        // var q2 = JsonConvert.DeserializeObject<Some>(s);
        // Debug.WriteLine(q2.Name);

        // var q = new Queue("123213123");
        //
        // var s = JsonConvert.SerializeObject(q);
        // Debug.WriteLine(s);
        // var q2 = JsonConvert.DeserializeObject<Queue>(s);
        // Debug.WriteLine(q2.Name);
    }
}

