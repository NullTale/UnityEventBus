using System.Runtime.CompilerServices;

namespace UnityEventBus
{
    public static class RequestExtensions
    {
        public static readonly RequestInvoker s_RequestInvoker = new RequestInvoker();

        // =======================================================================
        public class RequestInvoker: IEventInvoker
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke<TEvent>(in TEvent e, in ISubscriber listener)
            {
                if (((IRequestBase)e).IsApproved)
                    return;

                ((IListener<TEvent>)listener).React(in e);
            }
        }

        // =======================================================================
        // to bus
        public static bool SendRequest<TKey>(this IEventBus bus, in TKey key)
        {
            IRequest<TKey> request = new EventRequest<TKey>(in key);
            bus.Send(in request, in s_RequestInvoker);
            return request.IsApproved;
        }

        public static bool SendRequest<TKey, TData>(this IEventBus bus, in TKey key, in TData data)
        {
            IRequest<TKey> request = new EventDataRequest<TKey, TData>(in key, in data);
            bus.Send(in request, in s_RequestInvoker);
            return request.IsApproved;
        }

        public static bool SendRequest<TKey>(this IEventBus bus, in TKey key, params object[] data)
        {
            IRequest<TKey> request = new EventDataRequest<TKey, object[]>(in key, in data);
            bus.Send(in request, in s_RequestInvoker);
            return request.IsApproved;
        }

        // to listener
        public static bool SendRequest<TKey>(this IListener<TKey> listener, in TKey key)
        {
            IRequest<TKey> request = new EventRequest<TKey>(in key);
            s_RequestInvoker.Invoke(in request, listener);
            return request.IsApproved;
        }

        public static bool SendRequest<TKey, TData>(this IListener<TKey> listener, in TKey key, in TData data)
        {
            IRequest<TKey> request = new EventDataRequest<TKey, TData>(in key, in data);
            s_RequestInvoker.Invoke(in request, listener);
            return request.IsApproved;
        }

        public static bool SendRequest<TKey>(this IListener<TKey> listener, in TKey key, params object[] data)
        {
            IRequest<TKey> request = new EventDataRequest<TKey, object[]>(in key, in data);
            s_RequestInvoker.Invoke(in request, listener);
            return request.IsApproved;
        }
    }
}