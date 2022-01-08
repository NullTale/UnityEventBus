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

        internal static Stack<WrappersSet> s_WrappersSetPool = new Stack<WrappersSet>(128);

        public static readonly IComparer<ISubscriberWrapper> k_OrderComparer = new SubscriberWrapperComparer();

        private Dictionary<Type, SortedCollection<SubscriberWrapper>> m_Subscribers = new Dictionary<Type, SortedCollection<SubscriberWrapper>>();
        private SortedCollection<SubscriberBusWrapper>                m_Buses       = new SortedCollection<SubscriberBusWrapper>(k_OrderComparer);
        private int                                                   m_AddIndex;

        // =======================================================================
        internal class WrappersSet : IEnumerable<ISubscriberWrapper>
        {
            public ISubscriberWrapper[]  m_Data = Array.Empty<ISubscriberWrapper>();

            // =======================================================================
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ToStack()
            {
                Array.Clear(m_Data, 0, m_Data.Length);
                s_WrappersSetPool.Push(this);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Init(IReadOnlyList<ISubscriberWrapper> subs, IReadOnlyList<ISubscriberWrapper> buses)
            {
                var busesCount = buses.Count;
                var subsCount  = subs.Count;
                var size       = subsCount + busesCount;

                Array.Resize(ref m_Data, size);

                var subsIndex = 0;
                var busesIndex = 0;
                var n = 0;

                while (true)
                {
                    if (busesIndex == busesCount)
                    {
                        while (subsIndex != subsCount)
                            m_Data[n ++] = subs[subsIndex ++];

                        return;
                    }

                    var bus = buses[busesIndex ++];

                    // has bus, insert listeners
                    while (true)
                    {
                        if (subsIndex == subsCount)
                        {
                            m_Data[n ++] = bus;
                            while (busesIndex != busesCount)
                                m_Data[n ++] = buses[busesIndex ++];

                            return;
                        }

                        var sub = subs[subsIndex];

                        if (sub.Order == bus.Order ? sub.Index < bus.Index : sub.Order <= bus.Order)
                            m_Data[n ++] = sub;
                        else
                        {
                            m_Data[n ++] = bus;
                            break;
                        }

                        subsIndex ++;
                    }
                }
            }

            public IEnumerator<ISubscriberWrapper> GetEnumerator()
            {
                return ((IEnumerable<ISubscriberWrapper>)m_Data).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
            where TInvoker : IEventInvoker
        {
            var hasListeners = false;
            var hasBusses = m_Buses.Count > 0;

            if (m_Subscribers.TryGetValue(typeof(TEvent), out var listeners) && listeners.Count > 0)
                hasListeners = true;
            
            if (hasListeners && hasBusses)
            {
                var subs = s_WrappersSetPool.Count > 0 ? s_WrappersSetPool.Pop() : new WrappersSet();
                subs.Init(listeners.m_Collection, m_Buses.m_Collection);

                foreach (var wrapper in subs)
                    wrapper.Invoke(in e, in invoker);

                subs.ToStack();
            }
            else if (hasBusses)
                foreach (var bus in m_Buses.ToArray())
                    bus.Invoke(in e, in invoker);
            else if (hasListeners)
                foreach (var bus in listeners.ToArray())
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