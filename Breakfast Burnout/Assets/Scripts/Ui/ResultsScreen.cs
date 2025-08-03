using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    void Start()
    {
     startPos = standingsObj.localPosition;   
        if(checkRef != null)
        {
            for(int i = 0; i < checkRef.raceResults.Count; i++)
            {
                backgroundImages[i].color = playerColors[checkRef.raceResults[i]];
                racerTitles[i].text = npcNames[checkRef.raceResults[i]];
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
                        SceneManager.LoadScene("MainMenu");
                    }
                    else
                    {
                        SceneManager.LoadScene("ErykTestScene");
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
