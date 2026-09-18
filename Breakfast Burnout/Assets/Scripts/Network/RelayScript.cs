using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayScript : MonoBehaviour
{
    //made by following codemonkeys relay tutorial
    //https://www.youtube.com/watch?v=msPNJ2cxWfw&t=6s
    private async void Start()
    {
        //await UnityServices.InitializeAsync();

        //AuthenticationService.Instance.SignedIn += () =>
        //{
        //    Debug.Log("Signed in " + AuthenticationService.Instance.SignInAnonymouslyAsync());
        //};

        //await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }


    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5); // Change this to 5 to allow 6 players!
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            //Code partly assisted by ai (I know mostly what its doing, but i feel like i should state this)
            //(try looking up how to do this it legit don't exist :( )
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.UseWebSockets = true;

            string connectionType = "wss";
            RelayServerData relayData = AllocationUtils.ToRelayServerData(allocation, connectionType);

            transport.SetRelayServerData(relayData);
            //end of ai assisted code

            NetworkManager.Singleton.StartHost();

            return joinCode;

        }
        catch (RelayServiceException e)
        {

            Debug.Log(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
    joinAllocation.RelayServer.IpV4,
    (ushort)joinAllocation.RelayServer.Port,
    joinAllocation.AllocationIdBytes,
    joinAllocation.Key,
    joinAllocation.ConnectionData,
    joinAllocation.HostConnectionData
    );

            //Code partly assisted by ai (I know mostly what its doing, but i feel like i should state this)
            //(try looking up how to do this it legit don't exist :( )
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.UseWebSockets = true;

            string connectionType = "wss";
            RelayServerData relayData = AllocationUtils.ToRelayServerData(joinAllocation, connectionType);

            transport.SetRelayServerData(relayData);
            //end of ai assisted code


            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
