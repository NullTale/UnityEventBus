using System;

namespace UnityEventBus
{
    public class GlobalEventTrigger : GlobalEvent
    {
        public event Action OnInvoke;

        // =======================================================================
        public void Invoke()
        {
            OnInvoke?.Invoke();
        }

        public override void Init()
        {
            OnInvoke = null;
        }
    }
}