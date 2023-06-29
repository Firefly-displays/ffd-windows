using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using deamon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class RemoteClient
{
    private void Update(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string id = (string)jsonMsg["id"]!;
        string? requestId = (string)jsonMsg["requestId"]!;
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
                        try { queue = UpMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "down": try { queue = DownMedia(queue, (string)payload["mediaId"]!); }
                        catch (Exception e) { result = "error"; } break;
                    case "rename":
                        try { queue.Name = (string)payload["name"]!; }
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
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", result }
        }.ToString());
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