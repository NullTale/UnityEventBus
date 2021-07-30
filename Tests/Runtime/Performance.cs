using System;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEventBus;

public class Performance
{
    public class StringListener : Subscriber,
                                     IListener<string>
    {
        int n;
        public void React(in string e)
        {
            n ++;
        }
    }

    [Test, Performance]
    public void Send_String()
    {
        var sub = Logic._createSubscriber<StringListener>();
        sub.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        Measure.Method(() =>
               {
                   GlobalBus.Send("srt");
               })
               .IterationsPerMeasurement(10000)
               .MeasurementCount(10)
               .GC()
               .Run();
    }
    
    public class EventStringListener : Subscriber,
                                     IListener<IEvent<string>>
    {
        int n;
        public void React(in IEvent<string> e)
        {
             n ++;
        }
    }

    [Test, Performance]
    public void Send_Event()
    {
        var sub = Logic._createSubscriber<EventStringListener>();
        sub.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        Measure.Method(() =>
               {
                   GlobalBus.SendEvent("srt");
               })
               .IterationsPerMeasurement(10000)
               .MeasurementCount(10)
               .GC()
               .Run();
    }

    public interface IActionHandle
    {
        void Do();
    }

    public class ActionHandle : Subscriber,
                                IHandle<IActionHandle>,
                                IActionHandle
    {
        int n;
        public void Do()
        {
            n ++;
        }
    }
    
    [Test, Performance]
    public void Send_Action()
    {
        var sub = Logic._createSubscriber<ActionHandle>();
        sub.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        Measure.Method(() =>
               {
                   GlobalBus.SendAction<IActionHandle>(n => n.Do());
               })
               .IterationsPerMeasurement(10000)
               .MeasurementCount(10)
               .GC()
               .Run();
    }
    
    public class RequestListener : Subscriber,
                                     IListener<IRequest<string>>
    {
        int n;
        public void React(in IRequest<string> e)
        {
             n ++;
        }
    }

    [Test, Performance]
    public void Send_Request()
    {
        var sub = Logic._createSubscriber<RequestListener>();
        sub.SubscribeTo = Subscriber.SubscriptionTarget.Global;

        Measure.Method(() =>
               {
                   GlobalBus.SendRequest("str");
               })
               .IterationsPerMeasurement(10000)
               .MeasurementCount(10)
               .GC()
               .Run();
    }

    [Test, Performance]
    public void Subscribe()
    {
        var listeners = Enumerable
                        .Repeat<Func<Subscriber>>(() => Logic._createSubscriber<EventStringListener>(), 20)
                        .Select(n => n.Invoke())
                        .ToArray();


        Measure.Method(() =>
               {
                   foreach (var listener in listeners)
                       GlobalBus.Subscribe(listener);
                   
                   foreach (var listener in listeners)
                       GlobalBus.UnSubscribe(listener);
               })
               .IterationsPerMeasurement(500)
               .MeasurementCount(10)
               .GC()
               .Run();
    }
}