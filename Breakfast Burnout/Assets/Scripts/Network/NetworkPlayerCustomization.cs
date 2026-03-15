using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayerCustomization : MonoBehaviour
{
    [SerializeField] private GameObject UiHolder;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private LobbyScript lobbyRef;
    public string playerName = "Player";
    public int PlayerIconId = 0;
    public int playerIconColorId = 0;

    public TMP_Text playerNameText;

    public TMP_InputField playerNameInput;

    public Image playerIconImage;
    public Image playerIconBackground;


    public List<Sprite> IconSprites;
    public List<Color> IconColors;


    public void UpdateName()
    {
        if (playerNameInput.text != "" && playerNameInput.text.Length < 12)
        {
            playerName = playerNameInput.text;
            playerNameText.text = playerName;
        }
    }

    public void UpdateIcon(int id)
    {
        PlayerIconId = id;
        playerIconImage.sprite = IconSprites[PlayerIconId];
    }

    public void UpdateColor(int id)
    {
        playerIconColorId = id;
        playerIconBackground.color = IconColors[id];
    }

    public void CompleteCustomization()
    {
        lobbyRef.playerName = playerName;
        lobbyRef.IconID = PlayerIconId;
        lobbyRef.ColorID = playerIconColorId;

        UiHolder.SetActive(false); //Hide ui
        lobbyUI.SetActive(true);
    }

}
