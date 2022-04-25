using UnityEngine;

namespace UnityEventBus
{
    public class GlobalEventInvoker<T> : MonoBehaviour
        where T: GlobalEvent
    {
        [SerializeField]
        protected T m_GlobalEvent;

        public T GlobalEvent => m_GlobalEvent;
    }
}