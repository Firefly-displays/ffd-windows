using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using deamon.Entities;
using deamon.Models;

namespace deamon;

public class DeamonAPI: IDeamonAPI
{
    private static DeamonAPI Instance;
    
    public static DeamonAPI GetInstance()
    {
        if (Instance == null)
        {
            Instance = new DeamonAPI();
        }

        return Instance;
    }
    
    private readonly Dictionary<string, object> EntityModels;
    private DisplaysController DisplaysController { get; set; }

    private DeamonAPI()
    {
        var d = new Dictionary<string, object>();

        d.Add("Display", EntityModel<Display>.GetInstance());
        d.Add("Content", EntityModel<Content>.GetInstance());
        d.Add("Queue", EntityModel<Queue>.GetInstance());
        d.Add("SchedulerEntity", EntityModel<SchedulerEntity>.GetInstance());
        
        this.EntityModels = d;
        this.DisplaysController = DisplaysController.GetInstance();
    }
    
    public List<T> GET<T>() where T: Entity
    {
        return (EntityModels[typeof(T).Name] as EntityModel<T>)!.GetAll();
    }

    public T GET<T>(string id) where T : Entity
    {
        return (EntityModels[typeof(T).Name] as EntityModel<T>)!.GetById(id);
    }

    public void POST<T>(T entity) where T : Entity
    {
        (EntityModels[typeof(T).Name] as EntityModel<T>)!.Add(entity);
    }

    public void UPDATE<T>(T entity) where T : Entity
    {
        (EntityModels[typeof(T).Name] as EntityModel<T>)!.Update(entity);
    }

    public void DELETE<T>(string id) where T : Entity
    {
        (EntityModels[typeof(T).Name] as EntityModel<T>)!.Delete(id);
    }

    public void RunDisplay(string displayId)
    {
        try { DisplaysController.OpenPlayer(displayId); }
        catch (Exception e) { Debug.WriteLine(e); }
    }

    public void StopDisplay(string displayId)
    {
        try
        {
            DisplaysController.StopPlayer(displayId);
            
            
            Thread thread = Thread.CurrentThread;
            
            Debug.WriteLine("DeamonAPI.StopDisplay()");
            Debug.WriteLine(thread.Name);

            if (thread.GetApartmentState() == ApartmentState.STA)
            {
                Console.WriteLine("Текущий поток является STA-потоком.");
            }
            else
            {
                Console.WriteLine("Текущий поток является MTA-потоком.");
            }
        }
        catch (Exception e) { Debug.WriteLine(e); }
    }

    public void RestartDisplay(string displayId) => DisplaysController.Restart(displayId);
    public void PauseDisplay(string displayId) => DisplaysController.PausePlayer(displayId);
    public void ResumeDisplay(string displayId) => DisplaysController.ResumePlayer(displayId);
    public void SkipContent(string displayId) => DisplaysController.SkipContent(displayId);
}