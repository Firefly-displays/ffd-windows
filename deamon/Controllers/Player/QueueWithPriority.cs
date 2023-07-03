using deamon.Models;

namespace deamon;

public class QueueWithPriority
{
    public Queue? Queue { get; set; }
    public int Priority { get; set; }

    public bool IsForceUpdate { get; set; }

    public QueueWithPriority(Queue? queue, int priority, bool? isForceUpdate = false)
    {
        Queue = queue;
        Priority = priority;
        IsForceUpdate = isForceUpdate ?? false;
    }
}