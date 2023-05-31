namespace deamon.Models;
using NReco.VideoInfo;

public class Content
{
    public enum ContentType
    {
        Video,
        Image
    }

    public ContentType type;
    public string path;
    public int duration;
    
    public Content(ContentType type, string path, int duration = 0)
    {
        this.type = type;
        this.path = path;
        if (type == ContentType.Video)
        {
            this.duration = GetDuration();
        }
        else
        {
            this.duration = duration;
        }
    }

    private int GetDuration()
    {
        var ffProbe = new FFProbe();
        var videoInfo = ffProbe.GetMediaInfo(path);
        return (int) videoInfo.Duration.TotalSeconds;
    }
}