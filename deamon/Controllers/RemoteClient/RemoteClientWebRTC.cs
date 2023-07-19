using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using deamon.Models;
using Microsoft.MixedReality.WebRTC;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace deamon;

public partial class RemoteClient
{
    private void HandleIceCandidate(JObject jsonMsg)
    {
        JObject payload = JObject.Parse(((string)jsonMsg["payload"])!);
        Debug.WriteLine(pc.Initialized);
        Logger.Log(pc.Initialized.ToString());

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
        Logger.Log("==== Received remote peer SDP offer.");
        
        pc.IceCandidateReadytoSend += iceCandidate =>
        {
            Debug.WriteLine($"Sending ice candidate: {JsonConvert.SerializeObject(iceCandidate)}");
            Logger.Log($"Sending ice candidate: {JsonConvert.SerializeObject(iceCandidate)}");

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
                { "hostID", hostId },
                { "hostPassword", hostPassword },
                { "message", iceCandidateMsg.ToString() }
            };

            ws.Send(m.ToString());
        };

        pc.IceStateChanged += (newState) =>
        {
            Debug.WriteLine($"ice connection state changed to {newState}."); 
            Logger.Log($"ice connection state changed to {newState}.");
            if (
                // newState == IceConnectionState.Closed || 
                // newState == IceConnectionState.Failed ||
                newState == IceConnectionState.Disconnected)
            {
                Debug.WriteLine("ice connection state changed to closed, exiting.");
                Logger.Log("ice connection state changed to closed, exiting.");
                // pc.Close();
                // pc.Dispose();
                pc = new PeerConnection();
            }
        };
        
        pc.DataChannelAdded += (dc) =>
        {
            Debug.WriteLine($"Data channel added: {dc.Label}");
            Logger.Log($"Data channel added: {dc.Label}");
            dc.StateChanged += () =>
            {
                Debug.WriteLine($"Data channel state: {dc.State}");
                Logger.Log($"Data channel state: {dc.State}");
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
                            try
                            {
                                saveFilePath = Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "Media/"
                                ) + Guid.NewGuid() + "_" + (string)payload["name"]! + ".tmp";
                                saveFileStream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
                            }
                            catch (Exception e)
                            {
                                Logger.Log("Error while creating media file.");
                                Logger.Log(e.ToString());
                                Debug.WriteLine(e);
                            }
                            
                            break;
                        case "endSending": 
                            saveFileStream.Close();
                            var fileName = (string)payload["name"]!;
                            var newPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "Media/") + Guid.NewGuid() + "_" + fileName;
                            var isVideo = ((string)payload["type"]!).StartsWith("video");
                            var thumpPath = isVideo ? newPath + ".jpg" : newPath;

                            try
                            {
                                File.Move(saveFilePath, newPath, true);
                            }
                            catch (Exception e)
                            {
                                Logger.Log("Error while moving media file.");
                                Logger.Log(e.ToString());
                            }

                            try
                            {
                                Content newContent = new Content(
                                    (string)payload["name"]!,
                                    isVideo
                                        ? Content.ContentType.Video 
                                        : Content.ContentType.Image, 
                                    newPath,
                                    !isVideo ? thumpPath : null);

                                deamonApi.POST(newContent);

                                if (isVideo)
                                {
                                    try
                                    {
                                        var thumbFileStream = new FileStream(thumpPath, FileMode.Create, FileAccess.Write);
                                        var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                                        ffMpeg.GetVideoThumbnail(newPath, thumbFileStream, 5);
                                        thumbFileStream.Close();
                                        Debug.WriteLine("Saved raw thumb.");
                                        Logger.Log("Saved raw thumb.");
                            
                                        newContent.ThumbPath = thumpPath;
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Log("Error while creating thumb.");
                                        Logger.Log(e.ToString());
                                    }
                                    
                                    deamonApi.UPDATE(newContent);
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Log("Error while posting content.");
                                Logger.Log(e.ToString());
                            }

                            break;
                        case "abortSending":
                            try
                            {
                                saveFileStream.Close();
                                File.Delete(saveFilePath);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                                Logger.Log(e.ToString());
                            }
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
            Logger.Log($"SDP answer ready, sending to remote peer.");

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
                { "hostID", hostId },
                { "hostPassword", hostPassword },
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
        Logger.Log(pc.Initialized.ToString());
        await pc.InitializeAsync(config);
        Debug.WriteLine("Peer connection initialized.");
        Logger.Log("Peer connection initialized.");
        await pc.SetRemoteDescriptionAsync(sdpMessage);
        Debug.WriteLine("SetRemoteDescriptionAsync complete.");
        Logger.Log("SetRemoteDescriptionAsync complete.");
        if (!pc.CreateAnswer())
        {
            Debug.WriteLine("Failed to create peer connection answer, closing peer connection.");
            Logger.Log("Failed to create peer connection answer, closing peer connection.");
            pc.Close();
        }
        else
        {
            Debug.WriteLine("Peer connection answer successfully created.");
            Logger.Log("Peer connection answer successfully created.");
        }
    }
}