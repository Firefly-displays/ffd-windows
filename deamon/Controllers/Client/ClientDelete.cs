using System;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class Client
{
    private void Delete(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string id = (string)jsonMsg["id"]!;
        string? requestId = (string)jsonMsg["requestId"]!;
        string result = "";

        try
        {
            switch (entity)
            {
                case "queue":
                    deamonApi.DELETE<Queue>(id);
                    break;
                case "scheduler":
                    deamonApi.DELETE<SchedulerEntity>(id);
                    break;
                case "content":
                    deamonApi.DELETE<Content>(id);
                    break;
                case "display":
                    deamonApi.DELETE<Display>(id);
                    break;
            }
        }
        catch (Exception e)
        {
            result = "error";
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", result }
        }.ToString());
    }
}