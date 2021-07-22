using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEventBus.Utils;
using UnityEngine;

namespace UnityEventBus
{
    /// <summary>
    /// EventBus singleton
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GlobalBus : MonoBehaviour, IEventBus
    {
        private static GlobalBus s_Instance;
        public static GlobalBus  Instance
        {
            get
            {
#if !UNITY_EVENT_BUS_DISABLE_AUTO_INITIALIZATION
                if (s_Instance.IsNull())
                    Create(false, false);
#endif

                return s_Instance;
            }
            private set
            {
                if (s_Instance == value)
                    return;

                s_Instance = value;

                // instance discarded
                if (s_Instance.IsNull())
                    return;

                // need to parse all assemblies (refactor this?)
                // not tested with AOT, questionable usefulness, confusional behavior
                if (Instance.CollectClasses || Instance.CollectFunctions)
                {
                    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(n => n.GetTypes()).ToArray();

                    // create listener instances
                    if (Instance.CollectClasses)
                    {
                        foreach (var type in types)
                        {
                            var attribure = type.GetCustomAttribute<ListenerAttribute>();
                            // not null & active
                            if (attribure != null && attribure.Active)
                            {
                                // must be creatable class
                                if (type.IsAbstract || type.IsClass == false || type.IsGenericType)
                                    continue;

                                // must implement event listener interface
                                if (typeof(IListenerBase).IsAssignableFrom(type) == false)
                                    continue;

                                // create & register listener
                                try
                                {
                                    if (typeof(MonoBehaviour).IsAssignableFrom(type))
                                    {
                                        // listener is monobehaviour type
                                        var el = new GameObject(attribure.Name, type).GetComponent(type) as MonoBehaviour;
                                        el.transform.SetParent(Instance.transform);

                                        Subscribe(el as IListenerBase);
                                    }
                                    else
                                    {
                                        // listener is class
                                        var el = (IListenerBase)Activator.CreateInstance(type);
                                        Subscribe(el);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(e);
                                }
                            }
                        }
                    }

                    // create static function listeners
                    if (Instance.CollectFunctions)
                    {
                        foreach (var type in types)
                        {
                            // check all static methods
                            foreach (var methodInfo in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                            {
                                try
                                {
                                    // must be static
                                    if (methodInfo.IsStatic == false)
                                        continue;

                                    // not generic
                                    if (methodInfo.IsGenericMethod)
                                        continue;

                                    var attribure = methodInfo.GetCustomAttribute<ListenerAttribute>();
                                    // not null & active attribute
                                    if (attribure == null || attribure.Active == false)
                                        continue;

                                    var args = methodInfo.GetParameters();

                                    // must have input parameter
                                    if (args.Length != 1)
                                        continue;

                                    // create & register listener
                                    var keyType = args[0].ParameterType;
                                    var el = Activator.CreateInstance(
                                        typeof(ListenerStaticFunction<>).MakeGenericType(keyType),
                                        attribure.Name, methodInfo, attribure.Order) as IListenerBase;
                                    Subscribe(el);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(e);
                                }
                            }
                        }
                    }
                }
            }
        }

        public const object k_DefaultEventData = null;
        public const int    k_DefaultOrder     = 0;
        public const string k_DefaultName      = "";

        private IEventBusImpl m_Impl;

        public bool CollectClasses;
        public bool CollectFunctions;
        public bool InitOnAwake;

        // =======================================================================
        private abstract class ListenerActionBase<T> : IListener<T>, IListenerOptions
        {
            protected string            m_Name;
            protected int               m_Order;

            public string               Name => m_Name;
            public int                  Priority => m_Order;

            // =======================================================================
            public abstract void React(T e);
            
            protected ListenerActionBase(string name, int order)
            {
                m_Name                 = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;
                m_Order                = order;
            }
        }

        private class ListenerStaticFunction<T> : ListenerActionBase<T>
        {
            private ProcessDelagate    m_Action;
            
            // =======================================================================
            private delegate void ProcessDelagate(T e);
            
            // =======================================================================
            public override void React(T e)
            {
                // if key matches invoke action
                m_Action(e);
            }

            public ListenerStaticFunction(string name, MethodInfo method, int order)
                : base(name, order)
            {
                // proceed call
                m_Action = (ProcessDelagate)Delegate.CreateDelegate(typeof(ProcessDelagate), method);

                // set defaults from method info
                if (string.IsNullOrEmpty(name))
                    m_Name = method.Name;
            }
        }
        
        // =======================================================================
        private void Awake()
        {
            if (InitOnAwake)
            {
                Init(new EventBusImpl());
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Init(IEventBusImpl impl)
        {
            if (s_Instance != null && s_Instance != this)
                throw new NotSupportedException("Can't initialize EventSystem singleton twice.");

            if (impl == null)
                throw new ArgumentNullException(nameof(impl));

            // set implementation
            m_Impl = impl;

            // set instance
            Instance = this;
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
                Instance = null;

            if (m_Impl != null)
            {
                m_Impl.Dispose();
                m_Impl = null;
            }
        }

        // =======================================================================
        void IEventBus.Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
        {
            Send(in e, in invoker);
        }

        void IEventBus.Subscribe(IListenerBase listener)
        {
            Subscribe(listener);
        }

        void IEventBus.UnSubscribe(IListenerBase listener)
        {
            UnSubscribe(listener);
        }

        void IEventBus.Subscribe(IEventBus bus)
        {
            Subscribe(bus);
        }

        void IEventBus.UnSubscribe(IEventBus bus)
        {
            UnSubscribe(bus);
        }

        public IEnumerable<ListenerWrapper> GetListeners() => m_Impl.GetListeners();

        // =======================================================================
        /// <summary> Create and initialize EventSystem singleton game object, if singleton already created nothing will happen </summary>
        public static void Create(bool collectClasses, bool collectFunctions)
        {
            if (s_Instance != null)
                return;

            var go = new GameObject(nameof(GlobalBus));
            DontDestroyOnLoad(go);

            var es = go.AddComponent<GlobalBus>();
            es.CollectClasses = collectClasses;
            es.CollectFunctions = collectFunctions;

            es.Init(new EventBusImpl());
        }

        public static void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        { 
            Instance.m_Impl.Send(in e, in invoker);
        }
        
        public static void Send<TEvent>(in TEvent e)
        { 
            Instance.Send(in e);
        }
        
        /// <summary> Send IEvent message </summary>
        public static void SendEvent<TKey>(in TKey key)
        { 
            Instance.SendEvent(in key);
        }

        /// <summary> Send IEventData message </summary>
        public static void SendEvent<TKey, Data>(in TKey key, in Data data)
        {
            Instance.SendEvent(in key, data);
        }

        /// <summary> Send IEventData message </summary>
        public static void SendEvent<TKey>(in TKey key, params object[] data)
        {
            Instance.SendEvent(new EventData<TKey, object[]>(key, data));
        }

	    public static void Subscribe(IListenerBase listener)
	    {
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
                Instance.m_Impl.Subscribe(listenerWrapper);
        }

        public static void Subscribe(IEventBus bus)
        {
            Instance.m_Impl.Subscribe(bus);
        }

        public static void UnSubscribe(IListenerBase listener)
	    {
#if UNITY_EDITOR
            if (s_Instance == null)
                return;
#endif
            // allow multiply listeners in one
            var listeners = listener.ExtractWrappers();

            // push listeners in to the message system
            foreach (var listenerWrapper in listeners)
		        Instance.m_Impl.UnSubscribe(listenerWrapper);
	    }
        
        public static void UnSubscribe(IEventBus bus)
        {
#if UNITY_EDITOR
            if (s_Instance == null)
                return;
#endif
            Instance.m_Impl.UnSubscribe(bus);
        }
        
        // =======================================================================
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void _domainReloadCapability()
        {
            s_Instance = null;
        }

        // =======================================================================
        [ContextMenu("Log listeners")]
        public void LogListeners()
        {
            foreach (var listener in m_Impl.GetListeners())
                Debug.Log($"Name: {listener.Name}\n" +
                          $"Type: {listener.Listener.GetType()}\n" +
                          $"Key: {listener.KeyType}\n");
        }
    }
}