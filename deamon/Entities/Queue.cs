using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Documents;

namespace deamon.Models;

public class Queue: Entity
{
    
    
    public string Name { get => _Name; set => SetField(ref _Name, value); }
    private string _Name;

    
    public List<Content> ContentList
    {
        get => _contentList.ToList(); 
        set => SetField(ref _contentList,  new ObservableCollection<Content>(value));
    }

    private ObservableCollection<Content> _contentList;
    

    public void AddContent(Content content)
    {
        ContentList.Add(content);
    }

    public Queue(string name) : this(null, name) {}
    public Queue(string? id, string name) : base(id)
    {
        ContentList = new List<Content>();
        Name = name;

        _contentList.CollectionChanged += (sender, args) =>
        {
            OnPropertyChanged(nameof(ContentList));
        };
    }
    
     public Queue(string name, List<Content> contentList) : this(null, name, contentList) {}
     
     [JsonConstructor]
     public Queue(string? id, string name, List<Content> contentList) : base(id)
     {
         ContentList = contentList;
         Name = name;
     }
}