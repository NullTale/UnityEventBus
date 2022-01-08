using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEventBus;
using Object = UnityEngine.Object;

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

    public static void _clear()
    {
        foreach (var gameObject in Object.FindObjectsOfType<GameObject>().Where(n => n != GlobalBus.Instance.gameObject))
            Object.Destroy(gameObject);
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
        _clear();

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
        _clear();

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
        _clear();

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
        _clear();

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
        _clear();

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
    
    [Test]
    public void Bus_Listeners_Priority()
    {
        _clear();

        var bus0 = _createBus(null, -100);
        var bus1 = _createBus(null, 0);
        var bus2 = _createBus(null, 0);
        var bus3 = _createBus(null, 100);

        bus1.SubscribeTo = EventBus.SubscriptionTarget.Global;

        var listener0 = _createSubscriber<PriorityListener>(null, -150, Subscriber.SubscriptionTarget.Global);
        var listener1 = _createSubscriber<PriorityListener>(null, 0, Subscriber.SubscriptionTarget.Global);
        var listener2 = _createSubscriber<PriorityListener>(null, 0, Subscriber.SubscriptionTarget.Global);
        var listener3 = _createSubscriber<PriorityListener>(null, 50, Subscriber.SubscriptionTarget.Global);

        var busListener0 = _createSubscriber<PriorityListener>(bus0.transform, 0, Subscriber.SubscriptionTarget.FirstParent);
        var busListener1 = _createSubscriber<PriorityListener>(bus1.transform, 0, Subscriber.SubscriptionTarget.FirstParent);
        var busListener2 = _createSubscriber<PriorityListener>(bus2.transform, 0, Subscriber.SubscriptionTarget.FirstParent);
        var busListener3 = _createSubscriber<PriorityListener>(bus3.transform, 0, Subscriber.SubscriptionTarget.FirstParent);

        bus3.SubscribeTo = EventBus.SubscriptionTarget.Global;
        bus2.SubscribeTo = EventBus.SubscriptionTarget.Global;
        bus0.SubscribeTo = EventBus.SubscriptionTarget.Global;

        GlobalBus.Send(new IntRef() { Counter = 0 });

        Assert.AreEqual(0, listener0.CounterIn);
        Assert.AreEqual(1, busListener0.CounterIn);
        Assert.AreEqual(2, busListener1.CounterIn);
        Assert.AreEqual(3, listener1.CounterIn);
        Assert.AreEqual(4, listener2.CounterIn);
        Assert.AreEqual(5, busListener2.CounterIn);
        Assert.AreEqual(6, listener3.CounterIn);
        Assert.AreEqual(7, busListener3.CounterIn);

        GlobalBus.Send(k_ExpectedString);
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
        _clear();

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
        public int  InnerInt = 0;

        public void Inc()
        {
            InnerInt ++;
        }
    }

    [Test]
    public void SendAction()
    {
        _clear();

        var sub = _createSubscriber<ActionHandleSubscriber>();
        sub.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        GlobalBus.SendAction<IActionHandle>(ah => ah.Inc());
        Assert.AreEqual(sub.InnerInt, 1);
    }

    public class RequestListener : Subscriber, 
                                   IListener<IRequest<string>>

    {
        public string  StringIn = string.Empty;

        public void React(in IRequest<string> e)
        {
            StringIn = e.Key;
            e.Approve();
        }
    }

    [Test]
    public void SendRequest()
    {
        _clear();

        var sub1 = _createSubscriber<RequestListener>();
        var sub2 = _createSubscriber<RequestListener>();

        sub1.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        sub2.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        var requestResult = GlobalBus.SendRequest(k_ExpectedString);

        Assert.AreEqual(true, requestResult);
        Assert.AreEqual(k_ExpectedString, sub1.StringIn);
        Assert.AreEqual(string.Empty, sub2.StringIn);
    }

    [Test]
    public void Filter()
    {
        var listenerA     = _createSubscriber<SubscribeListener>();
        var listenerB     = _createSubscriber<SubscribeListener>();
        var listenerC     = _createSubscriber<SubscribeListener>();

        listenerA.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listenerB.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listenerC.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString, s => ReferenceEquals(s, listenerA));

        Assert.AreEqual(listenerA.StringIn, k_ExpectedString);
        Assert.AreEqual(listenerB.StringIn, string.Empty);
        Assert.AreEqual(listenerC.StringIn, string.Empty);
    }

    [Test]
    public void Invokation()
    {
        _clear();

        var listenerA     = _createSubscriber<SubscribeListener>();
        var listenerB     = _createSubscriber<ActionHandleSubscriber>();
        var listenerC     = _createSubscriber<RequestListener>();

        listenerA.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listenerB.SubscribeTo = Subscriber.SubscriptionTarget.Global;
        listenerC.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        GlobalBus.Send(k_ExpectedString);
        GlobalBus.SendAction<IActionHandle>(n => n.Inc());
        GlobalBus.SendRequest(k_ExpectedString);

        Assert.AreEqual(listenerA.StringIn, k_ExpectedString);
        Assert.AreEqual(listenerB.InnerInt, 1);
        Assert.AreEqual(listenerC.StringIn, k_ExpectedString);
    }
}