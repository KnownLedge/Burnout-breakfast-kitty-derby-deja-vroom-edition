using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Collectable : MonoBehaviour
{
    //DISCLAIMER: this code sucks, way too much stuff for a single variable, not my best work

    private float rotation = 0f;
    public float minHeight = -0.9f;
    public float maxHeight = 1.5f;
    public float heightLerp = 0f;
    private Vector3 initPosition;
    public float hoverSpeed = 1f;

    public float rotateSpeed = 1f;

    private float direction = 1f;

    private bool collected = false;

    public static int totalLoops = 0;

    public Transform model;

    public Material[] loopMaterials;

    void Start()
    {
        initPosition = transform.position;

        model.GetComponent<MeshRenderer>().material = loopMaterials[Random.Range(0, loopMaterials.Length)];
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 newPos = initPosition;
        newPos.y = Mathf.Lerp(newPos.y + minHeight, newPos.y + maxHeight, heightLerp);

        model.transform.position = newPos;

        heightLerp += Time.deltaTime * hoverSpeed * direction;
        if(heightLerp >= 1 || heightLerp <= 0)
        {
            //heightLerp = 1;
            direction *= -1;
            
            if(collected && heightLerp >= 1)
            {
                Destroy(gameObject);
            }
        }

        Vector3 newEuler = transform.eulerAngles;
        newEuler.y = Mathf.LerpAngle(newEuler.y, newEuler.y + 90, Time.deltaTime *rotateSpeed);
        transform.eulerAngles = newEuler;

        heightLerp = Mathf.Clamp(heightLerp, 0, 1);

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !collected)
        {
            collected = true;
            rotateSpeed *= 3;
            hoverSpeed *= 1.5f;
            maxHeight *= 2;
            totalLoops += 1;
            heightLerp = 0;
            other.transform.parent.GetComponent<PlayerMovement>().GetCollectable();
        }
    }

}
