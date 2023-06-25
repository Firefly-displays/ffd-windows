using System;
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
    
    public string Name { get => _name; set => SetField(ref _name, value); }
    private string _name;
    
    public string? ThumbPath { get => _thumbPath; set => SetField(ref _thumbPath, value); }
    private string? _thumbPath;

    public Content(string? name, ContentType type, string path, string? thumb, int duration = 0) 
        : this(name, null, type, path, thumb, duration) {}
    
    
    [JsonConstructor]
    public Content(string? name, string? id, ContentType type, string path, string? thumb, int duration = 0) : base(id)
    {
        Name = name ?? System.IO.Path.GetFileName(path);
        Type = type;
        Path = path;
        Duration = type == ContentType.Video ? GetDuration() : duration;
        ThumbPath = thumb;
    }

    private int GetDuration()
    {
        var ffProbe = new FFProbe();
        var videoInfo = ffProbe.GetMediaInfo(Path);
        return (int) videoInfo.Duration.TotalSeconds;
    }

    public string GetBaseThumb()
    {
        if (ThumbPath == null) return "";
        var bytes = System.IO.File.ReadAllBytes(ThumbPath);
        return Convert.ToBase64String(bytes);
    }
}