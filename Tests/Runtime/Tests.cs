using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventBus;

public class Tests
{
    const string k_ExpectedString = "Test";

    // =======================================================================
    public class SubscribeListener : ListenerBase,
                                IListener<string>

    {
        public string StringIn = string.Empty;

        public void React(string e)
        {
            StringIn = e;
        }
    }

    // =======================================================================
    [Test]
    public void Listener_Subscribe()
    {
        var parentBus = _createBus();
        var listener = _createListener<SubscribeListener>(parentBus.transform);
        var thisBus = listener.gameObject.AddComponent<EventBusBase>();

        listener.SubscribeTo = ListenerBase.SubscriptionTarget.Global;
        _check(GlobalBus.Instance);

        listener.SubscribeTo = ListenerBase.SubscriptionTarget.FirstParent;
        _check(parentBus);

        listener.SubscribeTo = ListenerBase.SubscriptionTarget.This;
        _check(thisBus);

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

    public class OrderListener : ListenerBase,
                                IListener<IntRef>

    {
        public int      CounterIn;

        // =======================================================================
        public void React(IntRef e)
        {
            CounterIn = e.Counter ++;
        }
    }
    
    [Test]
    public void Listener_Order()
    {
        var listener0 = _createListener<OrderListener>();
        listener0.Priority = -100;
        
        var listener1 = _createListener<OrderListener>();
        listener1.Priority = 0;
        
        var listener2 = _createListener<OrderListener>();
        listener2.Priority = 0;

        var listener3 = _createListener<OrderListener>();
        listener3.Priority = 100;

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

    public class EventListener : ListenerBase, 
                                IListener<IEvent<string>>

    {
        public string StringIn = string.Empty;
        
        public void React(IEvent<string> e)
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

    // =======================================================================
    private EventBus _createBus(Transform root = null)
    {
        var go = new GameObject("Bus");
        go.transform.SetParent(root, false);

        return go.AddComponent<EventBus>();
    }

    private TListener _createListener<TListener>(Transform root = null, int priority = 0) 
        where TListener : ListenerBase
    {
        var go = new GameObject("Listener");
        go.transform.SetParent(root, false);

        var result = go.AddComponent<TListener>();
        result.Priority = priority;

        return result;
    }
}
