using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                        { "img", content.GetBaseThumb() }
                    }); 
                }
                break;
            case "queue":
                if (id == "*")
                {
                    var queues = deamonApi.GET<Queue>();
                    Debug.WriteLine("fetching queues");
                    Debug.WriteLine(queues.Count);
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
                                    { "img", c.GetBaseThumb() }
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
            // case "schedulerEntity": t = typeof(SchedulerEntity); break;
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