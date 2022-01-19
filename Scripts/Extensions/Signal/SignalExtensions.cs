using UnityEngine.Timeline;

namespace UnityEventBus
{
    public static class SignalExtensions
    {
        public static void Rise(this SignalAsset signal)
        {
            GlobalBus.Send(in signal);
        }
    }
}