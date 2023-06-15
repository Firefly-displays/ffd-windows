using deamon.Models;

namespace deamon;

public class QueueWithPriority
{
    public Queue Queue { get; set; }
    public int Priority { get; set; }

    public QueueWithPriority(Queue queue, int priority)
    {
        Queue = queue;
        Priority = priority;
    }
}