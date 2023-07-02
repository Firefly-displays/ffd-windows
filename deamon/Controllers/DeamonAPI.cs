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
    private readonly Dictionary<string, object> EntityModels;
    private DisplaysController DisplaysController { get; set; }

    public DeamonAPI(Dictionary<string, object> EntityModels, DisplaysController displaysController)
    {
        // if (EntityModels.Any(x => x.Value.GetType().BaseType != typeof(EntityModel<>)))
        // {
        //     throw new ArgumentException("EntityModels must be a dictionary of type <string, EntityModel<>> but got " +
        //                                 "dictionary of type <string, " + EntityModels.First(x => x.Value.GetType().BaseType != typeof(EntityModel<>)).Value.GetType().Name + ">");
        // }
        
        this.EntityModels = EntityModels;
        this.DisplaysController = displaysController;
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