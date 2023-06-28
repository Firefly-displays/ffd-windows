using System;
using System.Diagnostics;
using System.Linq;
using deamon.Models;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class RemoteClient
{
    private void Update(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string id = (string)jsonMsg["id"]!;
        string? requestId = (string)jsonMsg["requestId"];
        JObject payload = JObject.Parse((string)jsonMsg["payload"]!);
        string result = "";

        switch (entity)
        {
            case "queue":
                var queue = deamonApi.GET<Queue>(id);
                var action = (string)payload["action"]!;

                switch (action)
                {
                    case "up":
                        try { queue = upMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "down": try { queue = downMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                }
                
                deamonApi.UPDATE(queue);
                break;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", result }
        }.ToString());
    }

    private Queue upMedia(Queue queue, string mediaId)
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
    
    private Queue downMedia(Queue queue, string mediaId)
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