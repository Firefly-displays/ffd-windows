using System;
using System.Collections.Generic;

namespace deamon.Models;

public class SchedulerConfig
{
    public string Name { get; set; }
    public string Id { get; set; }
    public List<QueueTriggerPair> QueueTriggerPairs { get; set; }
    
    public SchedulerConfig(string name, string id, List<QueueTriggerPair> queueTriggerPairs)
    {
        Name = name;
        Id = id;
        QueueTriggerPairs = queueTriggerPairs;
    }
}

public class QueueTriggerPair
{
    public Queue Queue { get; set; }
    public List<TriggerConfig> Triggers { get; set; }
    
    public int Duration { get; set; }
    
    public int Priority { get; set; }

    public QueueTriggerPair(Queue queue, List<TriggerConfig> triggers, int duration, int priority)
    {
        Queue = queue;
        Triggers = triggers;
        Duration = duration;
        Priority = priority;
    }
}

public class TriggerConfig
{
    public string ID { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? ExecuteTime { get; set; }
    public TriggerType Type { get; set; }
    
    public enum TriggerType
    {
        Cron,
        OneTime
    }
    
    public TriggerConfig(string id, string? cronExpression, DateTime? executeTime, TriggerType type)
    {
        ID = id;
        CronExpression = cronExpression;
        ExecuteTime = executeTime;
        Type = type;
    }
    
    public TriggerConfig(string id, string? cronExpression)
    {
        ID = id;
        CronExpression = cronExpression;
        Type = TriggerType.Cron;
    }
    
    public TriggerConfig(string id, DateTime? executeTime)
    {
        ID = id;
        ExecuteTime = executeTime;
        Type = TriggerType.OneTime;
    }
}