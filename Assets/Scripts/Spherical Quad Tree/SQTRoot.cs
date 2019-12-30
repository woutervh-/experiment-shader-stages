using UnityEngine;

public class SQTRoot : SQTTaxomy
{
    SQTConstants constants;
    SQTNode child;

    public SQTRoot(SQTConstants constants)
    {
        this.constants = constants;
        child = new SQTNode(this, constants, Vector2.zero, 0);
    }

    public SQTNode FindNode(GameObject player)
    {
        return child.FindNode(player);
    }

    public void Destroy()
    {
        child.Destroy();
    }
}
