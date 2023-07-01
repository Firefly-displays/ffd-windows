﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using deamon.Models;
using deamon.Views;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace deamon;

public class DisplaysController
{
    private EntityModel<Display> DisplaysDM;
    private ObservableCollection<Display> Displays;
    
    private Dictionary<Display, PlayerController> Players = new();

    public DisplaysController()
    {
        Players = new Dictionary<Display, PlayerController>();
        DisplaysDM = new EntityModel<Display>();
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
                Display.DisplayStatus.Unknown,
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
            var window = new DisplayIdentifier(display);
            window.Show();
        }
    }

    public void RunOnline()
    {
        try
        {
            Debug.WriteLine("RunOnline");
            foreach (var display in this.Displays
                         .Where(d => d.Status == Display.DisplayStatus.Online))
            {
                OpenPlayer(display.Id);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public void StopOnline()
    {
        foreach (var display in this.Displays
                     .Where(d => d.Status == Display.DisplayStatus.Online))
        {
            StopPlayer(display.Id);
        }
    }
    
    public void OpenPlayer(string displayId)
    {
        // if (display.SchedulerEntity == null) return;
        Display display = Displays.First(d => d.Id == displayId);
        var player = new PlayerController(display);
        Players.Add(display, player);
    }
    
    public void PausePlayer(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.Pause();
    }
    
    public void ResumePlayer(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.Resume();
    }
    
    public void StopPlayer(string displayId)
    {
        if (Players.All(p => p.Key.Id != displayId)) return;
        Players.First(p => p.Key.Id == displayId).Value.Stop();
        Display display = Displays.First(d => d.Id == displayId);
        Players.Remove(display);
    }
    
    public void Restart(string displayId)
    {
        StopPlayer(displayId);
        OpenPlayer(displayId);
    }
    
    public void SkipContent(Display display)
    {
        Players[display].SkipContent();
    }
}