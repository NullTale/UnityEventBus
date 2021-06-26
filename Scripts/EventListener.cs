namespace UnityEventBus
{
    // helper generic
    public abstract class EventListener<A> : ListenerBase, IListener<IEvent<A>>
    {
        public abstract void React(IEvent<A> e);
    }

    public abstract class EventListener<A, B> : EventListener<A>, IListener<IEvent<B>>
    {
        public abstract void React(IEvent<B> e);
    }

    public abstract class EventListener<A, B, C> : EventListener<A, B>, IListener<IEvent<C>>
    {
        public abstract void React(IEvent<C> e);
    }
    
    public abstract class EventListener<A, B, C, D> : EventListener<A, B, C>, IListener<IEvent<D>>
    {
        public abstract void React(IEvent<D> e);
    }

}