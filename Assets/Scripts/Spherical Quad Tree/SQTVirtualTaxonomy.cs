public interface SQTVirtualTaxonomy
{
    void EnsureNeighbor(int direction);
    SQTVirtualNode EnsureChild(int childOrdinal);
    SQTVirtualNode EnsureChildNeighbor(int childOrdinal, int direction);
    void EnsureBalanced();
}
