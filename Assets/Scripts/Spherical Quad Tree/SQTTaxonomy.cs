using UnityEngine;

public interface SQTTaxomy
{
    SQTNode FindNode(Camera camera);
    void Reconciliate(SQTReconciliationSettings reconciliationSettings);
}
