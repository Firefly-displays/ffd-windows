// using System;
// using System.Diagnostics;
// using System.Globalization;
// using System.Threading.Tasks;
// using Quartz;
//
// namespace deamon.Models;
//
// public class ShowContentJob: IJob
// {
//     public Task Execute(IJobExecutionContext context)
//     {
//         JobDataMap dataMap = context.JobDetail.JobDataMap;
//         ((SchedulerModel)(dataMap["parentSchedulerModel"]))
//             .CurrentQueue = dataMap["currentQueue"] as Queue;
//         
//         Debug.WriteLine("Hello from ShowContentJob!" + DateTime.Now + (dataMap["currentQueue"] as Queue)?.Name) ;
//         return Task.CompletedTask;
//     }
// }
//
// public class ConsoleLoggerJob : IJob
// {
//     public Task Execute(IJobExecutionContext context)
//     {
//         JobDataMap dataMap = context.JobDetail.JobDataMap;
//         
//         Debug.WriteLine("Hello from ConsoleLoggerJob! -> " + dataMap["data"]) ;
//         return Task.CompletedTask;
//     }
// }