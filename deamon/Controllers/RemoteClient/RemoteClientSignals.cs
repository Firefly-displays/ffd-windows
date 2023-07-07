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
                break;
            case "resume":
                deamonApi.ResumeDisplay(displayId);
                Debug.WriteLine("resume");
                break;
            case "next":
                deamonApi.SkipContent(displayId);
                Debug.WriteLine("next");
                break;
            case "restart":
                deamonApi.RestartDisplay(displayId);
                Debug.WriteLine("restart");
                break;
            case "run":
                deamonApi.RunDisplay(displayId);
                Debug.WriteLine("run");
                break;
            case "stop":
                deamonApi.StopDisplay(displayId);
                Debug.WriteLine("stop");
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