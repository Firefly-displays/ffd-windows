using System.Collections.Generic;
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
                string name = (string)payload["name"]!;
                string[] contentIds = payload["mediaIds"]!.ToObject<string[]>()!;
                var contentList = new List<Content>();
                foreach (var contentId in contentIds)
                {
                    contentList.Add(deamonApi.GET<Content>(contentId));
                }

                var queue = new Queue(name, contentList);
                deamonApi.POST(queue);
                result = queue.Id;
                break;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", result }
        }.ToString());
    }
}