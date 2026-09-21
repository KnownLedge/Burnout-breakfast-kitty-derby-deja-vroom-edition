using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    //NETPLAY VARIABLES
    [SerializeField] private bool PLAYING_ONLINE = false;

    [SerializeField] public int playerID = -1;

    [SerializeField] public List<bool> isPlayer;

    [SerializeField] private float raceProg; //Store how far player is in the race, combining laps, checkpoints and distance all into one variable

    [SerializeField] private NetworkRaceManager raceManager;

    [SerializeField] private bool finishedRace = false;

    [SerializeField] private int finishedPlayerCount = 0;

    //SYSTEM VARIABLES

    [SerializeField] public int lapsRequiredToWin; //Set in the inspector

    [SerializeField] public Checkpoint[] checkpoints; //Assign in the inspector


    [SerializeField] private GameObject playerReference; //Assign in the inspector
     public PlayerMovement playerMovement;

    [HideInInspector] public int currentPlayerCheckpoint;
    public int currentPlayerLap;

    //Note, moved some of these variables public so the Rival cpu can tell if the player is close to winning or not

    [SerializeField] private GameObject[] npcRacerReferences; //Assign in the inspector

    [HideInInspector] public int[] currentNPCRacerCheckpoints;
    public int[] currentNPCRacerLaps;

    [SerializeField] int playerPosition;

    bool raceActive = true; //True while player is still racing

    public PositionText uiRefPos;



    public List<int> raceResults;

    public ResultsScreen resultsScreenRef;

    void Start()
    {
        PLAYING_ONLINE = NetworkInfo.PLAYING_ONLINE;
        currentPlayerCheckpoint = 0;
        currentPlayerLap = 0;
        if (PLAYING_ONLINE)
        {
            raceManager = NetworkManager.Singleton.GetComponent<NetworkRaceManager>();
        }

        currentNPCRacerCheckpoints = new int[npcRacerReferences.Length];
        currentNPCRacerLaps = new int[npcRacerReferences.Length];

        for (int i = 0; i < npcRacerReferences.Length; i++)
        {
            currentNPCRacerCheckpoints[i] = 0;
            currentNPCRacerLaps[i] = 0;
            isPlayer.Add(false);
        }
        if (PLAYING_ONLINE)
        {
            isPlayer.Add(false);
        }
        StartCoroutine(UpdateRacePosition()); //Figure out what position in the race the player is in (1st, 2nd, 3rd, etc)
    }

    public void receiveOnlinePlayer(int newPlayerID) //Passed by any player *not* owned by this system, provides an id to replace npc player checking with
    {
        isPlayer[newPlayerID] = true;
    }

    public void receiveSystemPlayer(int localPlayerID, GameObject newPlayerRef)// Passed by the player owned by this system, gives the player id so the system knows which id to ignore entirely
    {
        playerID = localPlayerID;
        playerReference = newPlayerRef;
        isPlayer[playerID] = true;
        playerMovement = playerReference.GetComponentInParent<PlayerMovement>();
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
                if (PLAYING_ONLINE)
                {
                   if (finishedRace == false) {
                        finishedRace = true;
                        finishedPlayerCount += 1;
                        raceResults.Add(playerID);
                        playerMovement.DeclareRaceFinishServerRpc(playerID);
                        if(finishedPlayerCount == LobbyScript.expectedPlayers)
                        {
                            ForceRaceResults();
                        }
                        // ForceRaceResults();
                   }
                }
                else
                {
                    raceResults.Add(5);
                    ForceRaceResults();
                }
            }
        }
    }

    public void ReceiveOnlineFinish(int finishID)
    {
        raceResults.Add(finishID);
        finishedPlayerCount += 1;
        if (finishedPlayerCount == LobbyScript.expectedPlayers)
        {
            ForceRaceResults();
        }
        // ForceRaceResults();
    }

    private void CheckPlayersFinished()
    {
        bool isFinished = true;
        foreach(PlayerMovement player in raceManager.ActivePlayers)
        {

        }
    }

    public void ForceRaceResults()
    {
        for (int i = 0; i < npcRacerReferences.Length; i++)
        {
            bool hasFinished = false;
            foreach (int val in raceResults)
            {
                if (val == i)
                {

                        hasFinished = true;
                    
                    //Racer is in the results list, no need to add them
                }
            }
            if (!hasFinished)
            {
                raceResults.Add(i);
            }
        }
        if(resultsScreenRef != null)
        {
            resultsScreenRef.gameObject.SetActive(true);
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
                    bool hasFinished = false;
                    foreach (int val in raceResults)
                    {
                        if (val == npcRacerNumber)
                        {

                            hasFinished = true;

                            //Racer is in the results list, no need to add them
                        }
                    }
                    if (!hasFinished)
                    {
                        raceResults.Add(npcRacerNumber);
                    }
                
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
                if (isPlayer[i] == false && i != playerID) //If I is checking an actual npc player
                {
                    if (npcRacerReferences[i] != null) {
                        if (currentNPCRacerLaps[i] < currentPlayerLap)
                        {
                            newPlayerPosition--; //Player is a lap ahead of Npc
                        }
                        else if (currentNPCRacerLaps[i] == currentPlayerLap && currentNPCRacerCheckpoints[i] < currentPlayerCheckpoint)
                        {
                            newPlayerPosition--; //Player is at least one checkpoint ahead of Npc
                        }
                        else if (currentNPCRacerLaps[i] == currentPlayerLap && currentNPCRacerCheckpoints[i] == currentPlayerCheckpoint)
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

                            if (playDist < npcDist)
                            {
                                //Player is closer to reaching next checkpoint, therefore they are ahead.
                                newPlayerPosition--;
                            }
                        }
                    }
                }
                else if (isPlayer[i]) //If I is checking a online player (substituting a bot)
                {
                    if(raceManager.ActivePlayers[i].raceProgress.Value < raceProg)
                    {
                        newPlayerPosition--;
                        //Player has a higher race progress variable, therefore they should be ahead
                    }
                }
                else if(i == playerID)// If we're checking ourselves, update our net info
                {
                    int targetCheckpoint = currentPlayerCheckpoint + 1;

                    if (targetCheckpoint == checkpoints.Length) //If players are headed for the last checkpoint
                    {
                        targetCheckpoint = 0;
                    }
                    float distToNextCheck = Vector3.Distance(playerReference.transform.position, checkpoints[targetCheckpoint].transform.position);
                    float checkDist = Vector3.Distance(checkpoints[currentPlayerCheckpoint].transform.position, checkpoints[targetCheckpoint].transform.position);
                    float checkProgPerc = 1 - (distToNextCheck / checkDist); //Get percentage distance to next checkpoint as a decimal, that we can throw on the end of our progress
                    raceProg = ((currentPlayerLap * checkpoints.Length) + currentPlayerCheckpoint) + checkProgPerc;
                    //Creates a tally of all checkpoints passed, with a decimal value estimating the distance
                    playerMovement.setRaceProgress(Mathf.Round(raceProg * 1000) / 1000);

                    newPlayerPosition--;
                    //We can overtake ourselves, its only fair.
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
