using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEventBus
{
    /// <summary>
    /// MonoBehaviour event bus this auto subscription logic, if implements the IListener interface, subscribes it to itself
    /// </summary>
    public class EventBus : EventBusBase
    {
        [SerializeField]
        private SubscriptionTarget m_SubscribeTo = SubscriptionTarget.None;
        private bool               m_Connected;
        private List<IEventBus>    m_Subscriptions = new List<IEventBus>();

        // =======================================================================
        [Serializable] [Flags]
        public enum SubscriptionTarget
        {
            None = 0,
            /// <summary> EventBus singleton </summary>
            Global = 1,
            /// <summary> First parent EventBus </summary>
            FirstParent = 1 << 1,
        }

        public SubscriptionTarget SubscribeTo
        {
            get => m_SubscribeTo;
            set
            {
                if (m_SubscribeTo == value)
                    return;

                m_SubscribeTo = value;

                _buildSubscriptionList();

                if (m_Connected)
                {
                    _disconnectBus();
                    _buildSubscriptionList();
                    _connectBus();
                }
                else
                    _buildSubscriptionList();
            }
        }

        // =======================================================================
        protected virtual void Awake()
        {
            _buildSubscriptionList();
        }

        protected virtual void OnEnable()
        {
            // if implements listener interface manage self
            if (this is IListenerBase el)
                Subscribe(el);

            _connectBus();
        }

        protected virtual void OnDisable()
        {
            if (this is IListenerBase el)
                UnSubscribe(el);

            _disconnectBus();
        }

        // =======================================================================
        private void _disconnectBus()
        {
            if (m_Connected == false)
                return;

            m_Connected = false;
            
            foreach (var bus in m_Subscriptions)
                bus.UnSubscribe(this);
        }

        private void _connectBus()
        {
            if (m_Connected)
                return;

            m_Connected = true;
            
            foreach (var bus in m_Subscriptions)
                bus.Subscribe(this);
        }

        private void _buildSubscriptionList()
        {
            m_Subscriptions.Clear();

            if (m_SubscribeTo == SubscriptionTarget.None)
                return;

            if (m_SubscribeTo.HasFlag(SubscriptionTarget.Global) && GlobalBus.Instance != null)
                m_Subscriptions.Add(GlobalBus.Instance);

            if (m_SubscribeTo.HasFlag(SubscriptionTarget.FirstParent) && transform.parent != null)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Subscriptions.Add(firstParent);
            }
        }

        // =======================================================================
        [ContextMenu("Log listeners", false, 0)]
        public void LogListeners()
        {
            foreach (var listener in m_Impl.GetListeners())
                Debug.Log($"Name: {listener.Name} Type: {listener.Listener.GetType()} Key: {listener.KeyType}");
        }
    }
}