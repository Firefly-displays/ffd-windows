using System;
using System.Diagnostics;
using System.Threading.Tasks;
using deamon.Models;
using Quartz;

namespace deamon;

public class ShowContentJob: IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        
        Queue? queue = dataMap["queue"] as Queue;
        PlayerDataContext? playerDataContext = dataMap["playerDataContext"] as PlayerDataContext;
        int duration = (int) dataMap["duration"];

        if (playerDataContext != null)
        {
            playerDataContext.CurrentQueue = queue;
            Debug.WriteLine(DateTime.Now + " - ContentJob is running " + queue?.Name);

            System.Threading.Thread.Sleep(1000 * duration);
            
            Debug.WriteLine(DateTime.Now + " - ContentJob ended " + queue?.Name);
            playerDataContext.CurrentQueue = null;
        }

        return Task.CompletedTask;
    }
}