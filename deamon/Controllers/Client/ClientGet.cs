using System;
using System.Collections.Generic;
using System.Linq;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class Client
{
    private void Get(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string id = (string)jsonMsg["id"]!;
        string? requestId = (string)jsonMsg["requestId"];

        List<JObject> result = new List<JObject>();

        switch (entity)
        {
            case "img":
                var c = deamonApi.GET<Content>(id);
                WSSend(new JObject()
                {
                    { "type", "response" },
                    { "requestId", requestId },
                    { "payload", c.GetBaseThumb() }
                }.ToString());
                break;
            case "logs":
                WSSend(new JObject()
                {
                    { "type", "response" },
                    { "requestId", requestId },
                    { "payload", Logger.GetLogs() }
                }.ToString());
                return;
            case "display":
                var displays = id == "*" 
                    ? deamonApi.GET<Display>() 
                    : new List<Display>() { deamonApi.GET<Display>(id) };

                foreach (var display in displays)
                {
                    var currentContent = DisplaysController.GetInstance().GetCurrentContent(display.Id);
                    var currentQueue = DisplaysController.GetInstance().GetCurrentQueue(display.Id);
                    var status = DisplaysController.GetInstance().GetStatus(display.Id);
                    
                    result.Add(new()
                    {
                        { "id", display.Id },
                        { "name", display.Name },
                        { "scheduler", display.SchedulerEntityId },
                        { "currentMedia", currentContent != null 
                            ? new JObject()
                            {
                                { "queueId", currentQueue.Id },
                                { "queueName", currentQueue.Name },
                                { "id", currentContent.Id },
                                { "name", currentContent.Name },
                                { "duration", currentContent.Duration.ToString() }
                            }.ToString()
                            : null
                        },
                        { "status", status }
                    });      
                }
                
                break;
            case "content_count":
                WSSend(new JObject()
                {
                    { "type", "response" },
                    { "requestId", requestId },
                    { "payload", deamonApi.GET<Content>().Count.ToString() }
                }.ToString());
                return;
            case "content":
                var contents = id == "*" 
                    ? deamonApi.GET<Content>() 
                    : new List<Content>() { deamonApi.GET<Content>(id) };

                var page = jsonMsg["page"];
                
                if (page != null)
                {
                    int pageIndex = (int)page - 1;
                    int pageSize = 9;

                    contents = contents.GetRange(
                        pageIndex * pageSize, 
                        Int32.Min(pageSize, contents.Count-pageIndex * pageSize));
                }
                
                foreach (var content in contents)
                {
                    result.Add(new()
                    {
                        { "id", content.Id },
                        { "name", content.Name },
                        { "duration", content.Duration.ToString() },
                        { "type", content.Type == Content.ContentType.Video ? "video" : "img" }
                        // { "img", content.GetBaseThumb() }
                    }); 
                }
                break;
            case "queue":
                if (id == "*")
                {
                    var queues = deamonApi.GET<Queue>();
                    foreach (var queue in queues)
                    {
                        result.Add(new()
                        {
                            { "id", queue.Id },
                            { "name", queue.Name }
                        }); 
                    }
                }
                else
                {
                    var queue = deamonApi.GET<Queue>(id);
                    var items = queue.ContentList.Select((c, i) =>
                    {
                        return new JObject()
                        {
                            {
                                "media", new JObject()
                                {
                                    { "id", c.Id },
                                    { "name", c.Name },
                                    { "duration", c.Duration }
                                }
                            },
                            { "priority", i }
                        };
                    });
                    result.Add(new()
                    {
                        { "id", queue.Id },
                        { "name", queue.Name },
                        { "items", JsonConvert.SerializeObject(items) }
                    });
                }
                break;
            case "scheduler": 
                if (id == "*")
                {
                    var schedulers = deamonApi.GET<SchedulerEntity>();
                    foreach (var scheduler in schedulers)
                    {
                        result.Add(new()
                        {
                            { "id", scheduler.Id },
                            { "name", scheduler.Name }
                        }); 
                    }
                }
                else
                {
                    var scheduler = deamonApi.GET<SchedulerEntity>(id);

                    var queues = scheduler.QueueTriggerPairs.Select((q, i) =>
                    {
                        return new JObject()
                        {
                            {
                                "queue", new JObject()
                                {
                                    { "id", q.Queue.Id },
                                    { "name", q.Queue.Name }
                                }
                            },
                            { "id", q.Id },
                            { "cron", q.Cron },
                            { "emitTime", q.EmitTime },
                            { "duration", q.Duration },
                            { "priority", q.Priority }
                        };
                    });

                    result.Add(new JObject()
                    {
                        { "id", scheduler.Id },
                        { "name", scheduler.Name },
                        { "defaultQueue", scheduler.DefaultQueue == null ? "" : scheduler.DefaultQueue.Id },
                        { "queues", JsonConvert.SerializeObject(queues) }
                    });
                }; 
                break;
            default: return;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", JsonConvert.SerializeObject(result) }
        }.ToString());
    }
}