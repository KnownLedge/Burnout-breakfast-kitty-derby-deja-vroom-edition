using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyScript : MonoBehaviour
{
    //Mostly copy pasted from my networking project, very shoddy implementation of a lobby system

    //Functions learned from CodeMonkeys Lobby and Relay tutorial, however all UI and maingame connections are done by me
    // https://www.youtube.com/watch?v=-KDlEBfCBiU&t=1071s
    //https://www.youtube.com/watch?v=msPNJ2cxWfw&t=6s

    private Lobby hostLobby;
    private Lobby joinLobby;
    private Lobby currentLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float lobbyUpdateTimer;
    private float lobbyUpdateTimerMax = 1.1f;
    public string playerName;
    public int IconID = 0;
    public int ColorID = 0;
    public TMP_InputField playerNameInputField;
    [SerializeField] private float lobbyUIDistance = 70f;

    [SerializeField] private GameObject lobbyList;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject createLobbyButton;

    [SerializeField] private GameObject joinedLobbyUI;
    [SerializeField] private List<TMP_Text> joinedLobbyPlayerNames;
    [SerializeField] private List<Image> joinedLobbyPlayerBackground;
    [SerializeField] private List<Image> joinedLobbyPlayerIcons;
    [SerializeField] private NetworkPlayerCustomization customizationInfo;

    [SerializeField] private int maxLobbyPlayerId = 5;
    [SerializeField] private string defaultGameScene = "RyanTestScene";

    public RelayScript relayScript;

    public static string gamePlayerName = "Player";
    public static int gameIconID = 0;
    public static int gameColorID = 0;
    public static int expectedPlayers = 2;

    private string KEY_START_GAME = "0";

    private string lobbyCode;

    private async void Start()
    {

        playerName = "Player" + UnityEngine.Random.Range(10, 99); //Placeholder player name incase player avoids setting it somehow

        await UnityServices.InitializeAsync();


        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //makes new account

        Debug.Log(playerName);
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                UpdateJoinedLobbyUI(currentLobby);

            }
            else
            {

            }

        }
        else
        {
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0f)
            {
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinLobby.Id);
                joinLobby = lobby;
                UpdateJoinedLobbyUI(currentLobby);

                if (joinLobby.Data[KEY_START_GAME].Value != "0")
                {
                    gamePlayerName = playerName;
                    gameIconID = IconID;
                    gameColorID = ColorID;
                    expectedPlayers = lobby.Players.Count;
                    SceneManager.LoadScene(defaultGameScene);
                    if (hostLobby == null)
                    {
                        relayScript.JoinRelay(joinLobby.Data[KEY_START_GAME].Value);
                    }

                    joinLobby = null;

                }


            }
            else
            {
            }

        }
        else if (hostLobby != null)
        {

            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0f)
            {
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
                hostLobby = lobby;
                currentLobby = lobby;


            }
            else
            {

            }

        }
    }

    public async void CreateLobby() //public to connect to the create lobby button on the ui
    {
        try
        {
            string lobbyName = playerName + "'s Lobby";
            int maxPlayers = 6;

            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }

                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);

            hostLobby = lobby;
            currentLobby = hostLobby;

            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            lobbyCode = lobby.LobbyCode;
            createLobbyButton.SetActive(false);
            lobbyList.SetActive(false);
            joinedLobbyUI.SetActive(true);
            UpdateJoinedLobbyUI(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies() //Public to connect to Refresh button
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions
        {
            Count = 25,
            Filters = new List<QueryFilter>()
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            },
            Order = new List<QueryOrder> {
                new QueryOrder(false, QueryOrder.FieldOptions.Created )
            }
        };
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
            ShowLobbiesUi(queryResponse);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void ShowLobbiesUi(QueryResponse queryResponse)
    {
        foreach (Transform child in lobbyList.transform)
        {
            child.gameObject.SetActive(false);
            // child.gameObject.transform.SetParent(null);
            Destroy(child.gameObject);
        }
        for (int i = 0; i < queryResponse.Results.Count; i++)
        {
            GameObject newLobbyUI = GameObject.Instantiate(lobbyUI);
            newLobbyUI.transform.SetParent(lobbyList.transform, false);
            LobbyRoomUI lobbyinfo = newLobbyUI.GetComponent<LobbyRoomUI>();

            newLobbyUI.transform.position -= new Vector3(0, i * lobbyUIDistance, 0);

            lobbyinfo.ownerName = queryResponse.Results[i].Name;
            lobbyinfo.OwnerText.text = queryResponse.Results[i].Name;
            lobbyinfo.playerCountText.text = "Players " + queryResponse.Results[i].Players.Count + "/" + queryResponse.Results[i].MaxPlayers;
            Debug.Log("added ui thing at " + newLobbyUI.transform.position.y);

            string code = queryResponse.Results[i].Id;
            Debug.Log("Code is " + code);
            lobbyinfo.joinButton.onClick.AddListener(() => { JoinLobby(code); });
        }
    }

    private void UpdateJoinedLobbyUI(Lobby lobby)
    {
        for (int i = 0; i < lobby.Players.Count; i++)
        {
            Debug.Log("trying to update player info");
            joinedLobbyPlayerNames[i].text = lobby.Players[i].Data["PlayerName"].Value;

            joinedLobbyPlayerBackground[i].color = customizationInfo.IconColors[int.Parse(lobby.Players[i].Data["PlayerColor"].Value)];
            joinedLobbyPlayerIcons[i].sprite = customizationInfo.IconSprites[int.Parse(lobby.Players[i].Data["PlayerIcon"].Value)];
            joinedLobbyPlayerIcons[i].color = new Color(1, 1, 1, 1);
        }
        for (int i = maxLobbyPlayerId; i >= lobby.Players.Count; i--)
        {
            joinedLobbyPlayerNames[i].text = ""; //Hide playername, as no player is in this slot
            joinedLobbyPlayerBackground[i].color = new Color(0, 0, 0, 0);
            joinedLobbyPlayerIcons[i].sprite = null;
            joinedLobbyPlayerIcons[i].color = new Color(0, 0, 0, 0);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.T)) {
        //    CreateLobby();

        //}
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    ListLobbies();
        //}
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    JoinLobby(playerNameInputField.text);
        //}
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    PrintPlayers(hostLobby);
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    LeaveLobby();
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //  // await relayScript.CreateRelay();
        //}

        //Debug buttons, no longer needed and causes issue with name entry

        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
    }

    private async void JoinLobby(string lobbyID)
    {
        Debug.Log("trying to join a lobby!");
        try
        {
            JoinLobbyByIdOptions joinCodeOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            joinLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID, joinCodeOptions);
            currentLobby = joinLobby;
            Debug.Log("Joined Lobby with code " + lobbyCode);
            PrintPlayers(joinLobby);
            createLobbyButton.SetActive(false);
            lobbyList.SetActive(false);
            joinedLobbyUI.SetActive(true);
            UpdateJoinedLobbyUI(joinLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    //private async void JoinLobby(string lobbyCode)
    //{
    //    Debug.Log("trying to join a lobby!");
    //    try
    //    {
    //        JoinLobbyByCodeOptions joinCodeOptions = new JoinLobbyByCodeOptions
    //        {
    //            Player = GetPlayer()
    //        };
    //      joinLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinCodeOptions);

    //        Debug.Log("Joined Lobby with code " + lobbyCode);
    //        PrintPlayers(joinLobby);
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e);
    //    }
    //}

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log("tried to join lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                        {"PlayerIcon", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, IconID.ToString())},
                        {"PlayerColor", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ColorID.ToString())}
                    }
        };
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);

        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void LeaveLobby() //Public to connect to leave button on ui
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
            createLobbyButton.SetActive(true);
            joinedLobbyUI.SetActive(false);
            lobbyList.SetActive(true);
            hostLobby = null;
            joinLobby = null;
            currentLobby = null;
        }
        catch (LobbyServiceException e)
        {


        }
    }

    //private async string CreateRelay()
    //{
    //    try
    //    {
    //        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

    //        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

    //        Debug.Log(joinCode);

    //        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
    //            allocation.RelayServer.IpV4,
    //            (ushort)allocation.RelayServer.Port,
    //            allocation.AllocationIdBytes,
    //            allocation.Key,
    //            allocation.ConnectionData
    //            );
    //        NetworkManager.Singleton.StartHost();

    //        return joinCode;

    //    }
    //    catch (RelayServiceException e) {

    //        Debug.Log(e);
    //        return null;
    //    }
    //}

    public async void StartGame()
    {
        if (hostLobby != null)
        {
            try
            {

                gamePlayerName = playerName;
                gameIconID = IconID;
                gameColorID = ColorID;
                expectedPlayers = hostLobby.Players.Count;

                SceneManager.LoadScene(defaultGameScene);

                string relayCode = await relayScript.CreateRelay();

                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                        }
                });
                hostLobby = lobby;
                currentLobby = lobby;


            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }


}
