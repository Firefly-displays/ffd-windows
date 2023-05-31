using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace deamon.Models;

public class SchedulerModel : INotifyPropertyChanged
{
    private IScheduler _scheduler;

    private Queue? _currentQueue;
    private readonly List<Queue> queues;

    public Queue? CurrentQueue
    {
        get => _currentQueue;
        set => SetField(ref _currentQueue, value);
    }
    
    public SchedulerModel(IScheduler scheduler)
    {
        this.queues = new List<Queue>()
        {
            new Queue(new List<Content>()
            {
                new Content(
                    Content.ContentType.Video,
                    @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\1.mp4"
                ),
                new Content(
                    Content.ContentType.Video,
                    @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\2.mp4"
                ),
                new Content(
                    Content.ContentType.Video,
                    @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\3.mp4"
                )
            }, "first"),
            new Queue(new List<Content>()
            {
                new Content(
                    Content.ContentType.Video,
                    @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\Video\a1.mp4"
                )
            }, "second")
        };
        _scheduler = scheduler;
        Start();
    }

    public async Task Start()
    {
        var jobs = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
        jobs.Add(CreateJob(queues[0]), new ReadOnlyCollection<ITrigger>(new List<ITrigger>() {CreateTrigger(1)}));
        jobs.Add(CreateJob(queues[1]), new ReadOnlyCollection<ITrigger>(new List<ITrigger>() {CreateTrigger(3)}));
        
        // foreach (var queue in queues)
        // {
        //     jobs.Add(CreateJob(queue), new ReadOnlyCollection<ITrigger>(new List<ITrigger>()
        //     {
        //         CreateTrigger(5)
        //     }));
        // }
        
        _scheduler = await new StdSchedulerFactory().GetScheduler();
        await _scheduler.ScheduleJobs(jobs, true);
        
        await _scheduler.Start();
    }

    public async Task Stop()
    {
        await _scheduler.Shutdown();
    }

    private ITrigger CreateTrigger(int interval)    
    {
        var trigger = TriggerBuilder.Create()
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(interval)
                .RepeatForever())
            .Build();

        return trigger;
    }
    
    private IJobDetail CreateJob(Queue queue)
    {
        var context = new Dictionary<string, object>();
        context.Add("currentQueue", queue);
        context.Add("parentSchedulerModel", this);
        
        var job = JobBuilder.Create<ShowContentJob>()
            // .WithIdentity("VideoJob", "Video")
            .SetJobData(new JobDataMap((IDictionary)context))
            .Build();

        return job;
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