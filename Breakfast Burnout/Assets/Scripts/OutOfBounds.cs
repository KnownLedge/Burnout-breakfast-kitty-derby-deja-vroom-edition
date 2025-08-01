using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField] private CheckpointSystem checkpointSystemReference; //Assign in the inspector

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("NPC Racer"))
        {
            checkpointSystemReference.Respawn(other.gameObject);
        }
    }
}
