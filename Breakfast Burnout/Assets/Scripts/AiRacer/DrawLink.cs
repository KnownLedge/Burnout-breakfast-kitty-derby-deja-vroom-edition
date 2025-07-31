using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLink : MonoBehaviour
{

    void OnDrawGizmo()
    {
        Gizmos.color = new Color(0.75f, 0.0f, 0.0f, 0.75f);

        int targetIndex;
        int.TryParse(gameObject.name, out targetIndex);
        targetIndex++;
        Transform target = transform.parent.Find(targetIndex.ToString());
        if (target != null)
        {
            Gizmos.DrawLine(transform.position, target.position);
        }
    }



}
