using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEventBus.Utils;

namespace UnityEventBus
{
    public static class Extensions
    {
        internal static        Dictionary<Type, Type[]> s_ListenersTypesCache = new Dictionary<Type, Type[]>();
        internal static        MethodInfo               s_SendMethod          = typeof(IEventBus).GetMethod(nameof(IEventBusImpl.Send), BindingFlags.Instance | BindingFlags.Public);
        public static readonly IEventInvoker            s_EventInvokerDefault = new EventInvokerDefault();

        // =======================================================================
        public class EventInvokerDefault : IEventInvoker
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke<TEvent>(in TEvent e, in IListener<TEvent> listener)
            {
                listener.React(e);
            }
        }

        // =======================================================================
        public static TData GetData<TData>(this IEventBase e)
        {
            // try get data
            return ((IEventData<TData>)e).Data;
        }
        
        public static bool TryGetData<TData>(this IEventBase e, out TData data)
        {
            // try get data
            if (e is IEventData<TData> eventData)
            {
                data = eventData.Data;
                return true;
            }

            data = default;
            return false;
        }

        public static void SendEvent<TKey>(this IListener<IEvent<TKey>> receiver, in TKey key)
        {
            receiver.React(new Event<TKey>(in key));
        }

        public static void SendEvent<TKey, TData>(this IListener<IEvent<TKey>> receiver, in TKey key, in TData data)
        {
            receiver.React(new EventData<TKey, TData>(in key, in data));
        }

        public static void SendEvent<TKey>(this IListener<IEvent<TKey>> receiver, in TKey key, params object[] data)
        {
            receiver.React(new EventData<TKey, object[]>(in key, in data));
        }


        public static void Send<TEvent>(this IEventBus bus, in TEvent e)
        {
            bus.Send(e, s_EventInvokerDefault);
        }

        public static void SendEvent<TKey>(this IEventBus bus, in TKey key)
        {
            bus.Send<IEvent<TKey>>(new Event<TKey>(in key));
        }

        public static void SendEvent<TKey, TData>(this IEventBus bus, in TKey key, in TData data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, TData>(in key, in data));
        }
        
        public static void SendEvent<TKey>(this IEventBus bus, in TKey key, params object[] data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, object[]>(in key, in data));
        }
        
        public static bool SendRequest<TKey>(this IEventBus bus, in TKey key)
        {
            IRequest<TKey> request = new EventRequest<TKey>(in key);
            bus.Send<IRequest<TKey>>(in request);
            return request.IsApproved;
        }

        public static bool SendRequest<TKey, TData>(this IEventBus bus, in TKey key, in TData data)
        {
            IRequest<TKey> request = new EventDataRequest<TKey, TData>(in key, in data);
            bus.Send<IRequest<TKey>>(request);
            return request.IsApproved;
        }

        public static bool SendRequest<TKey>(this IEventBus bus, in TKey key, params object[] data)
        {
            IRequest<TKey> request = new EventDataRequest<TKey, object[]>(in key, in data);
            bus.Send<IRequest<TKey>>(request);
            return request.IsApproved;
        }

        internal static IEnumerable<ListenerWrapper> ExtractWrappers(this IListenerBase listener)
        {
            var listenerType = listener.GetType();

            // try get cache
            if (s_ListenersTypesCache.TryGetValue(listenerType, out var types))
                return types.Select(type => ListenerWrapper.Create(listener, type));

            // extract
            types = listenerType.GetInterfaces()
                                .Where(it => it.Implements<IListenerBase>() && it != typeof(IListenerBase))
                                .ToArray();

            // add to cache
            s_ListenersTypesCache.Add(listenerType, types);
            return types.Select(type => ListenerWrapper.Create(listener, type));
        }

        #region Deconstructors
        public static (T1, T2) GetData<T1, T2>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1]);
        }

        public static (T1, T2, T3) GetData<T1, T2, T3>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2]);
        }

        public static (T1, T2, T3, T4) GetData<T1, T2, T3, T4>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3]);
        }

        public static bool TryGetData<T1, T2>(this IEventBase e, out T1 dataA, out T2 dataB)
        {
            // safe deconstruction version
            if (e.TryGetData(out object[] dataArray) == false || dataArray.Length < 4)
            {
                dataA = default;
                dataB = default;
                return false;
            }

            try 
            { 
                dataA = (T1)dataArray[0];
                dataB = (T2)dataArray[1];
                return true;
            }
            catch 
            {
                dataA = default;
                dataB = default;
                return false;
            }
        }

        public static bool TryGetData<T1, T2, T3>(this IEventBase e, out T1 dataA, out T2 dataB, out T3 dataC)
        {
            // safe deconstruction version
            if (e.TryGetData(out object[] dataArray) == false || dataArray.Length < 4)
            {
                dataA = default;
                dataB = default;
                dataC = default;
                return false;
            }

            try 
            { 
                dataA = (T1)dataArray[0];
                dataB = (T2)dataArray[1];
                dataC = (T3)dataArray[2];
                return true;
            }
            catch 
            {
                dataA = default;
                dataB = default;
                dataC = default;
                return false;
            }
        }

        public static bool TryGetData<T1, T2, T3, T4>(this IEventBase e, out T1 dataA, out T2 dataB, out T3 dataC, out T4 dataD)
        {
            // safe deconstruction version
            if (e.TryGetData(out object[] dataArray) == false || dataArray.Length < 4)
            {
                dataA = default;
                dataB = default;
                dataC = default;
                dataD = default;
                return false;
            }

            try 
            { 
                dataA = (T1)dataArray[0];
                dataB = (T2)dataArray[1];
                dataC = (T3)dataArray[2];
                dataD = (T4)dataArray[3];
                return true;
            }
            catch 
            {
                dataA = default;
                dataB = default;
                dataC = default;
                dataD = default;
                return false;
            }
        }
        #endregion

        #region Questionable
        // rare in use, can cause problems in AOT build
        /*
        public static void SendDynamic(this IEventBus Bus, object key)
        {
            // cache created methods
            SendMethod.MakeGenericMethod(key.GetType()).Invoke(Bus, new []{ key });
        }

        public static void SendEventDynamic(this IEventBus Bus, object key)
        {
            try 
            {
                var message = Activator.CreateInstance(typeof(Event<>).MakeGenericType(key.GetType()), key);
                SendMethod.MakeGenericMethod(typeof(IEvent<>).MakeGenericType(key.GetType()))
                          .Invoke(Bus, new[] { message });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public static void SendEventDynamic(this IEventBus Bus, object key, object data)
        {
            try
            {
                var message = Activator.CreateInstance(typeof(EventData<,>).MakeGenericType(key.GetType(), data.GetType()), key,
                                                       data);
                SendMethod.MakeGenericMethod(typeof(IEvent<>).MakeGenericType(key.GetType()))
                          .Invoke(Bus, new[] { message });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public static void SendEventDynamic(this IEventBus Bus, object key, params object[] data)
        {
            try
            {
                var message = Activator.CreateInstance(typeof(EventData<,>).MakeGenericType(key.GetType(), data.GetType()), key,
                                                       data);
                SendMethod.MakeGenericMethod(typeof(IEvent<>).MakeGenericType(key.GetType()))
                          .Invoke(Bus, new[] { message });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        */
        #endregion
        
    }
}