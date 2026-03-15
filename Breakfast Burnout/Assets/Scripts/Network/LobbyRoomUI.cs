using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyRoomUI : MonoBehaviour
{
    //Script to store references to gameobjects in the lobby room UI, 
    //so it can be easilly referenced without finding the objects manually in each prefab

    public string ownerName;
    public TMP_Text OwnerText;
    public TMP_Text playerCountText;
    public Button joinButton;


}
