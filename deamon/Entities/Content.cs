using Newtonsoft.Json;

namespace deamon.Models;
using NReco.VideoInfo;

public class Content : Entity
{
    public enum ContentType
    {
        Video,
        Image
    }

    public ContentType Type { get => _type; set => SetField(ref _type, value); }
    private ContentType _type;
    
    public string Path { get => _path; set => SetField(ref _path, value); }
    private string _path;
    
    public int Duration { get => _duration; set => SetField(ref _duration, value); }
    private int _duration;

    public Content(ContentType type, string path, int duration = 0) : this(null, type, path, duration) {}
    [JsonConstructor]
    public Content(string? id, ContentType type, string path, int duration = 0) : base(id)
    {
        Type = type;
        Path = path;
        Duration = type == ContentType.Video ? GetDuration() : duration;
    }

    private int GetDuration()
    {
        var ffProbe = new FFProbe();
        var videoInfo = ffProbe.GetMediaInfo(Path);
        return (int) videoInfo.Duration.TotalSeconds;
    }
}