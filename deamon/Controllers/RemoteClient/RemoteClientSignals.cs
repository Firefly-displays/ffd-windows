using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Windows;

namespace deamon;

public partial class RemoteClient
{
    private void HandleSignal(JObject jsonMsg)
    {
        var signal = (string)jsonMsg["signal"]!;
        var displayId = (string)jsonMsg["id"]!;
        var requestId = (string)jsonMsg["requestId"]!;
        
        switch (signal)
        {
            case "pause":
                deamonApi.PauseDisplay(displayId);
                Debug.WriteLine("pause");
                Logger.Log("pause");
                break;
            case "resume":
                deamonApi.ResumeDisplay(displayId);
                Debug.WriteLine("resume");
                Logger.Log("resume");
                break;
            case "next":
                deamonApi.SkipContent(displayId);
                Debug.WriteLine("next");
                Logger.Log("next");
                break;
            case "restart":
                deamonApi.RestartDisplay(displayId);
                Debug.WriteLine("restart");
                Logger.Log("restart");
                break;
            case "run":
                deamonApi.RunDisplay(displayId);
                Debug.WriteLine("run");
                Logger.Log("run");
                break;
            case "stop":
                deamonApi.StopDisplay(displayId);
                Debug.WriteLine("stop");
                Logger.Log("stop");
                break;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", "ok" }
        }.ToString());
    }
}