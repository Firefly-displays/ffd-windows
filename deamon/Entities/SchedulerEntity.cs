using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using deamon.Models;
using Quartz;
using Quartz.Xml.JobSchedulingData20;

namespace deamon.Entities;

public class SchedulerEntity : Entity
{
    private string _Name;
    private List<QueueTriggerPair> _QueueTriggerPairs;

    public string Name { get => _Name; set => SetField(ref _Name, value); }
    public List<QueueTriggerPair> QueueTriggerPairs
    {
        get => _QueueTriggerPairs; set => SetField(ref _QueueTriggerPairs, value);
    }
    
    
    // public SchedulerEntity(string name)
    // {
    //     this.Name = name;
    //     this.QueueTriggerPairs = new List<QueueTriggerPair>();
    // }
    
    [JsonConstructor]
    public SchedulerEntity(string name, List<QueueTriggerPair> queueTriggerPairs)
    {
        this.Name = name;
        this.QueueTriggerPairs = queueTriggerPairs;
    }
}