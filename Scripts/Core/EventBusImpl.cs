using System;
using System.Collections.Generic;
using System.Linq;
using UnityEventBus.Utils;
using UnityEngine;


namespace UnityEventBus
{
    /// <summary>
    /// EventBus functionality
    /// </summary>
    public class EventBusImpl : IEventBusImpl
    {
        private const int k_DefaultSetSize = 1;

        private Dictionary<Type, SortedCollection<ListenerWrapper>> m_Listeners = new Dictionary<Type, SortedCollection<ListenerWrapper>>();

        private HashSet<IEventBus> m_Buses = new HashSet<IEventBus>();

        //////////////////////////////////////////////////////////////////////////
        public void Send<T>(in T e)
        {
            // propagate, invoke listeners
            if (m_Listeners.TryGetValue(typeof(IListener<T>), out var set))
            {
                // set can be modified through execution
                foreach (var listenerWrapper in set.ToArray())
                {
                    try
                    {
                        listenerWrapper.React(in e);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"Listener: id: {listenerWrapper.Name}, key: {listenerWrapper.KeyType}; Exception: {exception}");
                    }
                }
            }

            // to buses
            if (m_Buses.Count != 0)
            {
                foreach (var bus in m_Buses.ToArray())
                {
                    bus.Send(in e);
                }
            }
        }

        public void Subscribe(ListenerWrapper listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            // activate
            listener.IsActive = true;

            // get or create group
            if (m_Listeners.TryGetValue(listener.KeyType, out var set) == false)
            {
                set = new SortedCollection<ListenerWrapper>(ListenerWrapper.k_OrderComparer, k_DefaultSetSize);
                m_Listeners.Add(listener.KeyType, set);
            }

            if (set.Contains(listener) == false)
                set.Add(listener);
        }

        public void UnSubscribe(ListenerWrapper listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            // deactivate
            listener.IsActive = false;

            // remove first match
            if (m_Listeners.TryGetValue(listener.KeyType, out var set))
            {
                set.Remove(listener);
                if (set.Count == 0)
                    m_Listeners.Remove(listener.KeyType);
            }
        }

        public void Subscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_Buses.Add(bus);
        }

        public void UnSubscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_Buses.Remove(bus);
        }

        public IEnumerable<ListenerWrapper> GetListeners()
        {
            return m_Listeners.SelectMany(group => group.Value);
        }
    }

    /// <summary>
    /// Type constrained version of implementation
    /// </summary>
    public class EventBusImpl<T> : IEventBusImpl<T>
    {
        private SortedCollection<ListenerWrapper> m_Listeners = new SortedCollection<ListenerWrapper>(ListenerWrapper.k_OrderComparer);

        private HashSet<IEventBus<T>> m_Buses        = new HashSet<IEventBus<T>>();
        private HashSet<IEventBus>    m_GenericBuses = new HashSet<IEventBus>();

        //////////////////////////////////////////////////////////////////////////
        public void Send(in T e)
        {
            // invoke listeners
            foreach (var listenerWrapper in m_Listeners.ToArray())
            {
                try
                {
                    listenerWrapper.React(in e);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Listener: id: {listenerWrapper.Name}, key: {listenerWrapper.KeyType}; Exception: {exception}");
                }
            }

            // to buses
            if (m_Buses.Count != 0)
            {
                foreach (var bus in m_Buses.ToArray())
                {
                    bus.Send(in e);
                }
            }

            if (m_GenericBuses.Count != 0)
            {
                foreach (var bus in m_GenericBuses.ToArray())
                {
                    bus.Send(in e);
                }
            }
        }

        public void Subscribe(ListenerWrapper listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            // activate
            listener.IsActive = true;
            m_Listeners.Add(listener);
        }

        public void UnSubscribe(ListenerWrapper listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            // deactivate, must be deactivated in case of execution
            listener.IsActive = false;
            m_Listeners.Remove(listener);
        }

        public void Subscribe(IEventBus<T> bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_Buses.Add(bus);
        }

        public void UnSubscribe(IEventBus<T> bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_Buses.Remove(bus);
        }

        public void Subscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_GenericBuses.Add(bus);
        }

        public void UnSubscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_GenericBuses.Remove(bus);
        }

        public IEnumerable<ListenerWrapper> GetListeners()
        {
            return m_Listeners;
        }
    }
}