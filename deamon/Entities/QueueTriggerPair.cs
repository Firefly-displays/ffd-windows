using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using deamon.Models;
using Newtonsoft.Json;

namespace deamon.Entities;

public class QueueTriggerPair : INotifyPropertyChanged
{
    public Queue Queue { get => _Queue; set => SetField(ref _Queue, value); }
    private Queue _Queue;
    
    public string? Id { get => _Id; set => SetField(ref _Id, value); }
    private string? _Id;
    
    public string? Cron { get => _Cron; set => SetField(ref _Cron, value); }
    private string? _Cron;
    
    public DateTime? EmitTime { get => _EmitTime; set => SetField(ref _EmitTime, value); }
    private DateTime? _EmitTime;
    
    public int Duration { get => _Duration; set => SetField(ref _Duration, value); }
    private int _Duration;
    
    public int Priority { get => _Priority; set => SetField(ref _Priority, value); }
    private int _Priority;

    public QueueTriggerPair(Queue queue, string? cron, DateTime? emitTime, int duration, int priority)
        : this(null, queue, cron, emitTime, duration, priority) { }

    [JsonConstructor]
    public QueueTriggerPair(string? id, Queue queue, string? cron, DateTime? emitTime, int duration, int priority)
    {
        Id = id ?? Guid.NewGuid().ToString();
        Queue = queue;
        Cron = cron;
        EmitTime = emitTime;
        Duration = duration;
        Priority = priority;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}