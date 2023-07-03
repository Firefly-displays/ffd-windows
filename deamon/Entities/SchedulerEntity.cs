using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using deamon.Models;
using Quartz;
using Quartz.Xml.JobSchedulingData20;

namespace deamon.Entities;

public class SchedulerEntity : Entity
{
    public string Name { get => _Name; set => SetField(ref _Name, value); }
    private string _Name;
    
    public Queue? DefaultQueue { get => _DefautlQueue; set => SetField(ref _DefautlQueue, value); }
    private Queue? _DefautlQueue;
    
    public List<QueueTriggerPair> QueueTriggerPairs
    {
        get => _QueueTriggerPairs.ToList(); 
        set => SetField(ref _QueueTriggerPairs,  new ObservableCollection<QueueTriggerPair>(value));
    }

    private ObservableCollection<QueueTriggerPair> _QueueTriggerPairs;
    
    public void Add(QueueTriggerPair pair)
    {
        QueueTriggerPairs.Add(pair);
    }
    
    public SchedulerEntity(string name) : this(null, name) {}

    public SchedulerEntity(string? id, string name) : base(id)
    {
        QueueTriggerPairs = new List<QueueTriggerPair>();
        Name = name;

        _QueueTriggerPairs.CollectionChanged += (sender, args) =>
        {
            OnPropertyChanged(nameof(QueueTriggerPairs));
        };
    }
    
    public SchedulerEntity(string name, List<QueueTriggerPair> queueTriggerPairs) : this(null, name, queueTriggerPairs) {}
    
    [JsonConstructor]
    public SchedulerEntity(string? id, string name, List<QueueTriggerPair> queueTriggerPairs) : base(id)
    {
        QueueTriggerPairs = queueTriggerPairs;
        Name = name;
        
        _QueueTriggerPairs.CollectionChanged += (sender, args) =>
        {
            OnPropertyChanged(nameof(QueueTriggerPairs));
            
            foreach (var queueTriggerPair in QueueTriggerPairs)
            {
                queueTriggerPair.PropertyChanged += (sender, args) =>
                {
                    OnPropertyChanged(nameof(QueueTriggerPairs));
                };
            }
        };
    }
}