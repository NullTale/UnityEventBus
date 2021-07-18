using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEventBus.Utils;

namespace UnityEventBus
{
    public static class Extentions
    {
        internal static Dictionary<Type, List<Type>> ListenersTypesCache = new Dictionary<Type, List<Type>>();
        internal static MethodInfo                   SendMethod          = typeof(IEventBus).GetMethod(nameof(IEventBusImpl.Send), BindingFlags.Instance | BindingFlags.Public);

        // =======================================================================
        public static T GetData<T>(this IEventBase e)
        {
            // try get data
            return ((IEventData<T>)e).Data;
        }
        
        public static bool TryGetData<T>(this IEventBase e, out T data)
        {
            // try get data
            if (e is IEventData<T> eventData)
            {
                data = eventData.Data;
                return true;
            }

            data = default;
            return false;
        }

        public static void SendEvent<T>(this IListener<IEvent<T>> receiver, in T key)
        {
            receiver.React(new Event<T>(in key));
        }

        public static void SendEvent<T, D>(this IListener<IEvent<T>> receiver, in T key, in D data)
        {
            receiver.React(new EventData<T, D>(in key, in data));
        }

        public static void SendEvent<T>(this IEventBus Bus, in T key)
        {
            Bus.Send<IEvent<T>>(new Event<T>(in key));
        }

        public static void SendEvent<T, D>(this IEventBus Bus, in T key, in D data)
        {
            Bus.Send<IEvent<T>>(new EventData<T, D>(in key, in data));
        }
        
        public static void SendEvent<T>(this IEventBus Bus, in T key, params object[] data)
        {
            Bus.Send<IEvent<T>>(new EventData<T, object[]>(in key, in data));
        }

        internal static ListenerWrapper ExtractWrapper<T>(this IListener<T> listener)
        {
            return new ListenerWrapper(listener, typeof(IListener<T>));
        }

        internal static IEnumerable<ListenerWrapper> ExtractWrappers(this IListenerBase listener)
        {
            var type = listener.GetType();

            // try get cache
            if (ListenersTypesCache.TryGetValue(type, out var types))
                return types.Select(n => new ListenerWrapper(listener, n));

            // extract
            var extractedTypes = type
                                 .GetInterfaces()
                                 .Where(it =>
                                            it.Implements<IListenerBase>() &&
                                            it != typeof(IListenerBase))
                                 .ToList();

            // add to cache
            ListenersTypesCache.Add(type, extractedTypes);
            return extractedTypes.Select(n => new ListenerWrapper(listener, n));
        }

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
    }
}