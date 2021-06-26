
## Description
Code is designed to be easy to use. And optimized where it's possible.

An EventBus is a mechanism that allows different components to communicate with each other without knowing about each other. A component can send an Event to the EventBus without knowing who will pick it up or how many others will pick it up.


## Installation
Through Unity Package Manager git URL: `https://github.com/NullTale/UnityEventBus.git`

![PackageManager](https://user-images.githubusercontent.com/1497430/123476308-32c6c500-d605-11eb-8eca-9266624ad58b.gif)

Or copy-paste somewhere inside your project Assets folder.

## Event Listener

To listen messages from the bus you need to execute at least one `IListener<>` interface and subscribe to a desired bus.

```c#
using UnityEngine;
using UnityEventBus;

public class SimpleListener : MonoBehaviour, IListener<string>
{
    private void Start()
    {
        // somewhere in code...
        // send string event to the global bus
        EventSystem.Send<string>("String event");
    }

    // subscribe to the global bus
    private void OnEnable()
    {
        EventSystem.Subscribe(this);
    }

    // unsubscribe from the global bus
    private void OnDisable()
    {
        EventSystem.UnSubscribe(this);
    }

    // react on event
    public void React(string e)
    {
        Debug.Log(e);
    }
}
```

You can also implement `IListenerOptions` interface to setup debug name and order priority.

```c#
public class SimpleListener : MonoBehaviour, IListener<string>, IListenerOptions
{
    public string Name     => name;
    // lower priority triggers first
    public int    Priority => 1;
```

The same can be done with `Listener<>` or `ListenerBase` class.

```c#
using UnityEngine;
using UnityEventBus;

// same as SimpleListener : ListenerBase, IListener<string>
public class SimpleListener : Listener<string>
{
    public override void React(string e)
    {
        Debug.Log(e);
    }
}
```

In the unit editor, you can setup the behavior in detail.

![Listener](https://user-images.githubusercontent.com/1497430/123495864-1c812f00-d62e-11eb-81a9-0144b56529dd.png)


## Local Event Bus
To create a local bus, you need to inherit a class from the `EventBusBase`.

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

    public override void Send<T>(in T e)
    {
        // log each income event
        Debug.Log(e);

        base.Send(in e);
    }
}
```

Event bus can subscribe to the other buses like a listener

```c#
public class Unit : EventBusBase
{
    // subscribe to the global bus for example
    private void OnEnable()
    {
        EventSystem.Subscribe(this);
    }

    private void OnDisable()
    {
        EventSystem.UnSubscribe(this);
    }
}
```

You can also inherit from the `EventBus` class to configure auto-subscription.

```c#
public class Unit : EventBus
```

![Bus](https://user-images.githubusercontent.com/1497430/123495869-2145e300-d62e-11eb-8594-094b221f2bb8.png)

## Using IEvent messages
There is the helper message type `IEvent`, which contains the key and optional data. To send IEvent message you need to use `SendEvent` function.

```c#
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

// unit receive events and propagate them to subscribers
public class Unit : EventBusBase
{
    private void Start()
    {
        // send creation event
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
```

```c#
using UnityEngine;
using UnityEventBus;

// same as UnitHP : ListenerBase, IListener<IEvent<UnitEvent>>
public class UnitHP : EventListener<UnitEvent>
{
    public int HP = 2;

    // reacts on UnitEvents
    public override void React(IEvent<UnitEvent> e)
    {
        switch (e.Key)
        {
            case UnitEvent.Created:
                break;
            case UnitEvent.Damage:
                // get int data from the event
                HP -= e.GetData<int>();
                break;
            case UnitEvent.Heal:
                HP += e.GetData<int>();
                break;
        }

        if (HP <= 0)
            Debug.Log("Unit is dead");
    }
}
```

![UnitUsage](https://user-images.githubusercontent.com/1497430/123495873-260a9700-d62e-11eb-9e80-f729b71c480b.gif)

### Multiple data via IEvent

```c#
// send multiple data 
SendEvent(UnitEvent.Created, 1, 0.2f, this);
```

```c#
// get multiple data 
var (n, f, unit) = e.GetData<int, float, Unit>();
```
or
```c#
if (e.TryGetData(out int n, out float f, out Unit unit))
    // do something
```

## Send an Event
To send events the generic `Send<>(e)` and `SendEvent<>(e)` functions is used. The generic argument defines which listeners will receive messages. 
For example `Send<object>(obj)` will only trigger listeners that react to object type messages.
```c#

Send<object>("text");       // will trigger React(object e)

SendEvent<object>("text");  // will trigger React(IEvent<object> e)
```

So you can, for example, create your own interface and use it as an event.

```c#
public interface IActivatable
{
}

public class Trigger : MonoBehaviour, IActivatable
{
    public void Activate()
    {
        // call IActivatable version of send to trigger IActivatable listeners
        EventSystem.Send<IActivatable>(this);
    }
}

public class ActivatableListener : Listener<IActivatable>
{
    public override void React(IActivatable e)
    {
        // do something with e
    }
}
```
