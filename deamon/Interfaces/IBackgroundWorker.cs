using System.Collections.Generic;
using deamon.Models;

namespace deamon;

public interface IBackgroundWorker
{
    void Start();
    void Stop();
    void Restart();
    void Restart(List<Display> displays);
    void Pause(Display display);
}