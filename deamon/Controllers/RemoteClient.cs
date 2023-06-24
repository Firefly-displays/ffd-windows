using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using deamon.Entities;
using deamon.Models;
using Microsoft.MixedReality.WebRTC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace deamon;

public class RemoteClient
{
    private WebSocket ws = new("ws://localhost:6969");
    private PeerConnection pc = new();
    private DeamonAPI deamonApi;
    private string saveFilePath;
    private FileStream saveFileStream;

    public RemoteClient(DeamonAPI api)
    {
        deamonApi = api;
        
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
            var msg = e.Data;

            Debug.WriteLine($"web socket recv: {msg.Length} bytes");
            JObject jsonMsg = JObject.Parse(msg);

            switch ((string)jsonMsg["type"]!)
            {
                case "get": Get(jsonMsg); break;
                case "iceCandidate": HandleIceCandidate(jsonMsg); break;
                case "offer": HandleOffer(jsonMsg); break;
            }
        };
        
        ws.Connect();
    }

    private void Get(JObject jsonMsg)
    {
        string entity = (string)jsonMsg["entity"]!;
        string id = (string)jsonMsg["id"]!;
        string? requestId = (string)jsonMsg["requestId"];

        List<JObject> result = new List<JObject>();

        switch (entity)
        {
            case "display":
                var displays = id == "*" 
                    ? deamonApi.GET<Display>() 
                    : new List<Display>() { deamonApi.GET<Display>(id) };

                foreach (var display in displays)
                {
                    result.Add(new()
                    {
                        { "id", display.Id },
                        { "name", display.Name },
                        { "scheduler", display.SchedulerEntityId }
                    });      
                }
                
                break;
            // case "content": t = typeof(Content); break;
            // case "queue": t = typeof(Queue); break;
            // case "schedulerEntity": t = typeof(SchedulerEntity); break;
            default: return;
        }
        
        WSSend(new JObject()
        {
            { "type", "response" },
            { "requestId", requestId },
            { "payload", JsonConvert.SerializeObject(result) }
        }.ToString());
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
    
    private void HandleIceCandidate(JObject jsonMsg)
    {
        JObject payload = JObject.Parse(((string)jsonMsg["payload"])!);
        Debug.WriteLine(pc.Initialized);

        var iceCandidate = new IceCandidate();
        iceCandidate.SdpMid = (string)payload["sdpMid"]!;
        iceCandidate.SdpMlineIndex = (int)payload["sdpMLineIndex"]!;
        iceCandidate.Content = (string)payload["candidate"]!;
        pc.AddIceCandidate(iceCandidate);
    }

    private async void HandleOffer(JObject jsonMsg)
    {
        string sdp = (string)jsonMsg["payload"];
        Debug.WriteLine("==== Received remote peer SDP offer.");
        
        pc.IceCandidateReadytoSend += iceCandidate =>
        {
            Debug.WriteLine($"Sending ice candidate: {JsonConvert.SerializeObject(iceCandidate)}");

            string payload = new JObject
            {
                { "type", "ice" },
                { "candidate", iceCandidate.Content },
                { "sdpMLineindex", iceCandidate.SdpMlineIndex },
                { "sdpMid", iceCandidate.SdpMid }
            }.ToString();

            JObject iceCandidateMsg = new JObject
            {
                { "type", "iceCandidate" },
                { "iceCandidate", payload }
            };

            JObject m = new JObject()
            {
                { "role", "host" },
                { "hostID", "123" },
                { "hostPassword", "123" },
                { "message", iceCandidateMsg.ToString() }
            };

            ws.Send(m.ToString());
        };

        pc.IceStateChanged += (newState) =>
        {
            Debug.WriteLine($"ice connection state changed to {newState}.");
            if (
                // newState == IceConnectionState.Closed || 
                // newState == IceConnectionState.Failed ||
                newState == IceConnectionState.Disconnected)
            {
                Debug.WriteLine("ice connection state changed to closed, exiting.");
                // pc.Close();
                // pc.Dispose();
                pc = new PeerConnection();
            }
        };
        
        pc.DataChannelAdded += (dc) =>
        {
            Debug.WriteLine($"Data channel added: {dc.Label}");
            dc.StateChanged += () =>
            {
                Debug.WriteLine($"Data channel state: {dc.State}");
            };
            dc.MessageReceived += (data) =>
            {
                string text = System.Text.Encoding.Default.GetString(data);
                JObject jsonText = null;
                try { jsonText = JObject.Parse(text); } catch {}

                if (jsonText != null)
                {
                    JObject payload = JObject.Parse(jsonText["payload"]!.ToString()!);
                    switch ((string)jsonText["type"]!)
                    {
                        case "startSending":
                            saveFilePath = Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                "../../../Resources/Media/"
                                ) + "saving_" + (string)payload["name"]!; 
                            saveFileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
                            break;
                        case "endSending": 
                            saveFileStream.Close();
                            File.Move(
                                saveFilePath, 
                                saveFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Resources/Media/") + (string)payload["name"]!);
                            break;
                    }
                }
                else
                {
                    saveFileStream.Write(data);
                }
            };
        };

        pc.LocalSdpReadytoSend += message =>
        {
            Debug.WriteLine($"SDP answer ready, sending to remote peer.");

            // Send our SDP answer to the remote peer.
            JObject spmMessage = new JObject
            {
                { "type", "answer" },
                { "sdp", message.Content }
            };
            
            JObject payload = new JObject
            {
                { "type", "answer" },
                { "answer", spmMessage.ToString() }
            };

            JObject m = new JObject()
            {
                { "role", "host" },
                { "hostID", "123" },
                { "hostPassword", "123" },
                { "message", payload.ToString() }
            };

            ws.Send(m.ToString());
        };

        var sdpMessage = new SdpMessage
        {
            Type = SdpMessageType.Offer,
            Content = sdp.Trim('"').Replace("\\r\\n", "\r\n")
        };

        var config = new PeerConnectionConfiguration
        {
            IceServers = new List<IceServer> {
                new IceServer{ Urls = { "stun:stun.l.google.com:19302" } }
            }
        };
        Debug.WriteLine(pc.Initialized);
        await pc.InitializeAsync(config);
        Debug.WriteLine("Peer connection initialized.");
        await pc.SetRemoteDescriptionAsync(sdpMessage);
        Debug.WriteLine("SetRemoteDescriptionAsync complete.");
        if (!pc.CreateAnswer())
        {
            Debug.WriteLine("Failed to create peer connection answer, closing peer connection.");
            pc.Close();
        }
        else
        {
            Debug.WriteLine("Peer connection answer successfully created.");
        }
    }
}