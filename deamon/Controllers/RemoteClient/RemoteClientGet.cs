using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class RemoteClient
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
            case "display":
                var displays = id == "*" 
                    ? deamonApi.GET<Display>() 
                    : new List<Display>() { deamonApi.GET<Display>(id) };

                foreach (var display in displays)
                {
                    result.Add(new()
                    {
                        { "id", display.Id },
                        { "name", display.Name },
                        { "scheduler", display.SchedulerEntityId }
                    });      
                }
                
                break;
            case "content":
                var contents = id == "*" 
                    ? deamonApi.GET<Content>() 
                    : new List<Content>() { deamonApi.GET<Content>(id) };
                
                foreach (var content in contents)
                {
                    result.Add(new()
                    {
                        { "id", content.Id },
                        { "name", content.Name },
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
                                    // { "img", c.GetBaseThumb() }
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