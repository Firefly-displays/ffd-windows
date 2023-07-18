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
    
    private WebSocket ws = new("ws://oneren.space:6969");
    private PeerConnection pc = new();
    private DeamonAPI deamonApi;
    private string saveFilePath;
    private FileStream saveFileStream;

    private string hostId;
    private string hostPassword;

    public RemoteClient()
    {
        deamonApi = DeamonAPI.GetInstance();

        var creds = File.ReadAllText(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "credentials.txt"))
            .Split("\r\n");
        
        hostId = creds[0];
        hostPassword = creds[1];
        
        ws.OnOpen += (sender, e) =>
        {
            Debug.WriteLine("Соединение ws установлено");
            Logger.Log("Соединение ws установлено");
            
            ws.Send(new JObject()
            {
                { "type", "connection" },
                { "role", "host" },
                { "hostID", hostId },
                { "hostPassword", hostPassword }
            }.ToString());
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.WriteLine("Соединение ws закрыто");
            Logger.Log("Соединение ws закрыто");
        };

        ws.OnError += (sender, e) =>
        {
            Debug.WriteLine("Ошибка ws: " + e.Message);
            Logger.Log("Ошибка ws: " + e.Message);
        };

        ws.OnMessage += async (sender, e) =>
        {
            try
            {
                var msg = e.Data;

                Debug.WriteLine($"web socket recv: {msg.Length} bytes");
                Logger.Log($"web socket recv: {msg.Length} bytes");
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
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                Logger.Log(exception.ToString());
            }
        };
        
        ws.Connect();
    }

    public void WSSend(string msg)
    {
        ws.Send(new JObject()
        {
            { "role", "host" },
            { "hostID", hostId },
            { "hostPassword", hostPassword },
            { "message", msg }
        }.ToString());
    }
}