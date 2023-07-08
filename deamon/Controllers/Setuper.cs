using System;
using System.IO;

namespace deamon;

public static class Setuper
{
    public static void Setup()
    {
        SetupDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue"));
        SetupDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "Media"));
    }

    public static void SetupDir(string directoryPath)
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
}
