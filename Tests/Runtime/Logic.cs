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

    public static TSubscriber _createSubscriber<TSubscriber>(Transform root = null, int priority = 0, Subscriber.SubscriptionTarget subscriptionTarget = Subscriber.SubscriptionTarget.None) 
        where TSubscriber : Subscriber
    {
        var go = new GameObject("Listener");
        go.transform.SetParent(root, false);

        var result = go.AddComponent<TSubscriber>();
        result.Priority = priority;
        result.SubscribeTo = subscriptionTarget;

        return result;
    }

    // =======================================================================
    public class SubscribeListener : Subscriber,
                                IListener<string>

    {
        public string StringIn = string.Empty;

        public void React(in string e)
        {
            StringIn = e;
        }

    }

    public class SubscribeManyListener : Subscriber,
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
        var listener     = _createSubscriber<SubscribeListener>(parentBus.transform);
        var listenerMany = _createSubscriber<SubscribeManyListener>(parentBus.transform);
        var thisBus      = listener.gameObject.AddComponent<EventBusBase>();

        listener.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        _check(GlobalBus.Instance);

        listener.SubscribeTo = Subscriber.SubscriptionTarget.FirstParent;
        _check(parentBus);

        listener.SubscribeTo = Subscriber.SubscriptionTarget.This;
        _check(thisBus);
        
        listener.SubscribeTo = Subscriber.SubscriptionTarget.None;
        listener.StringIn = string.Empty;
        GlobalBus.Send(k_ExpectedString);
        Assert.AreNotEqual(k_ExpectedString, listener.StringIn);


        listenerMany.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString);
        GlobalBus.Send(k_ExpectedInt);
        GlobalBus.Send(k_ExpectedFloat);
        GlobalBus.Send(k_ExpectedObject);
        
        Assert.AreEqual(k_ExpectedString, listenerMany.StringIn);
        Assert.AreEqual(k_ExpectedInt, listenerMany.IntIn);
        Assert.AreEqual(k_ExpectedFloat, listenerMany.FloatIn);
        Assert.AreEqual(k_ExpectedObject, listenerMany.ObjectIn);
        
        listenerMany.SubscribeTo = Subscriber.SubscriptionTarget.None;
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

    public class PriorityListener : Subscriber,
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
        var listener0 = _createSubscriber<PriorityListener>(null, -100);
        var listener1 = _createSubscriber<PriorityListener>(null, 0);
        var listener2 = _createSubscriber<PriorityListener>(null, 0);
        var listener3 = _createSubscriber<PriorityListener>(null, 100);

        listener1.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listener2.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listener0.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listener3.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener0.CounterIn);
        Assert.AreEqual(1, listener1.CounterIn);
        Assert.AreEqual(2, listener2.CounterIn);
        Assert.AreEqual(3, listener3.CounterIn);

        // ===================================
        // priority resubscribe
        listener0.Priority = 100;

        // resubscribe in same order
        listener1.SubscribeTo = Subscriber.SubscriptionTarget.None;
        listener1.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        
        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener2.CounterIn);
        Assert.AreEqual(1, listener1.CounterIn);
        Assert.AreEqual(2, listener3.CounterIn);
        Assert.AreEqual(3, listener0.CounterIn);
    }

    
    public class GenericListener : Subscriber,
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
        var listenerA = _createSubscriber<GenericListener>();
        var listenerB = _createSubscriber<GenericListener>();
        
        listenerA.OnReact = gl => listenerB.SubscribeTo = Subscriber.SubscriptionTarget.None;

        listenerA.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listenerB.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString);

        Assert.AreEqual(k_ExpectedString, listenerA.StringIn);
        Assert.AreEqual(string.Empty, listenerB.StringIn);
    }

    
    [Test]
    public void Bus_RuntimeDeactivation()
    {
        var busA = _createBus();
        var busB = _createBus();

        var listenerA = _createSubscriber<GenericListener>(busA.transform);
        var listenerB = _createSubscriber<GenericListener>(busB.transform);

        listenerA.OnReact = gl => busB.SubscribeTo = EventBus.SubscriptionTarget.None;
        listenerA.SubscribeTo = Subscriber.SubscriptionTarget.FirstParent;
        listenerB.SubscribeTo = Subscriber.SubscriptionTarget.FirstParent;

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

        var listener0 = _createSubscriber<PriorityListener>(bus0.transform, 0, Subscriber.SubscriptionTarget.FirstParent);
        var listener1 = _createSubscriber<PriorityListener>(bus1.transform, 0, Subscriber.SubscriptionTarget.FirstParent);
        var listener2 = _createSubscriber<PriorityListener>(bus2.transform, 0, Subscriber.SubscriptionTarget.FirstParent);
        var listener3 = _createSubscriber<PriorityListener>(bus3.transform, 0, Subscriber.SubscriptionTarget.FirstParent);

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

    
    public class EventListener : Subscriber, 
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
    public void SendEvent()
    {
        var listener = _createSubscriber<EventListener>();
        listener.SubscribeTo = Subscriber.SubscriptionTarget.Global;

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

    public interface IActionHandle
    {
        void Inc();
    }

    public class ActionHandleSubscriber : Subscriber, 
                              IActionHandle,
                              IHandle<IActionHandle>

    {
        public int  InnerInt;

        public void Inc()
        {
            InnerInt++;
        }
    }

    [Test]
    public void SendAction()
    {
        var sub = _createSubscriber<ActionHandleSubscriber>();
        sub.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        sub.InnerInt = 0;
        GlobalBus.SendAction<IActionHandle>(ah => ah.Inc());
        Assert.AreEqual(1, sub.InnerInt);
    }

    public class RequestListener : Subscriber, 
                                   IListener<IRequest<string>>

    {
        public string  StringIn;

        public void React(in IRequest<string> e)
        {
            StringIn = e.Key;
            e.Approve();
        }
    }

    [Test]
    public void SendRequest()
    {
        var sub1 = _createSubscriber<RequestListener>();
        sub1.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        
        var sub2 = _createSubscriber<RequestListener>();
        sub2.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        sub1.StringIn = string.Empty;
        sub2.StringIn = string.Empty;
        var requestResult = GlobalBus.SendRequest(k_ExpectedString);

        Assert.AreEqual(true, requestResult);
        Assert.AreEqual(k_ExpectedString, sub1.StringIn);
        Assert.AreEqual(string.Empty, sub2.StringIn);
    }
}