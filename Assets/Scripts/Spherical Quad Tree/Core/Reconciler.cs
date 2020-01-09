namespace SQT.Core
{
    public interface Reconciler
    {
        void Reconcile(SQT.Core.SQTConstants[] constants, SQTBuilder.Node[] branches);
    }
}
