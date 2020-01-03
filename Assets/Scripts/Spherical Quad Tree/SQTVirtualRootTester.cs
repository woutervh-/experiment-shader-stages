using UnityEngine;

public class SQTVirtualRootTester
{
    SQTVirtualRoot root;

    public SQTVirtualRootTester(SQTVirtualRoot root)
    {
        this.root = root;
    }

    public void Render()
    {
        foreach (SQTVirtualNode branch in root.branches)
        {
            RenderBranch(branch);
        }
    }

    void RenderBranch(SQTVirtualNode branch)
    {
        Debug.DrawLine(Vector3.zero, Vector3.one, Color.magenta, 2f);
    }
}
