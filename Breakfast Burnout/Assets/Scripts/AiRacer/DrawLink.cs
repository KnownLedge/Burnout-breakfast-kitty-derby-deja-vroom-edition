using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawLink : MonoBehaviour
{
    public static float reachRadius = 4f;
    void OnDrawGizmos()
    {
        if (gameObject.name == "1")
        {
            Gizmos.color = new Color(1.0f, 1f, 1f, 0.75f);
        }
        else
        {
            Gizmos.color = new Color(0.0f, 0.75f, 0.0f, 0.75f);
        }

        int targetIndex;

        if (gameObject.name.Count() < 3) //I hope you don't go past 100 checkpoints
        {
            targetIndex = int.Parse(gameObject.name);

            targetIndex++;
            Transform target = transform.parent.Find(targetIndex.ToString());
            if (target != null)
            {
                Gizmos.DrawLine(transform.position, target.position);
            }
            else
            {
                target = transform.parent.Find("1");
                if (target != null)
                {
                    Gizmos.color = new Color(0.0f, 0.0f, 0.75f, 0.75f);
                    Gizmos.DrawLine(transform.position, target.position);

                }
            }

            Gizmos.DrawSphere(transform.position, reachRadius);

        }

    }

}
