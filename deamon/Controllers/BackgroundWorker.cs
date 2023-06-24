using System;
using System.Collections.Generic;
using System.Diagnostics;
using deamon.Entities;
using deamon.Models;

namespace deamon;

public class BackgroundWorker : IBackgroundWorker
{
    private DisplaysController displaysController;
    
    public DeamonAPI API;
    private readonly RemoteClient remoteClient;

    public BackgroundWorker()
    {
        displaysController = new();

        var d = new Dictionary<string, object>();
        
        d.Add("Display", new EntityModel<Display>());
        d.Add("Content", new EntityModel<Content>());
        d.Add("Queue", new EntityModel<Queue>());
        d.Add("SchedulerEntity", new EntityModel<SchedulerEntity>());
        
        API = new DeamonAPI(d, displaysController);
        Start();

        remoteClient = new RemoteClient(API);
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