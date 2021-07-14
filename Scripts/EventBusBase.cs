using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEventBus
{
    /// <summary>
    /// EventBus MonoBehaviour class, without auto subscription logic
    /// </summary>
    public abstract class EventBusBase : MonoBehaviour, IEventBus
    {
        protected EventBusImpl m_Impl = new EventBusImpl();

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Send<T>(in T e)
        {
            m_Impl.Send(in e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IListenerBase listener)
        {
            // can contain multiple listeners
            var listeners = listener.ExtractWrappers();

            // add listeners to the bus
            foreach (var listenerWrapper in listeners)
                m_Impl.Subscribe(listenerWrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IListenerBase listener)
        {
            // can contain multiple listeners
            var listeners = listener.ExtractWrappers();

            // remove listeners from the bus
            foreach (var listenerWrapper in listeners)
                m_Impl.UnSubscribe(listenerWrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IEventBus bus)
        {
            m_Impl.Subscribe(bus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IEventBus bus)
        {
            m_Impl.UnSubscribe(bus);
        }
    }
    
    /// <summary> Type constrained version </summary>
    public abstract class EventBusBase<T> : MonoBehaviour, IEventBus<T>
    {
        protected EventBusImpl<T> m_Impl = new EventBusImpl<T>();

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Send(in T e)
        {
            m_Impl.Send(in e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IListener<T> listener)
        {
            m_Impl.Subscribe(listener.ExtractWrapper());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IListener<T> listener)
        {
            m_Impl.UnSubscribe(listener.ExtractWrapper());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IEventBus<T> bus)
        {
            m_Impl.Subscribe(bus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IEventBus<T> bus)
        {
            m_Impl.UnSubscribe(bus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IEventBus bus)
        {
            m_Impl.Subscribe(bus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IEventBus bus)
        {
            m_Impl.UnSubscribe(bus);
        }
    }
}