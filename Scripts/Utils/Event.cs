using System.Linq;

namespace UnityEventBus
{
    /// <summary> Base event helper class </summary>
    public interface IEventBase { }

    /// <summary> Event with key only </summary>
    /// <typeparam name="TKey"> Type of event key </typeparam>
    public interface IEvent<out TKey> : IEventBase
    {
        TKey Key { get; }
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
        public TKey Key { get; }

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
    
    public sealed partial class GlobalBus
    {
        
        /// <summary> Send IEvent message </summary>
        public static void SendEvent<TKey>(in TKey key)
        { 
            Instance.SendEvent(in key);
        }

        /// <summary> Send IEventData message </summary>
        public static void SendEvent<TKey, Data>(in TKey key, in Data data)
        {
            Instance.SendEvent(in key, data);
        }

        /// <summary> Send IEventData message </summary>
        public static void SendEvent<TKey>(in TKey key, params object[] data)
        {
            Instance.SendEvent(key, data);
        }
    }
}