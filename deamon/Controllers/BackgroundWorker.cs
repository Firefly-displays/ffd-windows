using System;
using System.Collections.Generic;
using System.Diagnostics;
using deamon.Entities;
using deamon.Models;

namespace deamon;

public class BackgroundWorker : IBackgroundWorker
{
    public static BackgroundWorker Instance;
    
    public static BackgroundWorker GetInstance()
    {
        if (Instance == null)
        {
            Instance = new BackgroundWorker();
        }

        return Instance;
    }
    
    private DisplaysController displaysController;
    
    public DeamonAPI API;
    private readonly RemoteClient remoteClient;

    public BackgroundWorker()
    {
        displaysController = DisplaysController.GetInstance();
        API = DeamonAPI.GetInstance();
        remoteClient = RemoteClient.GetInstance();
        Start();
    }

    public void Start()
    {
        displaysController.RunOnline();
    }
    
    public void Stop()
    {
        displaysController.StopOnline();
    }

    public void Restart()
    {
        displaysController.StopOnline();
        displaysController.RunOnline();
    }

    public void Restart(List<Display> displays)
    {
        foreach (var display in displays)
        {
            displaysController.StopPlayer(display.Id);
        }
    }
    
    public void Pause(Display display)
    {
        displaysController.PausePlayer(display.Id);
    }
}