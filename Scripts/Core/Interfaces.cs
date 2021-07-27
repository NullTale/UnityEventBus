using System;
using System.Collections.Generic;

namespace UnityEventBus
{
    /// <summary> Event messages receiver interface </summary>
    public interface IEventBus
    {
        void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker;
        
        void Subscribe(ISubscriber subscriber);
        void UnSubscribe(ISubscriber subscriber);
        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);
    }

    /// <summary> Implementation </summary>
    internal interface IEventBusImpl : IDisposable
    {
        void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker;

        void Subscribe(SubscriberWrapper subscriber);
        void UnSubscribe(SubscriberWrapper subscriber);
        void Subscribe(IEventBus bus);
        void UnSubscribe(IEventBus bus);

        IEnumerable<object> GetSubscribers();
    }

    /// <summary> Invokes events on the listener </summary>
    public interface IEventInvoker
    {
        void Invoke<TEvent>(in TEvent e, in ISubscriber listener);
    }

    /// <summary> Container interface without generic constraints </summary>
    public interface ISubscriber
    {
    }
    
    /// <summary> Marker interface, basically event key container in form of generic argument </summary>
    public interface ISubscriber<TEvent>
    {
    }

    /// <summary> Reaction interface </summary>
    public interface IListener<TEvent> : ISubscriber<TEvent>, ISubscriber
    {
        void React(in TEvent e);
    }

    /// <summary> Provides additional options for event listener </summary>
    public interface ISubscriberOptions
    {
        /// <summary> Listener id, used in logs </summary>
        string      Name { get; }
        /// <summary> Order in listeners queue, lower first, same order listeners will be added at the back of the ordered stack </summary>
        int         Priority { get; }
    }
}