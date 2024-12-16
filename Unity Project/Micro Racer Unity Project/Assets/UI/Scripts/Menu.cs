using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public abstract class Menu : NetworkBehaviour
{
    private protected NetworkManager _networkManager;

    private protected LocalConnectionState _clientState;
    private protected LocalConnectionState _serverState;

    private void Start()
    {
        _networkManager = FindFirstObjectByType<NetworkManager>();

        _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _clientState = obj.ConnectionState;
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        _serverState = obj.ConnectionState;
    }
}
