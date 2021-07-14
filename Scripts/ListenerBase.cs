using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEventBus
{
    /// <summary> Base class for EventListener & Listener MonoBehavior </summary>
    public abstract class ListenerBase : MonoBehaviour, IListenerBase, IListenerOptions
    {
        [SerializeField] [Tooltip("Subscription targets")]
        private SubscriptionTarget m_SubscribeTo;
        [SerializeField] [Tooltip("Listener priority, lowest first, same last")]
        private int             m_Priority;
        //[SerializeField]  // must be readonly
        private bool            m_Connected;

        private List<IEventBus> m_Buses = new List<IEventBus>();

        public List<IEventBus> Subscriptions => m_Buses;

        public string Name => gameObject.name;
        public int Priority
        {
            get => m_Priority;
            set
            {
                if (m_Priority == value)
                    return;

                m_Priority = value;

                // reconnect if order was changed
                if (m_Connected)
                    _reconnect();
            }
        }

        // =======================================================================
        [Serializable] [Flags]
        private enum SubscriptionTarget
        {
            None = 0,
            /// <summary> EventBus singleton </summary>
            Global = 1,
            /// <summary> First parent EventBus </summary>
            FirstParent = 1 << 1,
            /// <summary> This gameObject EventBus </summary>
            This = 1 << 2,
        }

        // =======================================================================
        protected virtual void Awake()
        {
            _buildSubscriptionList();
            m_Connected = false;
        }

        protected virtual void OnEnable()
        {
            _connectListener();
        }

        protected virtual void OnDisable()
        {
            _disconnectListener();
        }

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _connectListener()
        {
            // connect if disconnected
            if (m_Connected)
                return;

            foreach (var bus in m_Buses)
                bus.Subscribe(this);

            m_Connected = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _disconnectListener()
        {
            // disconnect if connected
            if (m_Connected == false)
                return;

            foreach (var bus in m_Buses)
                bus.UnSubscribe(this);

            m_Connected = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _buildSubscriptionList()
        {
            // EventSystem singleton
            if (m_SubscribeTo.HasFlag(SubscriptionTarget.Global) && ReferenceEquals(GlobalBus.Instance, null) == false)
                m_Buses.Add(GlobalBus.Instance);

            // first parent EventBus
            if (m_SubscribeTo.HasFlag(SubscriptionTarget.FirstParent) && ReferenceEquals(transform.parent, null) == false)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Buses.Add(firstParent);
            }

            // self if has IEventBus component
            if (m_SubscribeTo.HasFlag(SubscriptionTarget.This))
            {
                if (transform.TryGetComponent<IEventBus>(out var thisBus))
                    m_Buses.Add(thisBus);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _reconnect()
        {
            _disconnectListener();
            _connectListener();
        }
    }
}