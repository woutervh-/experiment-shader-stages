using System.Collections.Generic;
using UnityEngine;

namespace SQT.Core
{
    public partial class Branches
    {
#if UNITY_EDITOR
        void DrawQuad(Vector3 up, Vector3 forward, Vector3 right, Vector2 offset, float scale)
        {
            Vector3 origin = up + offset.x * forward + offset.y * right;
            Debug.DrawLine(origin - forward * scale - right * scale, origin - forward * scale + right * scale, Color.magenta);
            Debug.DrawLine(origin - forward * scale + right * scale, origin + forward * scale + right * scale, Color.magenta);
            Debug.DrawLine(origin + forward * scale + right * scale, origin + forward * scale - right * scale, Color.magenta);
            Debug.DrawLine(origin + forward * scale - right * scale, origin - forward * scale - right * scale, Color.magenta);
        }

        void DrawBranch(Builder.Node root)
        {
            Vector3 up = directions[root.branch];
            Vector3 forward = new Vector3(up.y, up.z, up.x);
            Vector3 right = Vector3.Cross(up, forward);

            Stack<Builder.Node> nodes = new Stack<Builder.Node>();
            nodes.Push(root);
            while (nodes.Count >= 1)
            {
                Builder.Node node = nodes.Pop();
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

        public void DrawBranches(Camera camera)
        {
            ReconciliationData reconciliationData = ReconciliationData.GetData(global, constants, camera);
            Builder.Node[] branches = Builder.BuildBranches(reconciliationData);
            foreach (Builder.Node branch in branches)
            {
                DrawBranch(branch);
            }
        }

        string StringifyNode(Builder.Node node)
        {
            if (node.children == null)
            {
                // return "[" + string.Join(", ", node.path) + "]";
                return "[" + node.neighborMask + "]";
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
                return "[" + node.neighborMask + ", " + string.Join(", ", strings) + "]";
            }
        }
#endif
    }
}
