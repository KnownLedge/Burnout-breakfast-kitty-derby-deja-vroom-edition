using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPadStream : MonoBehaviour
{
    private Color toolTipColor = new Color(0.5f, 0.5f, 1f, 0.75f); //Feel free to change this colour to whatever you want

    [SerializeField] private Vector3 boostForce = new Vector3(0, 0, 1);
    //Direction and magnitude of the boost (only magnitude if Local or Player direction is used)

    private void OnTriggerEnter(Collider other)
    {
        if ((other.tag == "Player" || other.tag == "NPC Racer") && other.GetComponentInParent<PlayerMovement>())
        {
            ApplyWaterBoost(other.GetComponentInParent<PlayerMovement>());
        }else if((other.tag == "Player" || other.tag == "NPC Racer") && other.GetComponentInParent<AiRace>())
        {
            ApplyWaterBoost(other.GetComponentInParent<AiRace>());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((other.tag == "Player" || other.tag == "NPC Racer") && other.GetComponentInParent<PlayerMovement>())
        {
            ApplyWaterBoost(other.GetComponentInParent<PlayerMovement>());
        }
        else if ((other.tag == "Player" || other.tag == "NPC Racer") && other.GetComponentInParent<AiRace>())
        {
            ApplyWaterBoost(other.GetComponentInParent<AiRace>());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.body.tag == "Player" && collision.body.GetComponentInParent<PlayerMovement>()) || collision.body.tag == "NPC Racer")
        {
            ApplyWaterBoost(collision.body.GetComponentInParent<PlayerMovement>());
        }
        else if ((collision.body.tag == "Player" || collision.body.tag == "NPC Racer") && collision.body.GetComponentInParent<AiRace>())
        {
            ApplyWaterBoost(collision.body.GetComponentInParent<AiRace>());
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((collision.body.tag == "Player" && collision.body.GetComponentInParent<PlayerMovement>()) || collision.body.tag == "NPC Racer")
        {
            ApplyWaterBoost(collision.body.GetComponentInParent<PlayerMovement>());
        }
        else if ((collision.body.tag == "Player" || collision.body.tag == "NPC Racer") && collision.body.GetComponentInParent<AiRace>())
        {
            ApplyWaterBoost(collision.body.GetComponentInParent<AiRace>());
        }
    }

    private void ApplyWaterBoost(PlayerMovement playRef)
    {
        playRef.ApplyExternalBoost(boostForce);
    }
    private void ApplyWaterBoost(AiRace playRef) //Ai overRide
    {
        Debug.Log("BOOSTING AI");
        playRef.ApplyExternalBoost(boostForce);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = toolTipColor;
            Gizmos.DrawLine(transform.position, transform.position + boostForce * 10f);
            Gizmos.DrawSphere(transform.position + boostForce * 10f, 1.25f);
        
    }
}