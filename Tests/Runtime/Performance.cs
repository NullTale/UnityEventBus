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
        Logic._clear();

        var sub = Logic._createSubscriber<StringListener>(subscribeTo:Subscriber.SubscriptionTarget.Global);

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
        Logic._clear();

        var sub = Logic._createSubscriber<EventStringListener>(subscribeTo:Subscriber.SubscriptionTarget.Global);

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
        Logic._clear();

        var sub1 = Logic._createSubscriber<ActionHandle>(subscribeTo:Subscriber.SubscriptionTarget.Global);

        Measure.Method(() =>
               {
                   GlobalBus.SendAction<IActionHandle>(n => n.Do());
               })
               .IterationsPerMeasurement(10000)
               .MeasurementCount(10)
               .GC()
               .Run();
    }

    [Test, Performance]
    public void Send_Action_Complex()
    {
        Logic._clear();

        var sub1 = Logic._createSubscriber<ActionHandle>(priority:-1, subscribeTo:Subscriber.SubscriptionTarget.Global);
        var sub2 = Logic._createSubscriber<ActionHandle>(subscribeTo:Subscriber.SubscriptionTarget.Global);

        var bus1 = Logic._createBus(priority: -2, subscribeTo:EventBus.SubscriptionTarget.Global);
        var bus2 = Logic._createBus(subscribeTo:EventBus.SubscriptionTarget.Global);

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
        Logic._clear();

        var sub = Logic._createSubscriber<RequestListener>(subscribeTo:Subscriber.SubscriptionTarget.Global);

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
        Logic._clear();

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