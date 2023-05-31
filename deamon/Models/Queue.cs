using System.Collections.Generic;

namespace deamon.Models;

public class Queue
{
    public List<Content> contentList;
    public string Name { get; set; }
    
    public void AddContent(Content content)
    {
        contentList.Add(content);
    }

    public Queue(string name)
    {
        this.contentList = new List<Content>();
        this.Name = name;
    }
    
    public Queue(List<Content> contentList, string name)
    {
        this.contentList = contentList;
        this.Name = name;
    }
}