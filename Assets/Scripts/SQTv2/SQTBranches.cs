using System.Collections.Generic;
using UnityEngine;

public class SQTBranches
{
    static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    SQTConstants[] constants;

    public SQTBranches(SQTConstants.SQTGlobal global, SQTConstants.SQTDepth[] depth)
    {
        constants = new SQTConstants[directions.Length];
        int[] branchRootPath = new int[0];
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject branchGameObject = new GameObject("SQT (" + i + ")");
            branchGameObject.transform.SetParent(global.gameObject.transform, false);
            SQTConstants.SQTBranch branch = new SQTConstants.SQTBranch(i, directions[i])
            {
                gameObject = branchGameObject
            };
            constants[i] = new SQTConstants
            {
                global = global,
                branch = branch,
                depth = depth
            };
        }
    }

    public void Reconciliate(Camera camera)
    {
        SQTReconciliationData reconciliationData = GetReconciliationData(camera);
        SQTBuilder builder = new SQTBuilder();
        builder.CalculatePaths(reconciliationData);
        foreach (SQTBuilder.Node branch in builder.branches)
        {
            DrawBranch(branch);
        }
        // Debug.Log(StringifyNode(builder.branches[0]));
    }

    void DrawQuad(Vector3 up, Vector3 forward, Vector3 right, Vector2 offset, float scale)
    {
        Vector3 origin = up + offset.x * forward + offset.y * right;
        Debug.DrawLine(origin - forward * scale - right * scale, origin - forward * scale + right * scale, Color.magenta);
        Debug.DrawLine(origin - forward * scale + right * scale, origin + forward * scale + right * scale, Color.magenta);
        Debug.DrawLine(origin + forward * scale + right * scale, origin + forward * scale - right * scale, Color.magenta);
        Debug.DrawLine(origin + forward * scale - right * scale, origin - forward * scale - right * scale, Color.magenta);
    }

    void DrawBranch(SQTBuilder.Node root)
    {
        Vector3 up = directions[root.branch];
        Vector3 forward = new Vector3(up.y, up.z, up.x);
        Vector3 right = Vector3.Cross(up, forward);

        Stack<SQTBuilder.Node> nodes = new Stack<SQTBuilder.Node>();
        nodes.Push(root);
        while (nodes.Count >= 1)
        {
            SQTBuilder.Node node = nodes.Pop();
            DrawQuad(up, forward, right, node.offset, 1f / Mathf.Pow(2f, node.path.Length));
            if (node.children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (node.children[i] != null)
                    {
                        nodes.Push(node.children[i]);
                    }
                }
            }
        }
    }

    string StringifyNode(SQTBuilder.Node node)
    {
        if (node.children == null)
        {
            return "[" + string.Join(", ", node.path) + "]";
        }
        else
        {
            string[] strings = new string[node.children.Length];
            for (int i = 0; i < node.children.Length; i++)
            {
                if (node.children[i] == null)
                {
                    strings[i] = "null";
                }
                else
                {
                    strings[i] = StringifyNode(node.children[i]);
                }
            }
            return "[" + string.Join(", ", strings) + "]";
        }
    }

    SQTReconciliationData GetReconciliationData(Camera camera)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            SQTReconciliationData reconciliationData = SQTReconciliationData.GetData(constants[i], camera);
            if (reconciliationData != null)
            {
                return reconciliationData;
            }
        }
        return null;
    }
}
