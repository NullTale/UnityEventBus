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
        public virtual void Send<T>(in T e)
        {
            m_Impl.Send(in e);
        }

        public void Subscribe(IListenerBase listener)
        {
            // can contain multiple listeners
            var listeners = listener.ExtractWrappers();

            // add listeners to the bus
            foreach (var listenerWrapper in listeners)
                m_Impl.Subscribe(listenerWrapper);
        }

        public void UnSubscribe(IListenerBase listener)
        {
            // can contain multiple listeners
            var listeners = listener.ExtractWrappers();

            // remove listeners from the bus
            foreach (var listenerWrapper in listeners)
                m_Impl.UnSubscribe(listenerWrapper);
        }

        public void Subscribe(IEventBus bus)
        {
            m_Impl.Subscribe(bus);
        }

        public void UnSubscribe(IEventBus bus)
        {
            m_Impl.UnSubscribe(bus);
        }
    }
}