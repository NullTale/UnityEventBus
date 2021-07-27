using System;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEventBus;

public class Performance
{
    public class StringListener : ListenerBase,
                                     IListener<string>
    {
        public void React(in string e)
        {
        }
    }

    [Test, Performance]
    public void Send_String()
    {
        var listener = Logic._createListener<StringListener>();
        listener.SubscribeTo = ListenerBase.SubscriptionTarget.Global;

        Measure.Method(() =>
               {
                   GlobalBus.Send("srt");
               })
               .IterationsPerMeasurement(10000)
               .MeasurementCount(10)
               .GC()
               .Run();
    }
    
    public class EventStringListener : ListenerBase,
                                     IListener<IEvent<string>>
    {
        public void React(in IEvent<string> e)
        {
        }
    }

    [Test, Performance]
    public void Send_Event()
    {
        var listener = Logic._createListener<EventStringListener>();
        listener.SubscribeTo = ListenerBase.SubscriptionTarget.Global;

        Measure.Method(() =>
               {
                   GlobalBus.SendEvent("srt");
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
                        .Repeat<Func<ListenerBase>>(() => Logic._createListener<EventStringListener>(), 20)
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