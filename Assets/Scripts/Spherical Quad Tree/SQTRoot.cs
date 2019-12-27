public class SQTRoot : SQTTaxomy
{
    const int resolution = -1;

    SQTConstants constants;
    SQTNode[] children;

    public SQTRoot(SQTConstants constants)
    {
        this.constants = constants;
    }

    public void Destroy()
    {
        if (children != null)
        {
            foreach (SQTNode child in children)
            {
                child.Destroy();
            }
        }
    }

    public int GetResolution()
    {
        return resolution;
    }
}
