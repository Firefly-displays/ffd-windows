using System.Collections.Generic;
using deamon.Models;

namespace deamon;

public interface IDeamonAPI
{
    List<T> GET<T>() where T : Entity;
    T GET<T>(string id) where T : Entity;
    void POST<T>(T entity) where T : Entity;
    void UPDATE<T>(T entity) where T : Entity;
    void DELETE<T>(string id) where T : Entity;
    
    void RestartDisplay(string displayId);
    void PauseDisplay(string displayId);
    void ResumeDisplay(string displayId);
    void SkipContent(string displayId);
}