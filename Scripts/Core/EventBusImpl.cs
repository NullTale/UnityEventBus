using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEventBus.Utils;


namespace UnityEventBus
{
    /// <summary>
    /// EventBus functionality
    /// </summary>
    internal class EventBusImpl : IEventBusImpl
    {
        private const int k_DefaultSetSize = 4;

        private Dictionary<Type, SortedCollection<SubscriberWrapper>> m_Subscribers = new Dictionary<Type, SortedCollection<SubscriberWrapper>>();
        private SortedCollection<BusWrapper>                          m_Buses       = new SortedCollection<BusWrapper>(BusWrapper.k_OrderComparer);

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
            where TInvoker : IEventInvoker
        {
            // propagate, invoke listeners
            if (m_Subscribers.TryGetValue(typeof(TEvent), out var set))
            {
                // set can be modified through execution
                foreach (var wrapper in set.ToArray())
                {
#if  DEBUG
                    if (wrapper.IsActive)
                        invoker.Invoke(in e, in wrapper.Subscriber);
#else
                    try
                    {
                        if (wrapper.IsActive)
                            invoker.Invoke(in e, in wrapper.Subscriber);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"{wrapper}; Exception: {exception}");
                    }
#endif
                }
            }

            // to buses
            if (m_Buses.Count != 0)
            {
                foreach (var bus in m_Buses.ToArray())
                {
                    if (bus.IsActive)
                        bus.Bus.Send(in e, in invoker);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(SubscriberWrapper subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            // get or create group
            if (m_Subscribers.TryGetValue(subscriber.Key, out var set) == false)
            {
                set = new SortedCollection<SubscriberWrapper>(SubscriberWrapper.k_OrderComparer, k_DefaultSetSize);
                m_Subscribers.Add(subscriber.Key, set);
            }

            //if (set.Contains(subscriber) == false)
            set.Add(subscriber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(SubscriberWrapper subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            // remove first match
            if (m_Subscribers.TryGetValue(subscriber.Key, out var set))
            {
                // remove & dispose
                if (set.Extract(in subscriber, out var extracted))
                    extracted.Dispose();

                if (set.Count == 0)
                    m_Subscribers.Remove(subscriber.Key);
            }

            subscriber.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_Buses.Add(BusWrapper.Create(in bus));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            var busWrapper = BusWrapper.Create(in bus);
            if (m_Buses.Extract(busWrapper, out var extracted))
                extracted.Dispose();

            busWrapper.Dispose();
        }

        public IEnumerable<object> GetSubscribers()
        {
            return m_Subscribers.SelectMany<KeyValuePair<Type, SortedCollection<SubscriberWrapper>>, object>(group => group.Value).Concat(m_Buses);
        }

        public void Dispose()
        {
            foreach (var wrappers in m_Subscribers.Values)
            foreach (var wrapper in wrappers)
                wrapper.Dispose();
        }
    }
}