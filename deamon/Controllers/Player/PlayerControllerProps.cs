using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using deamon.Models;
using Quartz;

namespace deamon;

public sealed partial class PlayerController
{
    private IScheduler _scheduler;
    
    private ObservableCollection<QueueWithPriority> CurrentQueues;
    private Queue CurrentQueue { get; set; }
    
    private Queue DefaultQueue { get; set; }

    private Content _CurrectContent;

    private Content CurrentContent
    {
        get => _CurrectContent;
        set
        {
            _CurrectContent = value;
            CurrentContentSrc = value.Path;
            CurrentContentDuration = value.Duration;
            CurrentContentIsVideo = value.Type == Content.ContentType.Video;

            if (!CurrentContentIsVideo)
            {
                var t = new Timer(_ => PickContent(), null, CurrentContentDuration * 1000, Timeout.Infinite);
            }
            
            OnPropertyChanged();
        }
    }

    private string _CurrentContentSrc;
    public string CurrentContentSrc
    {
        get => _CurrentContentSrc; 
        set
        {
            _CurrentContentSrc = value;
            OnPropertyChanged();
        }
    }
    
    private int _CurrentContentDuration;
    public int CurrentContentDuration
    {
        get => _CurrentContentDuration; 
        set
        {
            _CurrentContentDuration = value;
            OnPropertyChanged();
        }
    }
    
    private bool _CurrentContentIsVideo;
    public bool CurrentContentIsVideo
    {
        get => _CurrentContentIsVideo; 
        set
        {
            _CurrentContentIsVideo = value;
            OnPropertyChanged();
        }
    }
    
    
    public enum PlayerState
    {
        Playing,
        Paused
    }
    
    private PlayerState _State;
    public PlayerState State
    {
        get => _State; 
        set
        {
            _State = value;
            OnPropertyChanged(nameof(_State));
        }
    }
    public Display Display { get; set; }

    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}