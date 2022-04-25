namespace UnityEventBus
{
    public class GlobalEventInvokerTrigger : GlobalEventInvoker<GlobalEventTrigger>
    {
        // =======================================================================
        public void Invoke()
        {
            GlobalEvent.Invoke();
        }
    }
}