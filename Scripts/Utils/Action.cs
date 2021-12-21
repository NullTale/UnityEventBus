using System;
using System.Runtime.CompilerServices;

namespace UnityEventBus
{
    /// <summary> SendAction target interface </summary>
    public interface IHandle<THandle> : ISubscriber<IHandleInvoker<THandle>>, ISubscriber
    {
    }

    /// <summary> Invocation interface without generic constraints </summary>
    public interface IHandleInvoker
    {
        void Invoke(ISubscriber listener);
    }

    /// <summary> Event key interface </summary>
    public interface IHandleInvoker<THandle> : IHandleInvoker
    {
    }

    internal class HandleInvoker<THandle> : IHandleInvoker<THandle>
    {
        private Action<THandle> m_Action;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ISubscriber listener)
        {
            m_Action.Invoke((THandle)listener);
        }

        public HandleInvoker(Action<THandle> action)
        {
            m_Action = action;
        }

        public override string ToString()
        {
            return $"Action({typeof(THandle)})";
        }
    }
}