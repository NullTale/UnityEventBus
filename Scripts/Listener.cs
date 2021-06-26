namespace UnityEventBus
{
    // helper generic
    public abstract class Listener<A> : ListenerBase, IListener<A>
    {
        public abstract void React(A e);
    }

    public abstract class Listener<A, B> : Listener<A>, IListener<B>
    {
        public abstract void React(B e);
    }
    
    public abstract class Listener<A, B, C> :  Listener<A, B>, IListener<C>
    {
        public abstract void React(C e);
    }
    
    public abstract class Listener<A, B, C, D> : Listener<A, B, C>, IListener<D>
    {
        public abstract void React(D e);
    }
}