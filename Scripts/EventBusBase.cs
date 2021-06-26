using UnityEngine;

namespace UnityEventBus
{
    /// <summary>
    /// EventBus mono behaviour class
    /// </summary>
    public abstract class EventBusBase : MonoBehaviour, IEventBus
    {
        protected EventBusImpl m_Impl = new EventBusImpl();

        //////////////////////////////////////////////////////////////////////////
        public virtual void Send<T>(in T e)
        {
            m_Impl.Send(in e);
        }

        public void Subscribe(IListenerBase listener)
        {
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
                m_Impl.Subscribe(listenerWrapper);
        }

        public void UnSubscribe(IListenerBase listener)
        {
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
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
    
    /// <summary> Type constrained version </summary>
    public abstract class EventBusBase<T> : MonoBehaviour, IEventBus<T>
    {
        protected EventBusImpl<T> m_Impl = new EventBusImpl<T>();

        //////////////////////////////////////////////////////////////////////////
        public void Send(in T e)
        {
            m_Impl.Send(in e);
        }

        public void Subscribe(IListener<T> listener)
        {
            m_Impl.Subscribe(listener.ExtractWrapper());
        }

        public void UnSubscribe(IListener<T> listener)
        {
            m_Impl.UnSubscribe(listener.ExtractWrapper());
        }

        public void Subscribe(IEventBus<T> bus)
        {
            m_Impl.Subscribe(bus);
        }

        public void UnSubscribe(IEventBus<T> bus)
        {
            m_Impl.UnSubscribe(bus);
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