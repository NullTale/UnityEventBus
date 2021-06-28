using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEventBus
{
    public class EventBus : EventBusBase
    {
        [SerializeField]
        private SubscriptionTarget m_SubscribeTo;

        private List<IEventBus>     m_Subscriptions;

        //////////////////////////////////////////////////////////////////////////
        [Serializable] [Flags]
        private enum SubscriptionTarget
        {
            None = 0,
            /// <summary> EventSystem singleton </summary>
            Global = 1,
            /// <summary> First parent EventBus </summary>
            FirstParent = 1 << 1,
        }

        //////////////////////////////////////////////////////////////////////////
        protected virtual void Awake()
        {
            m_Subscriptions = new List<IEventBus>();

            if (m_SubscribeTo.HasFlag(SubscriptionTarget.Global) && GlobalBus.Instance != null)
                m_Subscriptions.Add(GlobalBus.Instance);

            if (m_SubscribeTo.HasFlag(SubscriptionTarget.FirstParent) && transform.parent != null)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Subscriptions.Add(firstParent);
            }
        }

        protected virtual void OnEnable()
        {
            // if implements listener interface manage self
            if (this is IListenerBase el)
                Subscribe(el);

            foreach (var bus in m_Subscriptions)
                bus.Subscribe(this);
        }

        protected virtual void OnDisable()
        {
            if (this is IListenerBase el)
                UnSubscribe(el);

            foreach (var bus in m_Subscriptions)
                bus.UnSubscribe(this);
        }

        //////////////////////////////////////////////////////////////////////////
        [ContextMenu("Log listeners", false, 0)]
        public void LogListeners()
        {
            foreach (var listener in m_Impl.GetListeners())
                Debug.Log($"Name: {listener.Name} Type: {listener.Listener.GetType()} Key: {listener.KeyType}");
        }
    }
}