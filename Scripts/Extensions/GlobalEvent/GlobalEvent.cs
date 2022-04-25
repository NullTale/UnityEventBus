using UnityEngine;

namespace UnityEventBus
{
    public abstract class GlobalEvent : ScriptableObject
    {
        public virtual void Init()
        {
        }
    }
}