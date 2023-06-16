using System;
using System.Collections.Generic;
using System.Linq;
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

    public void RestartDisplay(string displayId)
    {
        var display = GET<Display>(displayId);
        DisplaysController.Restart(display);
    }

    public void PauseDisplay(string displayId)
    {
        var display = GET<Display>(displayId);
        DisplaysController.PausePlayer(display);
    }

    public void ResumeDisplay(string displayId)
    {
        var display = GET<Display>(displayId);
        DisplaysController.ResumePlayer(display);
    }

    public void SkipContent(string displayId)
    {
        var display = GET<Display>(displayId);
        DisplaysController.SkipContent(display);
    }
}