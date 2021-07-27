namespace UnityEventBus
{
    internal class DefaultSubscriberOptions : ISubscriberOptions
    {
        public string Name     => string.Empty;
        public int    Priority => 0;
    }
}