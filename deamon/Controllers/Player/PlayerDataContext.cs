using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using deamon.Models;
using Newtonsoft.Json;

namespace deamon;

public sealed class PlayerDataContext: INotifyPropertyChanged
{
    public PlayerDataContext()
    {
        CurrentQueue = null;
    }

    private Queue? _currentQueue;

    public Queue? CurrentQueue
    {
        get => _currentQueue;
        set
        {
            Debug.WriteLine("setting current queue - " + JsonConvert.SerializeObject(value));
            SetField(ref _currentQueue, value);
        }
    }

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
