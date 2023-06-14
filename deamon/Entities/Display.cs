using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using deamon.Entities;

namespace deamon.Models;

public class Display : Entity
{
    public enum DisplayStatus
    {
        Online,
        Offline,
        Unknown
    }

    private string _Name;
    private DisplayStatus _Status;
    private List<int> _Bounds;
    private SchedulerEntity _schedulerEntity;
    public string Name { get => _Name; set => SetField(ref _Name, value); }
    public DisplayStatus Status { get => _Status; set => SetField(ref _Status, value); }
    public List<int> Bounds { get => _Bounds; set => SetField(ref _Bounds, value); }
    public SchedulerEntity SchedulerEntity { get => _schedulerEntity; set => SetField(ref _schedulerEntity, value); }

    public Display(string name, DisplayStatus status, List<int> bounds)
    {
        Name = name;
        Status = status;
        Bounds = bounds;
    }
}