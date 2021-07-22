using UnityEngine;

namespace UnityEventBus
{
    /// <summary>
    /// EventBus MonoBehaviour class, without auto subscription logic
    /// </summary>
    public abstract class EventBusBase : MonoBehaviour, IEventBus
    {
        protected IEventBusImpl m_Impl = new EventBusImpl();

        // =======================================================================
        public virtual void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
            m_Impl.Send(in e, in invoker);
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

        protected virtual void OnDestroy()
        {
            m_Impl.Dispose();
            m_Impl = null;
        }
    }
}