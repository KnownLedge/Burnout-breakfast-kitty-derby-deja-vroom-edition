using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FruitLoopsFill : MonoBehaviour
{
    private Image image;
    public bool fillOnAwake = false;
    public bool shouldFill = false;

    public TMP_Text playersLoadedText;

    private float fillTimer = 1f;

    public bool isFill = false;

    private NetworkRaceManager raceManager;

    void Start()
    {
        image = GetComponent<Image>();
        if (fillOnAwake)
        {
          shouldFill = !shouldFill;  
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkInfo.PLAYING_ONLINE)
        {
            raceManager = NetworkManager.Singleton.GetComponent<NetworkRaceManager>();
        }

        if (!NetworkInfo.PLAYING_ONLINE || raceManager.netReady)
        {
            if (playersLoadedText != null)
            {
                playersLoadedText.gameObject.SetActive(false);
            }
                if (shouldFill)
            {
                image.fillAmount += fillTimer * Time.deltaTime;
                if (image.fillAmount >= 1)
                    isFill = true;
            }
            else
            {
                image.fillAmount -= fillTimer * Time.deltaTime;
                if (image.fillAmount <= 0)
                    isFill = false;
            }
        }
        else if(playersLoadedText != null)
        {
            playersLoadedText.gameObject.SetActive(true);
            playersLoadedText.text = raceManager.loadedPlayerCount.ToString() +"/" + LobbyScript.expectedPlayers.ToString();
            if(raceManager.ActivePlayers.Count >= LobbyScript.expectedPlayers)
            {
                //playersLoadedText.gameObject.SetActive(false);
            }
        }
    }
}
