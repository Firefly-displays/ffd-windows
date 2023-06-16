using System;
using System.Collections.Generic;
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
        
        // var d = new B("someName", Display.DisplayStatus.Offline, new List<int>(){1, 2, 3, 4}, null);
        // var s = JsonConvert.SerializeObject(d);
        // Console.WriteLine(s);
        // var c = JsonConvert.DeserializeObject<B>(s);
        
        // var d = new Display("123", Display.DisplayStatus.Offline, new List<int> {1, 2, 3, 4}, null);
        // var s = JsonConvert.SerializeObject(d);
        // Console.WriteLine(s);
        // var c = JsonConvert.DeserializeObject<Display>(s);

        var m = new EntityModel<Display>();
        var d = new Display("123", Display.DisplayStatus.Offline, new List<int> {1, 2, 3, 4}, null); 
        m.Add(d);

        // var all = m.GetAll();
        // var one = m.GetById(d.Id).Name;

        var updated = new Display(d.Id, "1234", Display.DisplayStatus.Online,
            new List<int>() { 2, 3, 4, 5 }, null);
        m.Update(updated);
        
        var updatedName = m.GetById(d.Id).Name;

        Console.ReadKey();
    }
}


public class A
{
    public A(string? id)
    {
        ID = id ?? Guid.NewGuid().ToString();
    }

    public string? ID { get; set; }
}

public class B : A
{
    public B(string name, Display.DisplayStatus status, List<int> bounds, string? sid) 
        : this(null, name, status, bounds, sid) {}
    
    [JsonConstructor]
    public B(string? id, string name, Display.DisplayStatus status, List<int> bounds, string? sid) : base(id)
    {
        Name = name;
        Status = status;
        SchedulerEntityId = sid;
        Bounds = bounds;
    }

    public List<int> Bounds { get; set; }

    public string? SchedulerEntityId { get; set; }

    public Display.DisplayStatus Status { get; set; }

    public string Name { get; set; }
    
}