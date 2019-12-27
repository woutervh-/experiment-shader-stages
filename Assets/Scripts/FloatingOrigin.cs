using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FloatingOrigin : MonoBehaviour
{
    public float threshold = 1000f;

    void LateUpdate()
    {
        if (transform.position.magnitude >= threshold)
        {
            Object[] objects = FindObjectsOfType(typeof(Transform));
            foreach (Object o in objects)
            {
                Transform t = (Transform)o;
                if (t.parent == null)
                {
                    t.position -= transform.position;
                }
            }
        }
    }
}
