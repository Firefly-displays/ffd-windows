using System;
using System.IO;
using Newtonsoft.Json;
using System.IO.Abstractions;
using deamon.Utils;

namespace deamon;

public class Setuper : FSTestable
{
    public Setuper(IFileSystem fs) : base(fs) { }
    public Setuper() { }
    
    public void Setup()
    {
        string appDir = Path.Combine(Environment.GetEnvironmentVariable("AppFolder")!, "Firefly-Displays");
        SetupDir(appDir);
        SetupDir(Path.Combine(appDir, "Media"));

        foreach  (string entity in new [] {"Content", "Display", "Queue", "QueueTriggerPair", "SchedulerEntity"})
        {
            string filePath = Path.Combine(appDir, entity + ".json");

            if (!FS.File.Exists(filePath))
            {
                FS.File.WriteAllText(filePath, "[]");
            }
        }
        
        SetupCreds();
    }

    private void SetupDir(string directoryPath)
    {
        try
        {
            if (!FS.Directory.Exists(directoryPath))
            {
                FS.Directory.CreateDirectory(directoryPath);
            }

            Logger.Log($"directory {directoryPath} created");
        }
        catch (Exception ex)
        {
            Logger.Log("Directory setup error");
            Logger.Log(ex.ToString());
        }
    }

    private void SetupCreds()
    {
        var credsFilePath = Path.Combine(Environment.GetEnvironmentVariable("AppFolder")!, "Firefly-Displays", "credentials.txt");
        
        if (!File.Exists(credsFilePath))
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(credsFilePath, true))
                {
                    var random = new Random();
                    writer.WriteLine(random.Next(1000, 9999));
                    writer.WriteLine(random.Next(1000, 9999));
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to set creds");
                Logger.Log(JsonConvert.SerializeObject(ex));
            }
        }
    }
}
