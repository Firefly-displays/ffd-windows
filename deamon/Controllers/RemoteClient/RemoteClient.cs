using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using deamon.Models;
using Microsoft.MixedReality.WebRTC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace deamon;

public partial class RemoteClient
{
    private static RemoteClient? instance;
    
    public static RemoteClient GetInstance()
    {
        if (instance == null)
        {
            instance = new RemoteClient();
        }

        return instance;
    }
    
    private WebSocket ws = new("ws://localhost:6969");
    private PeerConnection pc = new();
    private DeamonAPI deamonApi;
    private string saveFilePath;
    private FileStream saveFileStream;

    public RemoteClient()
    {
        deamonApi = DeamonAPI.GetInstance();
        
        ws.OnOpen += (sender, e) =>
        {
            Debug.WriteLine("Соединение ws установлено");
            ws.Send("{\"type\":\"connection\",\"role\":\"host\",\"hostID\":\"123\",\"hostPassword\":\"123\"}");
        };

        ws.OnClose += (sender, e) => { Debug.WriteLine("Соединение ws закрыто"); };

        ws.OnError += (sender, e) =>
        {
            Debug.WriteLine("Ошибка ws: " + e.Message);
        };

        ws.OnMessage += async (sender, e) =>
        {
            try
            {
                var msg = e.Data;

                Debug.WriteLine($"web socket recv: {msg.Length} bytes");
                JObject jsonMsg = JObject.Parse(msg);

                switch ((string)jsonMsg["type"]!)
                {
                    case "get": Get(jsonMsg); break;
                    case "post": Post(jsonMsg); break;
                    case "update": Update(jsonMsg); break;
                    case "delete": Delete(jsonMsg); break;
                    case "signal": HandleSignal(jsonMsg); break;
                    case "iceCandidate": HandleIceCandidate(jsonMsg); break;
                    case "offer": HandleOffer(jsonMsg); break;
                }
            }
            catch (Exception exception) { Debug.WriteLine(exception); }
        };
        
        ws.Connect();
    }

    private void WSSend(string msg)
    {
        ws.Send(new JObject()
        {
            { "role", "host" },
            { "hostID", "123" },
            { "hostPassword", "123" },
            { "message", msg }
        }.ToString());
    }
}