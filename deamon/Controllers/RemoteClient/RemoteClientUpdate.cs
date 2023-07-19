using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueueTriggerPair = deamon.Entities.QueueTriggerPair;

namespace deamon;

public partial class RemoteClient
{
    private void Update(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string id = (string)jsonMsg["id"]!;
        string? requestId = (string)jsonMsg["requestId"]!;
        JObject payload = JObject.Parse((string)jsonMsg["payload"]!);
        string action = (string)payload["action"]!;
        string result = "";

        switch (entity)
        {
            case "content":
                var c = deamonApi.GET<Content>(id);
                c.Name = (string)payload["name"]!;
                c.Duration = (int)payload["duration"]!;
                deamonApi.UPDATE(c);
                break;
            case "queue":
                var queue = deamonApi.GET<Queue>(id);

                switch (action)
                {
                    case "up":
                        try { queue = UpMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "down": try { queue = DownMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "rename":
                        try { queue.Name = (string)payload["name"]!; }
                        catch (Exception e) { result = "error"; } break;
                    case "removeMedia": try { queue = RemoveMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "add":
                        try
                        {
                            var contentList = queue.ContentList;
                            var contentIds = payload["mediaId"]!.ToObject<List<string>>();

                            var resList = new List<JObject>();

                            if (contentIds != null)
                                foreach (var contentId in contentIds)
                                {
                                    var content = deamonApi.GET<Content>(contentId);
                                    contentList.Add(content);
                                    resList.Add(new JObject()
                                    {
                                        { "id", content.Id },
                                        { "name", content.Name },
                                        // { "img", content.GetBaseThumb() }
                                    });
                                }

                            queue.ContentList = contentList;
                            result = JsonConvert.SerializeObject(resList);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                }
                
                deamonApi.UPDATE(queue);
                break;
            
            case "scheduler":
                var scheduler = deamonApi.GET<SchedulerEntity>(id);

                switch (action)
                {
                    case "changeDefaultQueue":
                        try
                        {
                            if ((string)payload["defaultQueue"]! == "null") scheduler.DefaultQueue = null;
                            else scheduler.DefaultQueue = deamonApi.GET<Queue>((string)payload["defaultQueue"]!);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                    case "schedule":
                        try
                        {
                            result = Schedule(id, payload);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                    case "editSchedule":
                        try
                        {
                            result = EditSchedule(id, payload);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                    case "dropQueue":
                        try
                        {
                            result = DropQueue(id, (string)payload["itemId"]!);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                    case "upQueue":
                        try
                        {
                            result = UpQueue(id, (string)payload["itemId"]!);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e); 
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                    case "downQueue":
                        try
                        {
                            result = DownQueue(id, (string)payload["itemId"]!);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e); 
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                }
                
                break;
            case "display":
                var display = deamonApi.GET<Display>(id);
                
                switch (action)
                {
                    case "changeScheduler":
                        try
                        {
                            if ((string)payload["schedulerId"]! == "null") display.SchedulerEntityId = null;
                            else display.SchedulerEntityId = (string)payload["schedulerId"]!;
                            result = "ok";
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e.ToString());
                            result = "error";
                        } break;
                }

                break;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", result }
        }.ToString());
    }
    
    private string UpQueue(string schedulerId, string queueId)
    {
        var scheduler = deamonApi.GET<SchedulerEntity>(schedulerId);
        var queueTriggerPairs = new List<QueueTriggerPair>();

        var item = scheduler.QueueTriggerPairs.Find(el => el.Id == queueId);
        if (item.Priority == 0) return "error";
        var upper = scheduler.QueueTriggerPairs.Find(el => el.Priority == item.Priority - 1);
        
        item.Priority -= 1;
        upper.Priority += 1;
        
        foreach (var el in scheduler.QueueTriggerPairs)
        {
            queueTriggerPairs.Add(el);
        }
        
        scheduler.QueueTriggerPairs = queueTriggerPairs;
        deamonApi.UPDATE(scheduler);
        return "ok";
    }
    
    private string DownQueue(string schedulerId, string queueId)
    {
        var scheduler = deamonApi.GET<SchedulerEntity>(schedulerId);
        var queueTriggerPairs = new List<QueueTriggerPair>();

        var item = scheduler.QueueTriggerPairs.First(el => el.Id == queueId);
        if (item.Priority == scheduler.QueueTriggerPairs
                .OrderByDescending(e => e.Priority).First().Priority) return "error";
        var lower = scheduler.QueueTriggerPairs.First(el => el.Priority == item.Priority + 1);
        
        item.Priority += 1;
        lower.Priority -= 1;
        
        foreach (var el in scheduler.QueueTriggerPairs)
        {
            queueTriggerPairs.Add(el);
        }

        scheduler.QueueTriggerPairs = queueTriggerPairs;
        deamonApi.UPDATE(scheduler);
        return "ok";
    }
    
    private string DropQueue(string schedulerId, string queueId)
    {
        var scheduler = deamonApi.GET<SchedulerEntity>(schedulerId);
        var queueTriggerPairs = scheduler.QueueTriggerPairs;
        var queueTriggerPair = queueTriggerPairs.Find(el => el.Id == queueId);
        var currentPriority = queueTriggerPair.Priority;
        queueTriggerPairs.Remove(queueTriggerPair);
        foreach (var el in queueTriggerPairs)
        {
            if (el.Priority > currentPriority) el.Priority -= 1;
        }
        scheduler.QueueTriggerPairs = queueTriggerPairs;
        deamonApi.UPDATE(scheduler);
        return "ok";
    }

    private string EditSchedule(string schedulerId, JObject payload)
    {
        var scheduler = deamonApi.GET<SchedulerEntity>(schedulerId);
        var itemId = (string)payload["itemId"]!;
        var cron = (string?)payload["cron"]!;
        var emitTime = (DateTime?)payload["emitTime"]!;
        var duration = (int)payload["duration"]!*60;
        
        var queueTriggerPairs = scheduler.QueueTriggerPairs;
        var queueTriggerPair = queueTriggerPairs.Find(el => el.Id == itemId);
        queueTriggerPair.Cron = cron;
        queueTriggerPair.EmitTime = emitTime;
        queueTriggerPair.Duration = duration;
        scheduler.QueueTriggerPairs = queueTriggerPairs;

        deamonApi.UPDATE(scheduler);
        return "ok";
    }
    
    private string Schedule(string schedulerId, JObject payload)
    {
        var scheduler = deamonApi.GET<SchedulerEntity>(schedulerId);
        var queue = deamonApi.GET<Queue>((string)payload["queue"]!);
        var cron = (string?)payload["cron"]!;
        var emitTime = (DateTime?)payload["emitTime"]!;
        var duration = (int)payload["duration"]!*60;
        
        var queueTriggerPairs = scheduler.QueueTriggerPairs;
        
        var p = new QueueTriggerPair(queue, cron, emitTime, duration, 0);
        foreach (var schedulerQueueTriggerPair in queueTriggerPairs)
        {
            schedulerQueueTriggerPair.Priority += 1;
        }
        queueTriggerPairs.Add(p);
        scheduler.QueueTriggerPairs = queueTriggerPairs;
        
        deamonApi.UPDATE(scheduler);

        return p.Id;
    }

    private Queue RemoveMedia(Queue queue, string mediaId)
    {
        var contentList = queue.ContentList;
        var contentId = mediaId;
        var index = contentList.FindIndex(el => el.Id == contentId);
        if (index == -1) throw new System.Exception("Media not found");   
        var item = contentList.Find(el => el.Id == contentId);
        contentList.RemoveAt(index);

        queue.ContentList = contentList;
        
        return queue;
    }

    private Queue UpMedia(Queue queue, string mediaId)
    {
        var contentList = queue.ContentList;
        var contentId = mediaId;
        var index = contentList.FindIndex(el => el.Id == contentId);
        if (index == -1) throw new System.Exception("Media not found");
        if (index == 0) throw new System.Exception("Media already at top");   
        var item = contentList.Find(el => el.Id == contentId);
        contentList.RemoveAt(index);
        contentList.Insert(index - 1, item!);

        queue.ContentList = contentList;
        
        return queue;
    }
    
    private Queue DownMedia(Queue queue, string mediaId)
    {
        var contentList = queue.ContentList;
        var contentId = mediaId;
        var index = contentList.FindIndex(el => el.Id == contentId);
        if (index == -1) throw new System.Exception("Media not found");
        if (index == contentList.Count - 1) throw new System.Exception("Media already at bottom");
        var item = contentList.Find(el => el.Id == contentId);
        contentList.RemoveAt(index);
        contentList.Insert(index + 1, item!);
        
        queue.ContentList = contentList;

        return queue;
    }
}