using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEventBus
{
    internal sealed class SubscriberBusWrapper : IDisposable, ISubscriberWrapper
    {
        internal static Stack<SubscriberBusWrapper> s_WrappersPool = new Stack<SubscriberBusWrapper>(512);
        private ISubscriberOptions m_Options;

        internal bool        IsActive;
        public   IEventBus   Bus;
        public   string      Name   => m_Options.Name;
        public   ISubscriber Target => Bus;
        public   int         Order  => m_Options.Priority;
        public   int         Index  { get; private set; }


        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
#if  DEBUG
            Bus.Send(in e, in invoker);
#else
            try
            {
                Bus.Send(in e, in invoker);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"{this}; Exception: {exception}");
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubscriberBusWrapper(in IEventBus bus, int index)
        {
            Setup(bus, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup(in IEventBus bus, int index)
        {
            Index    = index;
            IsActive = true;
            Bus      = bus;
            m_Options = bus as ISubscriberOptions ?? Extensions.s_DefaultSubscriberOptions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return Bus == ((SubscriberBusWrapper)obj).Bus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Bus.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            IsActive  = false;
            Bus       = null;
            m_Options = null;
            s_WrappersPool.Push(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SubscriberBusWrapper Create(in IEventBus bus, int index)
        {
            if (s_WrappersPool.Count > 0)
            {
                var wrapper = s_WrappersPool.Pop();
                wrapper.Setup(in bus, index);
                return wrapper;
            }
            else
                return new SubscriberBusWrapper(in bus, index);
        }

        public override string ToString()
        {
            return $"Bus: <Name> {Name}, <Type> {Bus.GetType()}";
        }
    }
}