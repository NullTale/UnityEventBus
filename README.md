
### Description
Designed to be easy to use, extendable and optimized where it's possible.

An EventBus is a mechanism that allows different components to communicate with each other without knowing about each other. A component can send an Event to the EventBus without knowing who will pick it up or how many others will pick it up.


### Installation
Through Unity Package Manager git URL: `https://github.com/NullTale/UnityEventBus.git`

![PackageManager](https://user-images.githubusercontent.com/1497430/123476308-32c6c500-d605-11eb-8eca-9266624ad58b.gif)

Or copy-paste somewhere inside your project Assets folder.


## Table of Content

* [Event Listener](#event-listener)
* [Event Bus](#event-bus)
* [Extentions](#extentions)
	+ [Event](#event)
	+ [Request](#request)
	+ [Action](#action)

## Event Listener

On OnEnable event Listener will connect to the desired bus and will start receiving messages from it.

```c#
using UnityEngine;
using UnityEventBus;

// same as SimpleListener : ListenerBase, IListener<string>
public class SimpleListener : Listener<string>
{
    public override void React(in string e)
    {
        Debug.Log(e);
    }
}
```

In the unity editor, you can set up the behavior in detail. Such as subscription targets and priority.

![Listener](https://user-images.githubusercontent.com/1497430/123495864-1c812f00-d62e-11eb-81a9-0144b56529dd.png)

To create your custom listener you need to implement at least one `IListener<>` interface and subscribe to the desired bus.

>Note: Listener can have any number of IListener<> interfaces.

```c#
using UnityEngine;
using UnityEventBus;

public class SimpleListener : MonoBehaviour, IListener<string>
{
    private void Start()
    {
        // somewhere in code...
        // send string event to the global bus
        GlobalBus.Send<string>("String event");
    }

    // subscribe to the global bus
    private void OnEnable()
    {
        GlobalBus.Subscribe(this);
    }

    // unsubscribe from the global bus
    private void OnDisable()
    {
        GlobalBus.UnSubscribe(this);
    }

    // react on event
    public void React(in string e)
    {
        Debug.Log(e);
    }
}
```

You can also implement `ISubscriberOptions` interface to setup debug name and listener priority.

```c#
public class SimpleListener : MonoBehaviour, IListener<string>, ISubscriberOptions
{
    public string Name     => name;
    // lower priority triggers first, highest last, equal invokes in order of subscription
    public int    Priority => 1;
```

## Event Bus
To create a local bus, you need to inherit from the `EventBusBase` class.

> Note: Event bus can subscribe to the other buses like a listener, using Subscribe and UnSubscribe methods. Busses always triggers last.

```c#
using UnityEngine;
using UnityEventBus;

public class Unit : EventBusBase
{
    private void Start()
    {
        // send event to this
        this.Send("String event");
    }
}
```


You can also inherit from the `EventBus` class to configure auto-subscription and priority.
> Note: If  EventBus implements any of the Subscribers interfaces, it automatically subscribes them to itself.

```c#
public class Unit : EventBus
```

![Bus](https://user-images.githubusercontent.com/1497430/123495869-2145e300-d62e-11eb-8594-094b221f2bb8.png)

## Extentions
### Event
Event is a message that contains keys and optional data. To send an Event, the `SendEvent` extension method is used. To receive events must be implemented `IListener<IEvent<TEventKey>>` interface, where TEventKey is a key of events, which listener wants to react.

```c#
using UnityEngine;
using UnityEventBus;

// unit is EventBus this receives events and propagate them to subscribers
public class Unit : EventBusBase
{
    private void Start()
    {
        // send creation event without data
        this.SendEvent(UnitEvent.Created);
    }

    [ContextMenu("Damage")]
    public void DamageSelf()
    {
        // send event with int data
        this.SendEvent(UnitEvent.Damage, 1);
    }

    [ContextMenu("Heal")]
    public void HealSelf()
    {
        this.SendEvent(UnitEvent.Heal, 1);
    }
}

// unit event keys
public enum UnitEvent
{
    Created,
    Damage,
    Heal
}
```

```c#
using UnityEngine;
using UnityEventBus;

// OnEnable will subscribe to the unit and start to listen messages
// same as UnitHP : EventListener<UnitEvent>
public class UnitHP : ListenerBase,
                      IListener<IEvent<UnitEvent>>
{
    public int HP = 2;

    // reacts on UnitEvents
    public override void React(in IEvent<UnitEvent> e)
    {
        switch (e.Key)
        {
            // event with damage or heal key always containts int data
            case UnitEvent.Damage:
                HP -= e.GetData<int>();
                break;
            case UnitEvent.Heal:
                HP += e.GetData<int>();
                break;

            case UnitEvent.Created:
                break;
        }
    }
}
```

![UnitUsage](https://user-images.githubusercontent.com/1497430/123495873-260a9700-d62e-11eb-9e80-f729b71c480b.gif)

Also multiple data can be sent through an event.

```c#
// send multiple data 
SendEvent(UnitEvent.Created, 1, 0.2f, this);
```

```c#
// get multiple data 
var (n, f, unit) = e.GetData<int, float, Unit>();

// or
if (e.TryGetData(out int n, out float f, out Unit unit))
{
	// do something with data
}
```

### Request
Request is needed to get permission for something from another subscriber. Request works just like Event, contains key and optional data, but it can be either approved or ignored and is propagated until first approval. It is in fact a usable event with the only difference that you can get the result of its execution. The `SendRequest` method extension is used to send a Request.
```c#
public class Unit : EventBus
{
    [ContextMenu("HealRequest ")]
    public void HealRequest()
    {
        if (this.SendRequest(UnitEvent.Heal, 1))
        {
            // request was approved
            // do something, play animation or implement logic
        }
    }
}
```
```c#
public class UnitHP : ListenerBase,
                      IListener<IRequest<UnitEvent>>
{
    public int HPMax = 2;
    public int HP = 2;

    public void React(in IRequest<UnitEvent> e)
    {
        switch (e.Key)
        {
            case UnitEvent.Heal:
            {
                // if can heal, approve heal request
                var heal = e.GetData<int>();
                if (heal > 0 && HP < HPMax)
                {
                    HP += heal;
                    e.Approve();
                }
            } break;
        }
    }
}
```
### Action
Sometimes it can be more convenient to look at listeners as a set of interfaces, the `SendAction` method extension is used for this. For an object to be an action target it must execute an interface` IHandle<>` with the interface type it provides.
```c#
public class Unit : EventBus
{
    [ContextMenu("Heal Action")]
    public void Heal()
    {
        // send invoke heal action on the IUnitHP interface
        this.SendAction<IUnitHP>(hp => hp.Heal(1));
    }
}
```
```c#
public interface IUnitHP
{
    void Heal(int val);
}

// implement IHandle<IUnitHP> interface to be an action target
public class UnitHP : ListenerBase, 
                      IUnitHP,
                      IHandle<IUnitHP>
{
    public int HP = 2;

    public void Heal(int val)
    {
        HP += val;
    }
}
```
