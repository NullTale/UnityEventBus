namespace UnityEventBus
{
    internal class DefaultListenerOptions : IListenerOptions
    {
        public string Name     => string.Empty;
        public int    Priority => 0;
    }
}