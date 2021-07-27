using UnityEngine;

namespace UnityEventBus
{
    /// <summary>
    /// EventBus MonoBehaviour class, without auto subscription logic
    /// </summary>
    public class EventBusBase : MonoBehaviour, IEventBus
    {
        private IEventBusImpl m_Impl = new EventBusImpl();

        // =======================================================================
        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
            m_Impl.Send(in e, in invoker);
        }

        public void Subscribe(ISubscriber subscriber)
        {
            // add listeners to the bus
            foreach (var wrapper in subscriber.ExtractWrappers())
                m_Impl.Subscribe(wrapper);
        }

        public void UnSubscribe(ISubscriber subscriber)
        {
            foreach (var wrapper in subscriber.ExtractWrappers())
                m_Impl.UnSubscribe(wrapper);
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
        
        // =======================================================================
        [ContextMenu("Log subscribers", false, 0)]
        public void LogSubscribers()
        {
            foreach (var subscriber in m_Impl.GetSubscribers())
                Debug.Log(subscriber.ToString());
        }
    }
}