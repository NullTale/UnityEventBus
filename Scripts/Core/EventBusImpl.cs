using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEventBus.Utils;


namespace UnityEventBus
{
    /// <summary>
    /// EventBus functionality
    /// </summary>
    internal class EventBusImpl : IEventBusImpl
    {
        private const int k_DefaultSetSize = 4;

        public static readonly IComparer<ISubscriberWrapper> k_OrderComparer = new SubscriberWrapperComparer();

        private Dictionary<Type, SortedCollection<SubscriberWrapper>> m_Subscribers = new Dictionary<Type, SortedCollection<SubscriberWrapper>>();
        private SortedCollection<SubscriberBusWrapper>                m_Buses       = new SortedCollection<SubscriberBusWrapper>(k_OrderComparer);
        private int                                                   m_AddIndex;

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
            where TInvoker : IEventInvoker
        {
            var hasListeners = m_Subscribers.TryGetValue(typeof(TEvent), out var listeners) && listeners.Count > 0;
            var hasBusses    = m_Buses.Count > 0;
            
            if (hasListeners && hasBusses)
            {
                var buses = m_Buses.m_Collection.ToArray();
                var subs = listeners.m_Collection.ToArray();

                var busIndex = 0;
                var subIndex = 0;

                var bus = buses[0];
                var sub = subs[0];

                while (true)
                {
                    // skip inactive subs without invoking
                    if (sub.IsActive == false)
                    {
                        if (++ subIndex >= subs.Length)
                        {
                            bus.Invoke(in e, in invoker);
                            while (++ busIndex < buses.Length)
                            {
                                buses[busIndex].Invoke(in e, in invoker);
                            }
                            break;
                        }

                        sub = subs[subIndex];
                        continue;
                    }

                    if (bus.IsActive == false)
                    {
                        if (++ busIndex >= buses.Length)
                        {
                            sub.Invoke(in e, in invoker);
                            while (++ subIndex < subs.Length)
                            {
                                subs[subIndex].Invoke(in e, in invoker);
                            }

                            break;
                        }

                        bus = buses[busIndex];
                        continue;
                    }

                    if (sub.Order == bus.Order ? sub.Index < bus.Index : sub.Order < bus.Order)
                    {
                        // invoke listener, move next, if no more listeners invoke remaining buses
                        sub.Invoke(in e, in invoker);
                        if (++ subIndex >= subs.Length)
                        {
                            bus.Invoke(in e, in invoker);
                            while (++ busIndex < buses.Length)
                            {
                                buses[busIndex].Invoke(in e, in invoker);
                            }
                            break;
                        }

                        sub = subs[subIndex];
                    }
                    else
                    {
                        // invoke bus, move next, if no more buses invoke remaining listeners
                        bus.Invoke(in e, in invoker);
                        if (++ busIndex >= buses.Length)
                        {
                            sub.Invoke(in e, in invoker);
                            while (++ subIndex < subs.Length)
                            {
                                subs[subIndex].Invoke(in e, in invoker);
                            }

                            break;
                        }

                        bus = buses[busIndex];
                    }
                }
            }
            else if (hasBusses)
                foreach (var bus in m_Buses.m_Collection.ToArray())
                    bus.Invoke(in e, in invoker);
            else if (hasListeners)
                foreach (var bus in listeners.m_Collection.ToArray())
                    bus.Invoke(in e, in invoker); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(ISubscriber subscriber)
        {
            if (subscriber is IEventBus bus) 
                Subscribe(bus);
            else
                foreach (var wrapper in subscriber.ExtractWrappers())
                    Subscribe(wrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(ISubscriber subscriber)
        {
            if (subscriber is IEventBus bus) 
                UnSubscribe(bus);
            else
                foreach (var wrapper in subscriber.ExtractWrappers())
                    UnSubscribe(wrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(SubscriberWrapper subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            subscriber.Index = m_AddIndex ++;

            // get or create group
            if (m_Subscribers.TryGetValue(subscriber.Key, out var set) == false)
            {
                set = new SortedCollection<SubscriberWrapper>(k_OrderComparer, k_DefaultSetSize);
                m_Subscribers.Add(subscriber.Key, set);
            }

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

            m_Buses.Add(SubscriberBusWrapper.Create(in bus, m_AddIndex ++));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            var busWrapper = SubscriberBusWrapper.Create(in bus, m_AddIndex ++);
            if (m_Buses.Extract(busWrapper, out var extracted))
                extracted.Dispose();

            busWrapper.Dispose();
        }

        public IEnumerable<ISubscriberWrapper> GetSubscribers()
        {
            return m_Subscribers.SelectMany<KeyValuePair<Type, SortedCollection<SubscriberWrapper>>, object>(group => group.Value).Concat(m_Buses).OfType<ISubscriberWrapper>();
        }

        public void Dispose()
        {
            foreach (var wrappers in m_Subscribers.Values)
            foreach (var wrapper in wrappers)
                wrapper.Dispose();
        }
    }
}