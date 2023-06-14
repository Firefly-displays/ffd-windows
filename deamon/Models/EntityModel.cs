using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Quartz;

namespace deamon.Models;

public class EntityModel<T> where T : Entity
{
    public ObservableCollection<T> Data;
    private readonly string configPath = 
        @"C:\Users\onere\Documents\VideoQueue\deamon\deamon\Resources\"+ typeof(T).Name + ".json";
    
    public EntityModel()
    {
        LoadFromFile();
        Data!.CollectionChanged += (sender, args) => SaveToFile();
        foreach (T element in Data)
        {
            element.PropertyChanged += (sender, args) => SaveToFile();
        }
    }

    private void LoadFromFile()
    {
        try
        {
            var fileContent = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<List<T>>(fileContent);
            if (config != null) Data = new ObservableCollection<T>(config);
            else Data = new ObservableCollection<T>();
        }
        catch (Exception e)
        {
            Console.WriteLine(JsonConvert.SerializeObject(e));
            Data = new ObservableCollection<T>();
        }
    }

    private void SaveToFile()
    {
        File.WriteAllText(configPath, JsonConvert.SerializeObject(Data));
    }
}