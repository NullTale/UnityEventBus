using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEventBus
{
    /// <summary>
    /// Subscriber container, helper class
    /// </summary>
    internal sealed class SubscriberWrapper : IDisposable
    {
        internal static Stack<SubscriberWrapper> s_WrappersPool = new Stack<SubscriberWrapper>(512);

        public static readonly IComparer<SubscriberWrapper> k_OrderComparer = new OrderComparer();

        private ISubscriberOptions m_Options;

        internal bool          IsActive;
        public   Type          Key;
        public   ISubscriber   Subscriber;
        public   string        Name     => m_Options.Name;
        public   int           Order    => m_Options.Priority;

        // =======================================================================
        private sealed class OrderComparer : IComparer<SubscriberWrapper>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(SubscriberWrapper x, SubscriberWrapper y)
            {
                return x.Order - y.Order;
            }
        }

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubscriberWrapper(ISubscriber listener, Type type)
        {
            Setup(listener, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup(ISubscriber listener, Type type)
        {
            IsActive   = true;
            Subscriber = listener;
            m_Options  = listener as ISubscriberOptions ?? Extensions.s_DefaultSubscriberOptions;
            Key        = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return Subscriber == ((SubscriberWrapper)obj).Subscriber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Subscriber.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            IsActive   = false;
            Subscriber = null;
            m_Options  = null;
            s_WrappersPool.Push(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SubscriberWrapper Create(ISubscriber listener, Type key)
        {
            if (s_WrappersPool.Count > 0)
            {
                var wrapper = s_WrappersPool.Pop();
                wrapper.Setup(listener, key);
                return wrapper;
            }
            else
                return new SubscriberWrapper(listener, key);
        }

        public override string ToString()
        {
            return $"Sub: <Name> {Name}, <Type> {Subscriber.GetType()}, <Key> {Key}";
        }
    }
}