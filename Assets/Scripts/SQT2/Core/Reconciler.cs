namespace SQT2.Core
{
    public static class Reconciler
    {
        public static void Initialize(Context context)
        {
            // TODO: load meshes without depth.
        }

        public static void Reconcile(Context context, ReconciliationData reconciliationData)
        {
            Core.Node root = context.roots[reconciliationData.branch.index];
        }
    }
}
