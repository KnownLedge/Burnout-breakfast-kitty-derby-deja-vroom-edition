using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    [SerializeField] public int lapsRequiredToWin; //Set in the inspector

    [SerializeField] public Checkpoint[] checkpoints; //Assign in the inspector


    [SerializeField] private GameObject playerReference; //Assign in the inspector

    [HideInInspector] public int currentPlayerCheckpoint;
    public int currentPlayerLap;

    //Note, moved some of these variables public so the Rival cpu can tell if the player is close to winning or not

    [SerializeField] private GameObject[] npcRacerReferences; //Assign in the inspector

    [HideInInspector] public int[] currentNPCRacerCheckpoints;
    public int[] currentNPCRacerLaps;

    [SerializeField] int playerPosition;

    bool raceActive = true; //True while player is still racing

    public PositionText uiRefPos;
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
        StartCoroutine(UpdateRacePosition()); //Figure out what position in the race the player is in (1st, 2nd, 3rd, etc)
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
            entity.transform.rotation = checkpoints[currentPlayerCheckpoint].gameObject.transform.rotation;
            entity.transform.Rotate(0,-90f,0);
            //Checkpoints face to the right, turn 90 degrees left to face the track

            PlayerMovement playerScript = entity.transform.parent.GetComponent<PlayerMovement>();

            if (playerScript != null) { 
            playerScript.plrKart.transform.rotation = checkpoints[currentPlayerCheckpoint].gameObject.transform.rotation;

                playerScript.plrKart.transform.Rotate(0, -90f, 0);
                //Checkpoints face to the right, turn 90 degrees left to face the track
                playerScript.RespawnStats();
                //Call a function to reset players acceleration
            }
        }
        if (entity.CompareTag("NPC Racer"))
        {
            entity.transform.position = checkpoints[currentNPCRacerCheckpoints[entity.GetComponentInParent<AiRace>().npcRacerIndex]].gameObject.transform.position;
            entity.transform.rotation = checkpoints[currentPlayerCheckpoint].gameObject.transform.rotation;
            entity.transform.Rotate(0, -90f, 0);

            AiRace npcScript = entity.transform.parent.GetComponent<AiRace>();
            if (npcScript != null) { 
            npcScript.aiKart.transform.rotation = checkpoints[currentPlayerCheckpoint].gameObject.transform.rotation;

                npcScript.aiKart.transform.Rotate(0, -90f, 0);
                
                npcScript.RespawnStats();
            
            }
        }
    }
    IEnumerator UpdateRacePosition()
    {
        while (raceActive)
        {
           int newPlayerPosition = npcRacerReferences.Count() + 1; //Default player to last place
            for (int i = 0; i < npcRacerReferences.Count(); i++)
            {
                if (currentNPCRacerLaps[i] < currentPlayerLap)
                {
                    newPlayerPosition--; //Player is a lap ahead of Npc
                }else if (currentNPCRacerLaps[i] == currentPlayerLap && currentNPCRacerCheckpoints[i] < currentPlayerCheckpoint)
                {
                    newPlayerPosition--; //Player is at least one checkpoint ahead of Npc
                }else if(currentNPCRacerLaps[i] == currentPlayerLap && currentNPCRacerCheckpoints[i] == currentPlayerCheckpoint)
                {
                    //Player and opponent Npc are tied on checkpoints, have to use distance check to see if player is ahead
                    int targetCheckpoint = currentPlayerCheckpoint + 1;

                    if (targetCheckpoint == checkpoints.Length) //If players are headed for the last checkpoint
                    {
                        targetCheckpoint = 0;
                    }

                        float playDist = Vector3.Distance(playerReference.transform.position, checkpoints[targetCheckpoint].transform.position);
                    //Get player distance to next checkpoint
                    float npcDist = Vector3.Distance(npcRacerReferences[i].transform.position, checkpoints[targetCheckpoint].transform.position);
                    //Get npc distance to next checkpoint

                    if(playDist < npcDist)
                    {
                        //Player is closer to reaching next checkpoint, therefore they are ahead.
                        newPlayerPosition--;
                    }
                }


            }
            if(newPlayerPosition != playerPosition)
            {
                playerPosition = newPlayerPosition;
                if (uiRefPos != null)
                {
                    uiRefPos.updatePosText(newPlayerPosition);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
