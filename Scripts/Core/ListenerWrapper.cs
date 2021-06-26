using System;
using System.Collections.Generic;

namespace UnityEventBus
{
    /// <summary>
    /// Listener container, helper class
    /// </summary>
    public class ListenerWrapper
    {
        private static readonly DefaultOptions             k_DefaultOptions = new DefaultOptions();
        public static readonly  IComparer<ListenerWrapper> k_OrderComparer  = new OrderComparer();

        private readonly IListenerBase    m_Listener;
        private readonly IListenerOptions m_Options;

        internal bool             IsActive;
        public Type               KeyType       { get; }
        public IListenerBase      Listener => m_Listener;
        public string             Name          => m_Options.Name;
        public int                Order         => m_Options.Priority;

        //////////////////////////////////////////////////////////////////////////
        private class DefaultOptions : IListenerOptions
        {
            public string Name     => string.Empty;
            public int    Priority => 0;
        }
        
        private sealed class OrderComparer : IComparer<ListenerWrapper>
        {
            public int Compare(ListenerWrapper x, ListenerWrapper y)
            {
                return x.Order - y.Order;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        public ListenerWrapper(IListenerBase listener, Type type)
        {
            m_Listener = listener;
            m_Options  = listener as IListenerOptions ?? k_DefaultOptions;
            KeyType    = type;
        }

        public override bool Equals(object obj)
        {
            //if (ReferenceEquals(null, obj)) return false;
            //if (ReferenceEquals(this, obj)) return true;
            return m_Listener == ((ListenerWrapper)obj).Listener;
        }

        public override int GetHashCode()
        {
            return (m_Listener != null ? m_Listener.GetHashCode() : 0);
        }
        
        public void React<T>(in T e)
        {
            // listener can be removed through execution
            if (IsActive)
                ((IListener<T>)m_Listener).React(e);
        }

        public void InvokeAction<T>(in Action<T> e) where T : IListenerBase
        {
            if (IsActive)
                e.Invoke((T)m_Listener);
        }
    }
}