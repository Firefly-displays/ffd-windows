using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using deamon.Models;
using deamon.Views;

namespace deamon;

public class MyViewModel: INotifyPropertyChanged
{
    public MyViewModel()
    {
        MyItems = new ObservableCollection<QueueWithPriority>();
        ShowMyView();
    }
    
    private ObservableCollection<QueueWithPriority> _myItems;

    public ObservableCollection<QueueWithPriority> MyItems
    {
        get { return _myItems; }
        set
        {
            _myItems = value;
            OnPropertyChanged(nameof(MyItems));
        }
    }

    private MyView _myView;

    public void ShowMyView()
    {
        MyItems.Add(new QueueWithPriority(new Queue("first"), 1));
        MyItems.Add(new QueueWithPriority(new Queue("second"), 0));
        
        _myView = new MyView();
        _myView.DataContext = this; // установим контекст данных текущего экземпляра MyViewModel
        _myView.Show();
        
        Thread.Sleep(5*1000);
        MyItems.Add(new QueueWithPriority(new Queue("third"), 0));
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}