using System.Collections.Generic;

namespace UnityEventBus
{
    /// <summary> Event messages receiver interface </summary>
    public interface IEventBus
    {
        void Send<T>(in T e);
        
        void Subscribe(IListenerBase listener);
        void UnSubscribe(IListenerBase listener);
        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);
    }

    /// <summary> Implementation </summary>
    public interface IEventBusImpl
    {
        void Send<T>(in T e);

        void Subscribe(ListenerWrapper listener);
        void UnSubscribe(ListenerWrapper listener);
        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);

        IEnumerable<ListenerWrapper> GetListeners();
    }

    /// <summary> Marker interface for event listeners </summary>
    public interface IListenerBase
    {
    }

    /// <summary> Reaction interface </summary>
    public interface IListener<in T> : IListenerBase 
    {
        void React(T e);
    }

    /// <summary> Provides additional options for event listener </summary>
    public interface IListenerOptions
    {
        /// <summary> Listener id, used in logs </summary>
        string      Name { get; }
        /// <summary> Order in listeners queue, same order listeners will be added at the back of the ordered stack </summary>
        int         Priority { get; }
    }
    
    /// <summary> Event messages receiver interface, generic constrained version </summary>
    public interface IEventBus<T>
    {
        void Send(in T e);
        
        void Subscribe(IListener<T> listener);
        void UnSubscribe(IListener<T> listener);

        void Subscribe(IEventBus<T> bus);
        void UnSubscribe(IEventBus<T> bus);

        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);
    }

    /// <summary> Generic constrained implementation </summary>
    public interface IEventBusImpl<T>
    {
        void Send(in T e);

        void Subscribe(ListenerWrapper listener);
        void UnSubscribe(ListenerWrapper listener);

        void Subscribe(IEventBus<T> bus);
        void UnSubscribe(IEventBus<T> bus);

        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);

        IEnumerable<ListenerWrapper> GetListeners();
    }

}