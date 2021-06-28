using UnityEngine;

namespace UnityEventBus.Utils
{
    public class Logger : EventBus
    {
        public override void Send<T>(in T e)
        {
            Debug.Log(e);
        }
    }
}