using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventBus;

public class Logic
{
    const  string k_ExpectedString = "Test";
    const  int    k_ExpectedInt    = 1812;
    const  float  k_ExpectedFloat  = 1.984f;
    static object k_ExpectedObject = new object();

    // =======================================================================
    public static EventBus _createBus(Transform root = null, int priority = 0)
    {
        var go = new GameObject("Bus");
        go.transform.SetParent(root, false);

        var result = go.AddComponent<EventBus>();
        result.Priority = priority;

        return result;
    }

    public static TListener _createListener<TListener>(Transform root = null, int priority = 0, ListenerBase.SubscriptionTarget subscriptionTarget = ListenerBase.SubscriptionTarget.None) 
        where TListener : ListenerBase
    {
        var go = new GameObject("Listener");
        go.transform.SetParent(root, false);

        var result = go.AddComponent<TListener>();
        result.Priority = priority;
        result.SubscribeTo = subscriptionTarget;

        return result;
    }

    // =======================================================================
    public class SubscribeListener : ListenerBase,
                                IListener<string>

    {
        public string StringIn = string.Empty;

        public void React(in string e)
        {
            StringIn = e;
        }

    }

    public class SubscribeManyListener : ListenerBase,
                                IListener<string>,
                                IListener<int>,
                                IListener<float>,
                                IListener<object>


    {
        public string StringIn = string.Empty;
        public int    IntIn    = 0;
        public float  FloatIn  = 0f;
        public object ObjectIn = null;

        public void React(in string e)
        {
            StringIn = e;
        }

        public void React(in int e)
        {
            IntIn = e;
        }

        public void React(in float e)
        {
            FloatIn = e;
        }

        public void React(in object e)
        {
            ObjectIn = e;
        }
    }

    // =======================================================================
    [Test]
    public void Listener_Subscribe()
    {
        var parentBus    = _createBus();
        var listener     = _createListener<SubscribeListener>(parentBus.transform);
        var listenerMany = _createListener<SubscribeManyListener>(parentBus.transform);
        var thisBus      = listener.gameObject.AddComponent<EventBusBase>();

        listener.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        _check(GlobalBus.Instance);

        listener.SubscribeTo = ListenerBase.SubscriptionTarget.FirstParent;
        _check(parentBus);

        listener.SubscribeTo = ListenerBase.SubscriptionTarget.This;
        _check(thisBus);
        
        listener.SubscribeTo = ListenerBase.SubscriptionTarget.None;
        listener.StringIn = string.Empty;
        GlobalBus.Send(k_ExpectedString);
        Assert.AreNotEqual(k_ExpectedString, listener.StringIn);


        listenerMany.SubscribeTo = ListenerBase.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString);
        GlobalBus.Send(k_ExpectedInt);
        GlobalBus.Send(k_ExpectedFloat);
        GlobalBus.Send(k_ExpectedObject);
        
        Assert.AreEqual(k_ExpectedString, listenerMany.StringIn);
        Assert.AreEqual(k_ExpectedInt, listenerMany.IntIn);
        Assert.AreEqual(k_ExpectedFloat, listenerMany.FloatIn);
        Assert.AreEqual(k_ExpectedObject, listenerMany.ObjectIn);
        
        listenerMany.SubscribeTo = ListenerBase.SubscriptionTarget.None;
        listenerMany.ObjectIn = null;
        GlobalBus.Send(k_ExpectedObject);
        Assert.AreNotEqual(k_ExpectedObject, listenerMany.ObjectIn);

        // ===================================
        void _check(IEventBus bus)
        {
            listener.StringIn = string.Empty;
            bus.Send(k_ExpectedString);
            Assert.AreEqual(k_ExpectedString, listener.StringIn);
        }
    }
    
    public class IntRef
    {
        public int Counter;
    }

    public class PriorityListener : ListenerBase,
                                IListener<IntRef>

    {
        public int      CounterIn;

        // =======================================================================
        public void React(in IntRef e)
        {
            CounterIn = e.Counter ++;
        }
    }
    

    [Test]
    public void Listener_Priority()
    {
        var listener0 = _createListener<PriorityListener>(null, -100);
        var listener1 = _createListener<PriorityListener>(null, 0);
        var listener2 = _createListener<PriorityListener>(null, 0);
        var listener3 = _createListener<PriorityListener>(null, 100);

        listener1.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        listener2.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        listener0.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        listener3.SubscribeTo = ListenerBase.SubscriptionTarget.Global;

        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener0.CounterIn);
        Assert.AreEqual(1, listener1.CounterIn);
        Assert.AreEqual(2, listener2.CounterIn);
        Assert.AreEqual(3, listener3.CounterIn);

        // ===================================
        // priority resubscribe
        listener0.Priority = 100;

        // resubscribe in same order
        listener1.SubscribeTo = ListenerBase.SubscriptionTarget.None;
        listener1.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        
        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener2.CounterIn);
        Assert.AreEqual(1, listener1.CounterIn);
        Assert.AreEqual(2, listener3.CounterIn);
        Assert.AreEqual(3, listener0.CounterIn);
    }

    
    public class GenericListener : ListenerBase,
                                   IListener<string>

    {
        public string                  StringIn = string.Empty;
        public Action<GenericListener> OnReact;

        // =======================================================================
        public void React(in string e)
        {
            StringIn = e;
            OnReact?.Invoke(this);
        }
    }
    
    [Test]
    public void Listener_RuntimeDeactivation()
    {
        var listenerA = _createListener<GenericListener>();
        var listenerB = _createListener<GenericListener>();
        
        listenerA.OnReact = gl => listenerB.SubscribeTo = ListenerBase.SubscriptionTarget.None;

        listenerA.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        listenerB.SubscribeTo = ListenerBase.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString);

        Assert.AreEqual(k_ExpectedString, listenerA.StringIn);
        Assert.AreEqual(string.Empty, listenerB.StringIn);
    }

    
    [Test]
    public void Bus_RuntimeDeactivation()
    {
        var busA = _createBus();
        var busB = _createBus();

        var listenerA = _createListener<GenericListener>(busA.transform);
        var listenerB = _createListener<GenericListener>(busB.transform);

        listenerA.OnReact = gl => busB.SubscribeTo = EventBus.SubscriptionTarget.None;
        listenerA.SubscribeTo = ListenerBase.SubscriptionTarget.FirstParent;
        listenerB.SubscribeTo = ListenerBase.SubscriptionTarget.FirstParent;

        busA.SubscribeTo = EventBus.SubscriptionTarget.Global;
        busB.SubscribeTo = EventBus.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString);

        Assert.AreEqual(k_ExpectedString, listenerA.StringIn);
        Assert.AreEqual(string.Empty, listenerB.StringIn);
    }

    [Test]
    public void Bus_Priority()
    {
        var bus0 = _createBus(null, -100);
        var bus1 = _createBus(null, 0);
        var bus2 = _createBus(null, 0);
        var bus3 = _createBus(null, 100);

        var listener0 = _createListener<PriorityListener>(bus0.transform, 0, ListenerBase.SubscriptionTarget.FirstParent);
        var listener1 = _createListener<PriorityListener>(bus1.transform, 0, ListenerBase.SubscriptionTarget.FirstParent);
        var listener2 = _createListener<PriorityListener>(bus2.transform, 0, ListenerBase.SubscriptionTarget.FirstParent);
        var listener3 = _createListener<PriorityListener>(bus3.transform, 0, ListenerBase.SubscriptionTarget.FirstParent);

        bus3.SubscribeTo = EventBus.SubscriptionTarget.Global;
        bus1.SubscribeTo = EventBus.SubscriptionTarget.Global;
        bus2.SubscribeTo = EventBus.SubscriptionTarget.Global;
        bus0.SubscribeTo = EventBus.SubscriptionTarget.Global;

        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener0.CounterIn);
        Assert.AreEqual(1, listener1.CounterIn);
        Assert.AreEqual(2, listener2.CounterIn);
        Assert.AreEqual(3, listener3.CounterIn);

        GlobalBus.Send(k_ExpectedString);

        // ===================================
        // priority resubscribe
        bus0.Priority = 100;

        // resubscribe in same order
        bus1.SubscribeTo = EventBus.SubscriptionTarget.None;
        bus1.SubscribeTo = EventBus.SubscriptionTarget.Global;
        
        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener2.CounterIn);
        Assert.AreEqual(1, listener1.CounterIn);
        Assert.AreEqual(2, listener3.CounterIn);
        Assert.AreEqual(3, listener0.CounterIn);
    }

    
    public class EventListener : ListenerBase, 
                                IListener<IEvent<string>>

    {
        public string StringIn = string.Empty;
        
        public void React(in IEvent<string> e)
        {
            switch (e.Key)
            {
                case "Key":
                    StringIn = k_ExpectedString;
                    break;

                case "Data":
                    StringIn = e.GetData<string>();
                    break;
                
                case "TryGetData":
                    Assert.True(e.TryGetData(out string sData));
                    StringIn = sData;
                    break;
                
                case "Deconstruction":
                    var (f, i, s) = e.GetData<float, int, string>();
                    StringIn = s;
                    break;
            }
        }
    }

    [Test]
    public void IEvent()
    {
        var listener = _createListener<EventListener>();
        listener.SubscribeTo = ListenerBase.SubscriptionTarget.Global;

        listener.StringIn = string.Empty;
        GlobalBus.SendEvent("Key");
        Assert.AreEqual(k_ExpectedString, listener.StringIn);

        listener.StringIn = string.Empty;
        GlobalBus.SendEvent("Data", k_ExpectedString);
        Assert.AreEqual(k_ExpectedString, listener.StringIn);
        
        listener.StringIn = string.Empty;
        GlobalBus.SendEvent("TryGetData", k_ExpectedString);
        Assert.AreEqual(k_ExpectedString, listener.StringIn);
        
        listener.StringIn = string.Empty;
        GlobalBus.SendEvent("Deconstruction", 0.1f, 1, k_ExpectedString);
        Assert.AreEqual(k_ExpectedString, listener.StringIn);
    }

}