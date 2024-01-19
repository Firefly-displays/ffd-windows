using System;
using System.IO;
using Newtonsoft.Json;

namespace deamon;

public static class Setuper
{
    public static void Setup()
    {
        string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Firefly-Displays");
        SetupDir(appDir);
        SetupDir(Path.Combine(appDir, "Media"));

        foreach  (string entity in new [] {"Content", "Display", "Queue", "QueueTriggerPair", "SchedulerEntity"})
        {
            string filePath = Path.Combine(appDir, entity + ".json");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
        }
        
        SetupCreds();
    }

    private static void SetupDir(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Logger.Log($"directory {directoryPath} created");
        }
        catch (Exception ex)
        {
            Logger.Log("Directory setup error");
            Logger.Log(ex.ToString());
        }
    }

    private static void SetupCreds()
    {
        var credsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Firefly-Displays", "credentials.txt");
        
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
