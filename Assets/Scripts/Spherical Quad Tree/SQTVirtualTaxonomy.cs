public interface SQTVirtualTaxonomy
{
    // SQTVirtualNode EnsureNeighbor(int direction);
    SQTVirtualNode EnsureChild(int childOrdinal);
    SQTVirtualNode EnsureChildNeighbor(int childOrdinal, int direction);
    void EnsureBalanced();
}
