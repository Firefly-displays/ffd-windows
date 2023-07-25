using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using deamon.Models;
using deamon.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace deamon;

public class DisplaysController
{
    private static DisplaysController _instance;
    
    public static DisplaysController GetInstance()
    {
        if (_instance == null)
        {
            _instance = new DisplaysController();
        }

        return _instance;
    }
    
    private EntityModel<Display> DisplaysDM;
    private ObservableCollection<Display> Displays;
    
    private Dictionary<Display, PlayerController> Players = new();

    private DisplaysController()
    {
        Players = new Dictionary<Display, PlayerController>();
        DisplaysDM = EntityModel<Display>.GetInstance();
        Displays = DisplaysDM.Data;
        Sync();
        
        SystemEvents.DisplaySettingsChanging += DisplaySettingsChangingEventHandler;
    }
    
    private void DisplaySettingsChangingEventHandler(object sender, EventArgs e)
    {
        Sync();
    }

    private void Sync()
    {
        var onlineDisplays = Screen.AllScreens;
        
        foreach (var display in Displays)
        {
            if (display.Status == Display.DisplayStatus.Unknown) continue;
            
            var isOnline = onlineDisplays.Any(curr => 
                curr.Bounds.Top == display.Bounds[0] &&
                curr.Bounds.Left == display.Bounds[1] &&
                curr.Bounds.Width == display.Bounds[2] &&
                curr.Bounds.Height == display.Bounds[3]
            );
            
            display.Status = isOnline ? Display.DisplayStatus.Online : Display.DisplayStatus.Offline;
        }
        
        onlineDisplays.Where(curr => !Displays.Any(disp => 
            curr.Bounds.Top == disp.Bounds[0] &&
            curr.Bounds.Left == disp.Bounds[1] &&
            curr.Bounds.Width == disp.Bounds[2] &&
            curr.Bounds.Height == disp.Bounds[3]
        )).ToList().ForEach(curr =>
        {
            Displays.Add(new Display(
                curr.DeviceName,
                Display.DisplayStatus.Online,
                new List<int>
                {
                    curr.Bounds.Top,
                    curr.Bounds.Left,
                    curr.Bounds.Width,
                    curr.Bounds.Height
                }, null
            ));
        });
    }

    public void Identify()
    {
        foreach (var display in Displays.Where(d => d.Status == Display.DisplayStatus.Online))
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
               var window = new DisplayIdentifier(display);
                window.Show();
            });
        }
    }

    public Content? GetCurrentContent(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return null;
        return Players.First(p => p.Key.Id == displayId).Value.CurrentContent;
    }
    
    public Queue? GetCurrentQueue(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return null;
        return Players.First(p => p.Key.Id == displayId).Value.CurrentQueue;
    }
    

    public void RunOnline()
    {
        try
        {
            Debug.WriteLine("RunOnline");
            Logger.Log("RunOnline");
            foreach (var display in this.Displays
                         .Where(d => d.Status == Display.DisplayStatus.Online && d.SchedulerEntityId != null))
            {
                OpenPlayer(display.Id);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            Logger.Log(e.ToString());
        }
    }

    public void StopOnline()
    {
        foreach (var display in this.Displays
                     .Where(d => d.Status == Display.DisplayStatus.Online))
        {
            try
            {
                StopPlayer(display.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Logger.Log(e.ToString());
            }
        }
    }
    
    public void OpenPlayer(string displayId)
    {
        // if (display.SchedulerEntity == null) return;
        Display display = Displays.First(d => d.Id == displayId);
        var player = new PlayerController(display);
        if (player.State == PlayerController.PlayerState.Aborted)
        {
            SendStatus(displayId, "stopped");
            return;
        }
        Players.Add(display, player);
        SendStatus(displayId, "playing");
    }
    
    public void PausePlayer(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.Pause();
        SendStatus(displayId, "paused");
    }
    
    public void ResumePlayer(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.Resume();
        SendStatus(displayId, "playing");
    }
    
    public void StopPlayer(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.Stop();
        Display display = Displays.First(d => d.Id == displayId);
        Players.Remove(display);
        SendStatus(displayId, "stopped");
    }
    
    public void Restart(string displayId)
    {
        StopPlayer(displayId);
        OpenPlayer(displayId);
    }
    
    public void SkipContent(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.SkipContent();
    }
    
    private void SendStatus(string displayId, string status)
    {
        string msg = new JObject()
        {
            { "type", "displayStatusChanged" },
            { "displayId", displayId },
            { "status", status }
        }.ToString();

        RemoteClient.GetInstance().WSSend(msg);
        LocalClient.GetInstance().WSSend(msg);
    }

    public string GetStatus(string displayId)
    {
        if (Displays.All(x => x.Id != displayId)) return "unknown";
        var d = Displays.First(x => x.Id == displayId);
        if (d.Status == Display.DisplayStatus.Offline) return "offline";
        if (!Players.ContainsKey(d)) return "stopped";
        if (Players[d].State == PlayerController.PlayerState.Paused) return "paused";
        if (Players[d].State == PlayerController.PlayerState.Playing) return "playing";
        return "unknown";
    }

    public void SafeRefresh()
    {
        foreach (var (display, playerController) in Players)
        {
            playerController.SafeRefresh();
        }
    }
}