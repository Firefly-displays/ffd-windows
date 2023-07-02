using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                                        { "img", content.GetBaseThumb() }
                                    });
                                }

                            queue.ContentList = contentList;
                            result = JsonConvert.SerializeObject(resList);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
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
                        try { scheduler.DefaultQueue = deamonApi.GET<Queue>((string)payload["defaultQueue"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "schedule":
                        try { result = Schedule(id, payload); }
                        catch (Exception e) { result = "error"; } break;
                    case "editSchedule":
                        try { result = EditSchedule(id, payload); }
                        catch (Exception e) { result = "error"; } break;
                    case "dropQueue":
                        try { result = DropQueue(id, (string)payload["itemId"]!); }
                        catch (Exception e) { result = "error"; } break;
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
    
    private string DropQueue(string schedulerId, string queueId)
    {
        var scheduler = deamonApi.GET<SchedulerEntity>(schedulerId);
        var queueTriggerPairs = scheduler.QueueTriggerPairs;
        var queueTriggerPair = queueTriggerPairs.Find(el => el.Id == queueId);
        queueTriggerPairs.Remove(queueTriggerPair);
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
        queueTriggerPair.Duration = duration * 60;
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