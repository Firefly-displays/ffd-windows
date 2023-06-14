using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace deamon.Models;

public class Queue
{
    public List<Content> ContentList { get; set; }
    public string Name { get; set; }
    
    public void AddContent(Content content)
    {
        ContentList.Add(content);
    }

    public Queue(string name)
    {
        this.ContentList = new List<Content>();
        this.Name = name;
    }
    
    public Queue(List<Content> contentList, string name)
    {
        ContentList = contentList;
        Name = name;
    }
}