using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEventBus
{
    public sealed class BusWrapper : IDisposable
    {
        internal static Stack<BusWrapper> s_WrappersPool = new Stack<BusWrapper>(512);

        public static readonly  IComparer<BusWrapper> k_OrderComparer  = new OrderComparer();

        private IListenerOptions m_Options;

        internal bool          IsActive;
        public   IEventBus     Bus;
        public   string        Name  => m_Options.Name;
        public   int           Order => m_Options.Priority;

        // =======================================================================
        private sealed class OrderComparer : IComparer<BusWrapper>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(BusWrapper x, BusWrapper y)
            {
                return x.Order - y.Order;
            }
        }

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BusWrapper(in IEventBus bus)
        {
            Setup(bus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup(in IEventBus bus)
        {
            IsActive = true;
            Bus  = bus;
            m_Options = bus as IListenerOptions ?? Extensions.s_DefaultListenerOptions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return Bus == ((BusWrapper)obj).Bus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Bus.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            IsActive = false;
            Bus  = null;
            m_Options = null;
            s_WrappersPool.Push(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BusWrapper Create(in IEventBus bus)
        {
            if (s_WrappersPool.Count > 0)
            {
                var wrapper = s_WrappersPool.Pop();
                wrapper.Setup(in bus);
                return wrapper;
            }
            else
                return new BusWrapper(in bus);
        }
    }
}