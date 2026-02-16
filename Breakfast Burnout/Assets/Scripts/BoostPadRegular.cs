using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPadRegular : MonoBehaviour
{
    private Color toolTipColor = new Color(0.5f, 0.5f, 1f, 0.75f); //Feel free to change this colour to whatever you want

    [SerializeField] private Vector3 boostForce = new Vector3(0,0,1);
    //Direction and magnitude of the boost (only magnitude if Local or Player direction is used)
    public enum BDirection {Set, LocalToObject, PlayerDirection}
    [Tooltip("What direction to use for the boost, Set: set by value Local: set by object direction, Player: set by players current direction")]
    public BDirection BoostDirection = BDirection.Set;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.tag == "Player" || other.tag == "NPC Racer") && other.GetComponent<Rigidbody>())
        {
            ApplyBoost(other.GetComponent<Rigidbody>());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.body.tag == "Player" || collision.body.tag == "NPC Racer")
        {
            ApplyBoost(collision.rigidbody);
        }
    }

    private void ApplyBoost(Rigidbody rb)
    {
        if (BoostDirection == BDirection.Set)
        {
            rb.AddForce(boostForce, ForceMode.Impulse);
        }
        else if (BoostDirection == BDirection.LocalToObject) { 
        rb.AddForce(transform.forward * boostForce.magnitude, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(rb.velocity.normalized * boostForce.magnitude, ForceMode.Impulse);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = toolTipColor;
        if (BoostDirection == BDirection.LocalToObject)
        {
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * boostForce.magnitude * 0.5f);
            Gizmos.DrawSphere(transform.position + transform.forward * boostForce.magnitude * 0.5f, 2.5f);
            
        }
        else
        {
          Gizmos.DrawLine(transform.position, transform.position + boostForce * 0.5f);
            Gizmos.DrawSphere(transform.position + boostForce * 0.5f, 2.5f);
        }
    }
}
