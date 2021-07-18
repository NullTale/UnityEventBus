using System;
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
    public interface IEventBusImpl : IDisposable
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
        /// <summary> Order in listeners queue, lower first, same order listeners will be added at the back of the ordered stack </summary>
        int         Priority { get; }
    }
}