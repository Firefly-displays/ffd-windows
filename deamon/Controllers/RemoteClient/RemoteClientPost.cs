using System.Collections.Generic;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class RemoteClient
{
    private void Post(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string? requestId = (string)jsonMsg["requestId"];
        JObject payload = JObject.Parse((string)jsonMsg["payload"]!);

        string result = "";

        switch (entity)
        {
            case "queue":
                result = AddQueue(payload);
                break;
            case "scheduler":
                result = AddScheduler(payload); 
                break;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", result }
        }.ToString());
    }
    
    private string AddScheduler(JObject payload)
    {
        string name = (string)payload["name"]!;

        var scheduler = new SchedulerEntity(name);
        deamonApi.POST(scheduler);
        
        return new JObject()
        {
            { "id", scheduler.Id },
            { "name", scheduler.Name }
        }.ToString();
    }

    private string AddQueue(JObject payload)
    {
        string name = (string)payload["name"]!;
        string[] contentIds = payload["mediaIds"]!.ToObject<string[]>()!;
        var contentList = new List<Content>();
        foreach (var contentId in contentIds)
        {
            contentList.Add(deamonApi.GET<Content>(contentId));
        }

        var queue = new Queue(name, contentList);
        deamonApi.POST(queue);
        return new JObject()
        {
            { "id", queue.Id },
            { "name", queue.Name }
        }.ToString();
    }
}