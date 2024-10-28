using System.Collections.Generic;

namespace hospital_pc_client.Utils;

public delegate void NotifyEventHandler(object sender, NotifyEventArgs e);

public interface INotify
{
    // public NotifyEventHandler _notifyEventHandler { get; set; }

    public List<IObserver> _observers { get; set; }
    
    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
        // _notifyEventHandler += observer.OnNotify;
    }
    
    public void RemoveObserver(IObserver observer)
    {
        _observers.Remove(observer);
        // _notifyEventHandler -= observer.OnNotify;
    }

    public void ClearObservers()
    {
        _observers.Clear();
    }
    
    public void Notify(object sender, NotifyEventArgs e)
    {
        foreach (var observer in _observers)
        {
            observer.OnNotify(sender, e);
        }
        // _notifyEventHandler?.Invoke(sender, e);
    }

}


public interface IObserver
{
    void OnNotify(object sender, NotifyEventArgs e);

    void Subscribe(INotify notify)
    {
        notify.AddObserver(this);
    }
    
    void Unsubscribe(INotify notify)
    {
        notify.RemoveObserver(this);
    }
}

public class NotifyEventArgs
{
    public string EventName { get; set; }
    public object EventData { get; set; }
    
    public NotifyEventArgs(string eventName, object eventData=null)
    {
        EventName = eventName;
        EventData = eventData;
    }
}

public static class ObservableUtil
{
    public static void Subscribe(INotify notify, object observer)
    {
        notify.AddObserver((IObserver)observer);
    }
    
    public static void Unsubscribe(INotify notify, object observer)
    {
        notify.RemoveObserver((IObserver)observer);
    }

    public static void ClearObservers(INotify notify)
    {
        notify.ClearObservers();
    }

    public static void Notify(object notify, object sender, NotifyEventArgs e)
    {
        ((INotify)notify).Notify(sender, e);
    }
    
    public static void Notify(object notify, NotifyEventArgs e)
    {
        ((INotify)notify).Notify(notify, e);
    }

    

}