namespace deamon.Models;
using NReco.VideoInfo;

public class Content
{
    public enum ContentType
    {
        Video,
        Image
    }

    public ContentType Type { get; set;}
    public string Path { get; set;}
    public int Duration { get; set;}
    
    public Content(ContentType type, string path, int duration = 0)
    {
        this.Type = type;
        this.Path = path;
        if (type == ContentType.Video)
        {
            this.Duration = GetDuration();
        }
        else
        {
            this.Duration = duration;
        }
    }

    private int GetDuration()
    {
        var ffProbe = new FFProbe();
        var videoInfo = ffProbe.GetMediaInfo(Path);
        return (int) videoInfo.Duration.TotalSeconds;
    }
}