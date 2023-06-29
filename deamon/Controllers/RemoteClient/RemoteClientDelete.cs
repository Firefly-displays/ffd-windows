using System;
using deamon.Models;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class RemoteClient
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