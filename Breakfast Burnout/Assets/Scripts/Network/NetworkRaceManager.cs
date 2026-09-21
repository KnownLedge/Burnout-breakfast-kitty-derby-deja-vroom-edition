using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkRaceManager : MonoBehaviour
{
    public List<PlayerMovement> ActivePlayers; //List of all the players in the scene, mostly for counting
    public PlayerMovement HostPlayer;
    public int loadedPlayerCount = 0;
    public bool netReady = false;
    public void ReceivePlayer(PlayerMovement newPlayer)
    {
        ActivePlayers.Add(newPlayer);
        loadedPlayerCount = ActivePlayers.Count;
    }

    public void ReceiveHost(PlayerMovement hostPlayer)
    {
        HostPlayer = hostPlayer;
    }

    public bool CheckNetReady()
    {//Function for checking if every player is readied up, only the host should be calling this
        bool isReady = true;
        foreach (PlayerMovement player in ActivePlayers)
        {
            if(player.isNetReady== false)
            {
                isReady = false;
            }

        }
        return isReady;
    }

    public void DeclareNetReady()
    {
        int i = 0;
        foreach (PlayerMovement player in ActivePlayers) {
            player.SetPlayerIdServerRpc(i);
            player.SetNetReadyClientRpc();
            i++;
        }
    }

    public void DeclareRaceStart()
    {
        foreach (PlayerMovement player in ActivePlayers)
        {
            player.StartRaceClientRpc();

        }
    }
}
