using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RivalRace : AiRace
{


    public PlayerMovement playerRef;

    public enum RivalState {Regular, Rush, What};
    public RivalState State;
    public CheckpointSystem checkRef;
    public List<Transform> rushPoints;
    public float rushTime = 1.5f;
    public float rushTimer = 0f; //Should make private later
    public float playerDist = 0f;
    public AudioSource audio;
    public AudioClip rushSound;

    [SerializeField] private bool hasRushed = false;

    public int rushCheckPoint = 3; //Checkpoint that rival will go into rush mode once player reaches

    new void Start()
    {
        base.Start();
        if(checkRef == null)
        {
            GameObject.Find("Checkpoint System").GetComponent<CheckpointSystem>();
            //Failsafe attempt to get checkpoint system if one is not manually set.
        }
    }

    new void Update()
    {
        if (State == RivalState.Regular)
        {
            base.Update();

            if(checkRef.currentPlayerCheckpoint >= rushCheckPoint && checkRef.currentPlayerLap == checkRef.lapsRequiredToWin - 1 && !hasRushed){
                //If player is nearing finish line where we want to rush

                int extraLaps = checkRef.currentPlayerLap - checkRef.currentNPCRacerLaps[npcRacerIndex];
                //Get how many laps ahead the player is from the rival

                for (int i = checkRef.currentNPCRacerLaps[npcRacerIndex]; i < wayPoints.Count; i++)
                {
                    rushPoints.Add(wayPoints[i]);
                }

                while (extraLaps > 0)
                {
                    foreach (Transform wayP in wayPoints)
                    {
                        rushPoints.Add(wayP);
                        extraLaps--;
                    }
                }
                rushPoints.Add(wayPoints[1]);

                playerDist = Vector3.Distance(playerRef.plrObj.transform.position, checkRef.checkpoints[0].transform.position);

                State = RivalState.Rush;
                audio.PlayOneShot(rushSound);
            }

        }
        else if(State == RivalState.Rush)
        {
            rushTimer += Time.deltaTime;
            float rushLerp = rushTimer / rushTime; //Get percentage completion of rushtimer for lerp

            float playerLerp = 1.1f - (Vector3.Distance(playerRef.plrObj.transform.position, checkRef.checkpoints[0].transform.position) / playerDist);
            Debug.Log("playlerp: " + playerLerp + " otherlerp " + rushLerp);
            //Get percentage value of how close player is to finish line
            if(playerLerp > rushLerp)
            {
                rushLerp = playerLerp;
                rushTimer = rushTime * rushLerp;
            }

            if (rushLerp > 1)
            {
                rushLerp = 1;
                hasRushed = true;
            }

            float extendLerp = rushPoints.Count; //Extend the lerp by each checkpoint
            rushLerp = rushLerp * extendLerp;

            int rushTargetIndex = 0;

            while(rushLerp > 1)
            {
                rushLerp--;
                rushTargetIndex++;
            }
            if(rushTargetIndex == 0)
            {
                physObj.transform.position = Vector3.Lerp(transform.position, rushPoints[rushTargetIndex].transform.position, rushLerp);
            }
            else
            {
                physObj.transform.position = Vector3.Lerp(rushPoints[rushTargetIndex - 1].transform.position, rushPoints[rushTargetIndex].transform.position, rushLerp);
            }

            if (hasRushed)
            {
                State = RivalState.What;
                targetIndex = 0;
                currentTarget = wayPoints[targetIndex];

                checkRef.currentNPCRacerCheckpoints[npcRacerIndex] = checkRef.checkpoints.Length - 1;
                checkRef.currentNPCRacerLaps[npcRacerIndex] = checkRef.lapsRequiredToWin - 1;
                checkRef.NPCRacerPassedCheckpoint(npcRacerIndex);
                enabled = false;
            }

        }
    }
}
