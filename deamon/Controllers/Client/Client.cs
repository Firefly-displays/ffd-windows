using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.MixedReality.WebRTC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using Application = System.Windows.Forms.Application;

namespace deamon;

public partial class Client
{
    private WebSocket ws;
    private PeerConnection pc = new();
    private DeamonAPI deamonApi;
    private string saveFilePath;
    private FileStream saveFileStream;

    private string hostId;
    private string hostPassword;

    public Client(string url)
    {
        deamonApi = DeamonAPI.GetInstance();

        var creds = File.ReadAllText(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "credentials.txt"))
            .Split("\r\n");
        
        hostId = creds[0];
        hostPassword = creds[1];
        
        InitWS(url);
    }
    
    private async Task InitWS(string url)
    {
        ws = new(url);
        
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
            InitWS(url);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.WriteLine("Ошибка ws: " + e.Message);
            Logger.Log("Ошибка ws: " + e.Message);
            InitWS(url);
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
                    case "changePassword": ChangePassword(jsonMsg); break;
                }
            }
            catch (Exception exception)
            {
                Logger.Log(exception.ToString());
            }
        };
        
        try
        {
            ws.Connect();
            if (!ws.IsAlive) throw new Exception();
        }
        catch (Exception e)
        {
            Logger.Log(JsonConvert.SerializeObject(e));
            await Task.Delay(500);
            InitWS(url);
        }
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

    private void ChangePassword(JObject jsonMsg)
    {
        string newPass = (string)jsonMsg["password"]!;
        hostPassword = newPass;

        string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "credentials.txt");

        string[] lines = File.ReadAllLines(filePath);
        lines[^1] = newPass;
        File.WriteAllLines(filePath, lines);
        
        Application.Restart();
        Environment.Exit(0);
    }
}