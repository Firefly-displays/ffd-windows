using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using deamon.Entities;
using Newtonsoft.Json;

namespace deamon.Models;

public class Display : Entity
{
    public enum DisplayStatus
    {
        Online,
        Offline,
        Unknown
    }

    public string Name { get => _Name; set => SetField(ref _Name, value); }
    private string _Name;

    public DisplayStatus Status { get => _Status; set => SetField(ref _Status, value); }
    private DisplayStatus _Status;

    public List<int> Bounds { get => _Bounds; set => SetField(ref _Bounds, value); }
    private List<int> _Bounds;

    public string? SchedulerEntityId { get => _schedulerEntityId; set => SetField(ref _schedulerEntityId, value); }
    private string? _schedulerEntityId;
    
    public Display(string name, DisplayStatus status, List<int> bounds, string? schedulerEntityId) 
        : this(null, name, status, bounds, schedulerEntityId) {}
    
    [JsonConstructor]
    public Display(string? id, string name, DisplayStatus status, List<int> bounds, string? schedulerEntityId) : base(id)
    {
        Name = name;
        Status = status;
        Bounds = bounds;
        SchedulerEntityId = schedulerEntityId;
    }
}