using System.Linq;

namespace UnityEventBus
{
    /// <summary> Base event helper class </summary>
    public interface IEventBase {}

    /// <summary> Event with key data </summary>
    /// <typeparam name="T"> Type of event key </typeparam>
    public interface IEvent<out T> : IEventBase
    {
        T Key           { get; }
    }

    /// <summary> Event with key and custom data </summary>
    /// <typeparam name="T"> Type of event data </typeparam>
    public interface IEventData<out T> : IEventBase
    {
        T Data { get; }
    }

    /// <summary> Base event class </summary>
    internal class Event<T> : IEvent<T>
    {
        public T    Key { get; }

        //////////////////////////////////////////////////////////////////////////
        public Event(in T key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    /// <summary> Event with data </summary>
    internal class EventData<K, D> : Event<K>, IEventData<D>
    {
        public D Data { get; }

        //////////////////////////////////////////////////////////////////////////
        public EventData(in K key, in D data) 
            : base(in key)
        {
            Data = data;
        }

        public override string ToString()
        {
            return $"{Key} {(typeof(D) == typeof(object[]) ? (Data as object[])?.Aggregate("", (s, o) => s + " " + o) : " " + Data)}";
        }
    }
}