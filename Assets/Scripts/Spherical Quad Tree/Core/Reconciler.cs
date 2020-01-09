namespace SQT.Core
{
    public interface Reconciler
    {
        void Reconcile(Builder.Node[] branches);
        void Destroy();
    }

    public interface ReconcilerFactory
    {
        Reconciler FromConstants(SQT.Core.Constants[] constants);
    }
}
