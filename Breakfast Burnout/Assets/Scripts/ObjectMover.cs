using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    //Script that is mostly hardcoded for a silly little animation on lobby screen
    //I'd suggest just making a new script that copies from this if it seems useful.
    public Vector3 origin = Vector3.zero;
    public Vector3 resetPoint = Vector3.zero;

    public float speed = 1f;



    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.right * speed * Time.deltaTime);
        if (transform.position.x > resetPoint.x)
        {
            transform.position = origin;
        }
    }
}
