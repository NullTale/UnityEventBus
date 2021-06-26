using UnityEngine;

namespace UnityEventBus
{
    public class Logger : EventBus
    {
        public override void Send<T>(in T e)
        {
            Debug.Log(e);
        }
    }
}