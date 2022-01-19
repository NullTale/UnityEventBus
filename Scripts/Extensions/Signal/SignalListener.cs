using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace UnityEventBus
{
    public class SignalListener : Subscriber, IListener<SignalAsset>
    {
        [SerializeField]
        private SignalAsset m_Signal;
        [SerializeField]
        private UnityEvent<SignalAsset> m_React;

        // =======================================================================
        public void React(in SignalAsset e)
        {
            if (m_Signal != e)
                return;

            m_React.Invoke(e);
        }
    }
}