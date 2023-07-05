using System;
using System.IO;

namespace deamon;

public static class Logger
{
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "log.txt");
        // AppDomain.CurrentDomain.BaseDirectory, "log.txt");

    public static void Log(string msg)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now} - {msg}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }

    public static string GetLogs()
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VideoQueue", "log.txt");
        int numberOfLines = 1000;

        if (!File.Exists(filePath))
        {
            return "Файл не существует.";
        }

        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length <= numberOfLines)
        {
            return string.Join(Environment.NewLine, lines);
        }
        else
        {
            string[] lastLines = new string[numberOfLines];
            Array.Copy(lines, lines.Length - numberOfLines, lastLines, 0, numberOfLines);
            return string.Join(Environment.NewLine, lastLines);
        }
    }
}