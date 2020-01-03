using UnityEngine;

public interface SQTTaxomy
{
    // void Reconciliate(SQTReconciliationData reconciliationData);
    SQTVirtualNode GetChild(int ordinal);
    SQTVirtualNode EnsureNeighbor(int direction);
}
