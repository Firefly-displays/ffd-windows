using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
    private readonly Client _remoteClient;
    private readonly Client _localClient;

    public BackgroundWorker()
    {
        Setuper.Setup();
        displaysController = DisplaysController.GetInstance();
        API = DeamonAPI.GetInstance();
        
        string pathToExe = @"C:\Users\onere\WebstormProjects\video-queue-web\node_app.exe";
        Process process = new Process();
        process.StartInfo.FileName = pathToExe;
        // process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        // process.StartInfo.CreateNoWindow = true;
        process.Start();
        
        _remoteClient = RemoteClient.GetInstance();
        _localClient =  LocalClient.GetInstance();

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