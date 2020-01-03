using System.Collections.Generic;
using UnityEngine;

public class SQTVirtualRootTester
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    SQTVirtualRoot root;

    public SQTVirtualRootTester(SQTVirtualRoot root)
    {
        this.root = root;
    }

    public void Render()
    {
        for (int i = 0; i < directions.Length; i++)
        {
            RenderBranch(i);
        }
    }

    void RenderQuad(Vector3 up, Vector3 forward, Vector3 right, Vector2 offset, float scale)
    {
        Vector3 origin = up + offset.x * forward + offset.y * right;
        Debug.DrawLine(origin - forward * scale - right * scale, origin - forward * scale + right * scale, Color.magenta, 1f / 30f);
        Debug.DrawLine(origin - forward * scale + right * scale, origin + forward * scale + right * scale, Color.magenta, 1f / 30f);
        Debug.DrawLine(origin + forward * scale + right * scale, origin + forward * scale - right * scale, Color.magenta, 1f / 30f);
        Debug.DrawLine(origin + forward * scale - right * scale, origin - forward * scale - right * scale, Color.magenta, 1f / 30f);
    }

    void RenderBranch(int index)
    {
        Vector3 up = directions[index];
        Vector3 forward = new Vector3(up.y, up.z, up.x);
        Vector3 right = Vector3.Cross(up, forward);

        Stack<SQTVirtualNode> nodes = new Stack<SQTVirtualNode>();
        nodes.Push(root.branches[index]);
        while (nodes.Count != 0)
        {
            SQTVirtualNode node = nodes.Pop();
            RenderQuad(up, forward, right, node.offset, 1f / Mathf.Pow(2f, node.GetDepth()));
            if (node.children != null)
            {
                nodes.Push(node.children[0]);
                nodes.Push(node.children[1]);
                nodes.Push(node.children[2]);
                nodes.Push(node.children[3]);
            }
        }
    }
}
