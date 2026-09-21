using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsScreen : MonoBehaviour
{
    public Transform standingsObj;
    public float lerpSpeed = 3f;
    private int state = 0; //It is too late to do an enum (it would take 3 seconds)
    public FruitLoopsFill FLScript;
    public Vector3 centerPos;
    private Vector3 startPos;

    public List<string> npcNames;
    public List<Color> playerColors;
    public CheckpointSystem checkRef;

    public List<Image> backgroundImages;
    public List<TMP_Text> racerTitles;

    //ONLINE
    public NetworkRaceManager raceManager;

    void Start()
    {
        if (NetworkInfo.PLAYING_ONLINE)
        {
            raceManager = NetworkManager.Singleton.GetComponent<NetworkRaceManager>();
        }
     startPos = standingsObj.localPosition;   
        if(checkRef != null)
        {
            for(int i = 0; i < checkRef.raceResults.Count; i++)
            {
                int checkedID = checkRef.raceResults[i];
                if (!NetworkInfo.PLAYING_ONLINE ||  checkRef.isPlayer[checkedID] == false && checkRef.playerID != checkedID)
                {
                    backgroundImages[i].color = playerColors[checkedID];
                    racerTitles[i].text = npcNames[checkedID];
                }
                else if(checkRef.isPlayer[checkedID]) 
                {
                    if (raceManager.ActivePlayers[checkedID].playerKart.Value - 1 != -1)
                    {
                        backgroundImages[i].color = playerColors[raceManager.ActivePlayers[checkedID].playerKart.Value - 1]; //Set background color to color of players kart
                    }
                    else
                    {
                        backgroundImages[i].color = playerColors[playerColors.Count - 1];
                    }
                        racerTitles[i].text = raceManager.ActivePlayers[checkedID].playerName.Value.ToString(); //Sets character name to player name, IDENTIFICATION!!!
                }
                else //Dealing with system player
                {

                    if (raceManager.ActivePlayers[checkedID].playerKart.Value - 1 != -1)
                    {
                        backgroundImages[i].color = playerColors[checkRef.playerMovement.playerKart.Value - 1]; //Set background color to color of players kart
                    }
                    else
                    {//Loop around to default player color
                        backgroundImages[i].color = playerColors[playerColors.Count - 1];
                    }

                    racerTitles[i].text = checkRef.playerMovement.playerName.Value.ToString(); //Sets character name to player name, IDENTIFICATION!!!
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (FLScript.isFill)
        {
            if (state == 0)
            {
                standingsObj.localPosition = Vector3.Lerp(standingsObj.localPosition, centerPos, lerpSpeed * Time.deltaTime);

                if (Vector3.Distance(standingsObj.localPosition, centerPos) < 9 && Input.GetButtonDown("Jump")) { 
                state = 1;
                }
            }
            else if (state == 1) { 
                standingsObj.localPosition = Vector3.Lerp(standingsObj.localPosition, startPos, lerpSpeed * Time.deltaTime);

                if (Vector3.Distance(standingsObj.localPosition, startPos) < 9)
                {
                    FLScript.shouldFill = false;
                    if (checkRef.raceResults[0] == 5)
                    {
                        SceneManager.LoadScene("WinMenu");
                    }
                    else
                    {
                        SceneManager.LoadScene("AshTestScene");
                    }
                    state = 2;
                }
            }



        }
        else if(state == 2)
        {
            //LOAD NEXT SCENE HERE

            gameObject.SetActive(false);
        }
    }
}
