using System;
using System.Collections.Generic;

namespace UnityEventBus
{
    /// <summary> Event messages receiver interface </summary>
    public interface IEventBus
    {
        void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker;
        
        void Subscribe(IListenerBase listener);
        void UnSubscribe(IListenerBase listener);
        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);
    }

    /// <summary> Invokes events on the listener </summary>
    public interface IEventInvoker
    {
        void Invoke<TEvent>(in TEvent e, in IListener<TEvent> listener);
    }

    /// <summary> Implementation </summary>
    public interface IEventBusImpl : IDisposable
    {
        void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker;

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