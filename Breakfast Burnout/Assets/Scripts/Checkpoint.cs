using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private CheckpointSystem checkpointSystemReference; //Assign in the inspector

    public int checkpointNumber; //Assign in the inspector

    private void OnTriggerEnter(Collider other)
    {
        //If this is the next checkpoint that the player needs to pass,
        if (other.gameObject.CompareTag("Player") && checkpointNumber == checkpointSystemReference.currentPlayerCheckpoint + 1)
        {
            if (NetworkInfo.PLAYING_ONLINE)
            {
                PlayerMovement playerScript = other.gameObject.GetComponentInParent<PlayerMovement>();
                if ((playerScript.IsOwner))
                {
                    checkpointSystemReference.PlayerPassedCheckpoint(); //Then have them pass the checkpoint
                    //This is only being allowed for the owner object so players don't share race progress
                }
            }
            else
            {
                checkpointSystemReference.PlayerPassedCheckpoint(); //Then have them pass the checkpoint
            }
        }
        //If this is the next checkpoint that the npc racer needs to pass,
        if (other.gameObject.CompareTag("NPC Racer") && checkpointNumber == checkpointSystemReference.currentNPCRacerCheckpoints[other.GetComponentInParent<AiRace>().npcRacerIndex] + 1)
        {
            checkpointSystemReference.NPCRacerPassedCheckpoint(other.GetComponentInParent<AiRace>().npcRacerIndex); //Then have them pass the checkpoint, and pass over their racer index too
        }
    }
}
