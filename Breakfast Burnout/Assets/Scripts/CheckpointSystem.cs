using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    [SerializeField] private int lapsRequiredToWin; //Set in the inspector

    [SerializeField] private Checkpoint[] checkpoints; //Assign in the inspector


    [SerializeField] private GameObject playerReference; //Assign in the inspector

    public int currentPlayerCheckpoint;
    public int currentPlayerLap;


    [SerializeField] private GameObject[] npcRacerReferences; //Assign in the inspector

    public int[] currentNPCRacerCheckpoints;
    public int[] currentNPCRacerLaps;


    void Start()
    {
        currentPlayerCheckpoint = 0;
        currentPlayerLap = 0;


        currentNPCRacerCheckpoints = new int[npcRacerReferences.Length];
        currentNPCRacerLaps = new int[npcRacerReferences.Length];

        for (int i = 0; i < npcRacerReferences.Length; i++)
        {
            currentNPCRacerCheckpoints[i] = 0;
            currentNPCRacerLaps[i] = 0;
        }
    }

    //On passing a checkpoint, check if the player has completed a lap, or won the game.
    public void PlayerPassedCheckpoint()
    {
        currentPlayerCheckpoint++;

        if(currentPlayerCheckpoint == checkpoints.Length)
        {
            currentPlayerLap++;
            currentPlayerCheckpoint = 0;

            if (currentPlayerLap == lapsRequiredToWin)
            {
                print("You win! :D");
            }
        }
    }

    //On passing a checkpoint, check if the npc racer has completed a lap, or won the game.
    public void NPCRacerPassedCheckpoint(int npcRacerNumber)
    {
        currentNPCRacerCheckpoints[npcRacerNumber]++;

        if (currentNPCRacerCheckpoints[npcRacerNumber] == checkpoints.Length)
        {
            currentNPCRacerLaps[npcRacerNumber]++;
            currentNPCRacerCheckpoints[npcRacerNumber] = 0;

            if (currentNPCRacerLaps[npcRacerNumber] == lapsRequiredToWin)
            {
                print("NPC racer " + (npcRacerNumber + 1) + " won! >:(");
            }
        }
    }

    //Respawn either the player or an npc racer to the last checkpoint if they fall off the map. Todo: Reset their velocity and facing direction?
    public void Respawn(GameObject entity)
    {
        if(entity.CompareTag("Player"))
        {
            entity.transform.position = checkpoints[currentPlayerCheckpoint].gameObject.transform.position;
        }
        if (entity.CompareTag("NPC Racer"))
        {
            entity.transform.position = checkpoints[currentNPCRacerCheckpoints[entity.GetComponentInParent<AiRace>().npcRacerIndex]].gameObject.transform.position;
        }
    }
}
