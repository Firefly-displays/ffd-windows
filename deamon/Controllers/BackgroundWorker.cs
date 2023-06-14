using System;
using System.Collections.Generic;
using System.Diagnostics;
using deamon.Models;

namespace deamon;

public class BackgroundWorker : IBackgroundWorker
{
    private DisplaysController displaysController;
    public BackgroundWorker()
    {
        displaysController = new();
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
            displaysController.StopPlayer(display);
        }
    }
    
    public void Pause(Display display)
    {
        displaysController.PausePlayer(display);
    }
}