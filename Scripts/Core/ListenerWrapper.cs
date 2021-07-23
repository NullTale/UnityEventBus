using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEventBus
{
    /// <summary>
    /// Listener container, helper class
    /// </summary>
    public sealed class ListenerWrapper : IDisposable
    {
        internal static Stack<ListenerWrapper> s_WrappersPool = new Stack<ListenerWrapper>(512);

        public static readonly IComparer<ListenerWrapper> k_OrderComparer = new OrderComparer();

        private IListenerOptions m_Options;

        internal bool          IsActive;
        public   Type          KeyType;
        public   IListenerBase Listener;
        public   string        Name     => m_Options.Name;
        public   int           Order    => m_Options.Priority;

        // =======================================================================
        private sealed class OrderComparer : IComparer<ListenerWrapper>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(ListenerWrapper x, ListenerWrapper y)
            {
                return x.Order - y.Order;
            }
        }

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ListenerWrapper(IListenerBase listener, Type type)
        {
            Setup(listener, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup(IListenerBase listener, Type type)
        {
            IsActive  = true;
            Listener  = listener;
            m_Options = listener as IListenerOptions ?? Extensions.s_DefaultListenerOptions;
            KeyType   = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return Listener == ((ListenerWrapper)obj).Listener;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Listener.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            IsActive = false;
            Listener = null;
            m_Options  = null;
            s_WrappersPool.Push(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListenerWrapper Create(IListenerBase listener, Type type)
        {
            if (s_WrappersPool.Count > 0)
            {
                var wrapper = s_WrappersPool.Pop();
                wrapper.Setup(listener, type);
                return wrapper;
            }
            else
                return new ListenerWrapper(listener, type);
        }
    }
}