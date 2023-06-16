using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Quartz;

namespace deamon.Models;

public class EntityModel<T> where T : Entity
{
    public ObservableCollection<T> Data;

    public List<T> GetAll() 
    {
        return Data.ToList();
    } 
    
    public T GetById(string id)
    {
        if (Data.All(x => x.Id != id))
        {
            throw new Exception("Entity " + typeof(T).Name + " with id " + id + " not found");
        }
        return Data.First(x => x.Id == id);
    }
    
    public void Add(T entity)
    {
        if (Data.Any(x => x.Id == entity.Id))
        {
            throw new Exception("Entity " + typeof(T).Name + " with id " + entity.Id + " already exists");
        }
        Data.Add(entity);
    }
    
    public void Update(T entity)
    {
        if (Data.All(x => x.Id != entity.Id))
        {
            throw new Exception("Entity " + typeof(T).Name + " with id " + entity.Id + " not found");
        }
        var oldEntity = Data.First(x => x.Id == entity.Id);
        var index = Data.IndexOf(oldEntity);
        Data[index] = entity;
    }

    public void Delete(string id)
    {
        if (Data.All(x => x.Id != id))
        {
            throw new Exception("Entity " + typeof(T).Name + " with id " + id + " not found");
        }
        Data.Remove(Data.First(x => x.Id == id));
    }

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