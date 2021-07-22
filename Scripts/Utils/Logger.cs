using UnityEngine;

namespace UnityEventBus.Utils
{
    public class Logger : EventBus
    {
        public override void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
        {
            Debug.Log(e);
        }
    }
}