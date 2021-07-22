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

        private static readonly DefaultOptions             k_DefaultOptions = new DefaultOptions();
        public static readonly  IComparer<ListenerWrapper> k_OrderComparer  = new OrderComparer();

        private IListenerBase    m_Listener;
        private IListenerOptions m_Options;
        private Type             m_KeyType;

        internal bool          IsActive;
        public   Type          KeyType  => m_KeyType;
        public   IListenerBase Listener => m_Listener;
        public   string        Name     => m_Options.Name;
        public   int           Order    => m_Options.Priority;

        // =======================================================================
        private class DefaultOptions : IListenerOptions
        {
            public string Name     => string.Empty;
            public int    Priority => 0;
        }
        
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
            m_Listener = listener;
            m_Options  = listener as IListenerOptions ?? k_DefaultOptions;
            m_KeyType  = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return m_Listener == ((ListenerWrapper)obj).Listener;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return m_Listener.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            m_Listener = null;
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