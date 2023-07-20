using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using deamon.Entities;
using deamon.Models;
using Newtonsoft.Json;

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

        var id = entity.Id;
        
        if (typeof(T) == typeof(Queue))
        {
            foreach (var schedulerEntity in GET<SchedulerEntity>())
            {
                if (schedulerEntity.DefaultQueue?.Id == id)
                {
                    schedulerEntity.DefaultQueue = entity as Queue;
                    UPDATE(schedulerEntity);
                }

                foreach (var el in schedulerEntity.QueueTriggerPairs)
                {
                    if (el.Queue.Id == id)
                    {
                        el.Queue = entity as Queue;
                        UPDATE(schedulerEntity);
                    }
                }
            }
        }
        
        else if (typeof(T) == typeof(Content))
        {
            foreach (var queue in GET<Queue>())
            {
                var contentList = queue.ContentList;
                bool updated = false;
                
                for (var i = 0; i < queue.ContentList.Count; i++)
                {
                    var content = queue.ContentList[i];
                    if (content.Id == id)
                    {
                        queue.ContentList[i] = entity as Content;
                        contentList[i] = entity as Content;
                        updated = true;
                    }
                }

                if (updated)
                {
                    queue.ContentList = new List<Content>(contentList);
                    UPDATE(queue);
                }
            }
        }
    }

    public void DELETE<T>(string id) where T : Entity
    {
        (EntityModels[typeof(T).Name] as EntityModel<T>)!.Delete(id);

        if (typeof(T) == typeof(SchedulerEntity))
        {
            foreach (var display in GET<Display>())
            {
                if (display.SchedulerEntityId == id)
                {
                    display.SchedulerEntityId = null;
                    UPDATE(display);
                }
            }
        }

        if (typeof(T) == typeof(Queue))
        {
            foreach (var schedulerEntity in GET<SchedulerEntity>())
            {
                schedulerEntity.QueueTriggerPairs = schedulerEntity.QueueTriggerPairs
                    .Where(x => x.Queue.Id != id).ToList();
                if (schedulerEntity.DefaultQueue?.Id == id)
                {
                    schedulerEntity.DefaultQueue = null;
                }
                UPDATE(schedulerEntity);
            }
        }
        
        if (typeof(T) == typeof(Content))
        {
            foreach (var queue in GET<Queue>())
            {
                queue.ContentList = queue.ContentList
                    .Where(x => x.Id != id).ToList();
                UPDATE(queue);
            }
        }
    }

    public void RunDisplay(string displayId)
    {
        try
        {
            DisplaysController.OpenPlayer(displayId);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            Logger.Log(e.ToString());
        }
    }

    public void StopDisplay(string displayId)
    {
        try
        {
            DisplaysController.StopPlayer(displayId);


            Thread thread = Thread.CurrentThread;

            Debug.WriteLine("DeamonAPI.StopDisplay()");
            Debug.WriteLine(thread.Name);
            Logger.Log("DeamonAPI.StopDisplay()");


            if (thread.GetApartmentState() == ApartmentState.STA)
            {
                Console.WriteLine("Текущий поток является STA-потоком.");
            }
            else
            {
                Console.WriteLine("Текущий поток является MTA-потоком.");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            Logger.Log(e.ToString());
        }
    }

    public void RestartDisplay(string displayId) => DisplaysController.Restart(displayId);
    public void PauseDisplay(string displayId) => DisplaysController.PausePlayer(displayId);
    public void ResumeDisplay(string displayId) => DisplaysController.ResumePlayer(displayId);
    public void SkipContent(string displayId) => DisplaysController.SkipContent(displayId);
    public void Identify() => DisplaysController.Identify();
}