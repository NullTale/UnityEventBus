using System.Linq;

namespace UnityEventBus
{
    /// <summary> Base event helper class </summary>
    public interface IEventBase { }

    /// <summary> Event with key only </summary>
    /// <typeparam name="TKey"> Type of event key </typeparam>
    public interface IEvent<out TKey> : IEventBase
    {
        TKey Key           { get; }
    }

    /// <summary> Event with key and custom data </summary>
    /// <typeparam name="TData"> Type of event data </typeparam>
    public interface IEventData<out TData> : IEventBase
    {
        TData Data { get; }
    }

    /// <summary> Base event class </summary>
    internal class Event<TKey> : IEvent<TKey>
    {
        public TKey    Key { get; }

        // =======================================================================
        public Event(in TKey key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    /// <summary> Event with data </summary>
    internal class EventData<TKey, TData> : Event<TKey>, IEventData<TData>
    {
        public TData Data { get; }

        // =======================================================================
        public EventData(in TKey key, in TData data) 
            : base(in key)
        {
            Data = data;
        }

        public override string ToString()
        {
            return $"{Key} {(typeof(TData) == typeof(object[]) ? (Data as object[])?.Aggregate("", (s, o) => s + " " + o) : " " + Data)}";
        }
    }
    
    
    public interface IRequestBase
    {
        bool IsApproved { get; }

        void Approve();
    }

    /// <summary>
    /// Base request class, can be only approved or ignored
    /// </summary>
    public interface IRequest<out TKey>: IRequestBase, IEvent<TKey>
    {
    }
    
    /// <summary>
    /// Request extends IEvent
    /// </summary>
    internal class EventRequest<TKey> : Event<TKey>, IRequest<TKey>
    {
        public  bool   IsApproved { get; private set; }

        // =======================================================================
        public EventRequest(in TKey key) : base(in key) { }

        public void Approve()
        {
            IsApproved = true;
        }
    }

    /// <summary>
    /// Request extends IEventData
    /// </summary>
    internal class EventDataRequest<TKey, TData> : EventData<TKey, TData>, IRequest<TKey>
    {
        public  bool   IsApproved { get; private set; }

        // =======================================================================
        public EventDataRequest(in TKey key, in TData data) : base(in key, in data) { }

        public void Approve()
        {
            IsApproved = true;
        }
    }
    
}