using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using deamon.Models;
using Quartz;

namespace deamon;

public sealed partial class PlayerController
{
    private IScheduler _scheduler;
    
    private ObservableCollection<QueueWithPriority> CurrentQueues;
    public Queue? CurrentQueue { get; set; }
    
    private Queue? DefaultQueue { get; set; }

    private Content _CurrectContent;

    public Content CurrentContent
    {
        get => _CurrectContent;
        set
        {
            _CurrectContent = value;
            if (value.Type == Content.ContentType.Video)
            {
                CurrentContentVideoSrc = value.Path;
                CurrentContentImgSrc =  Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "black.jpg");
            }
            else
            {
                CurrentContentVideoSrc = "";
                CurrentContentImgSrc = value.Path;
            }
            CurrentContentDuration = value.Duration;
            CurrentContentIsVideo = value.Type == Content.ContentType.Video;

            if (!CurrentContentIsVideo)
            {
                var t = new Timer(_ => PickContent(), null, CurrentContentDuration * 1000, Timeout.Infinite);
            }
            
            OnPropertyChanged();
        }
    }

    private string _CurrentContentImgSrc;
    public string CurrentContentImgSrc
    {
        get => _CurrentContentImgSrc; 
        set
        {
            _CurrentContentImgSrc = value;
            OnPropertyChanged();
        }
    }
    
    private string _CurrentContentVideoSrc;
    public string CurrentContentVideoSrc
    {
        get => _CurrentContentVideoSrc; 
        set
        {
            _CurrentContentVideoSrc = value;
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
            CurrentContentIsImg = !value;
            OnPropertyChanged();
        }
    }
    
    private bool _CurrentContentIsImg;
    public bool CurrentContentIsImg
    {
        get => _CurrentContentIsImg; 
        set
        {
            _CurrentContentIsImg = value;
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
            OnPropertyChanged();
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