using Unity.Netcode;
using UnityEngine;

public class HostingState : ConnectionState
{
    public HostingState(ConnectionManager connectionManager) : base(connectionManager) { }

    public override void Enter()
    {
        connectionManager.ConnectionMethod.HandleHostStartedSuccessfully();
    }

    public override void Exit()
    {
        connectionManager.ConnectionMethod.SetupDisconnect();
        networkManager.Shutdown();
    }

    public override void OnUserRequestedShutdown()
    {
        connectionManager.ChangeState(EConnectionState.Offline);
    }

    public override void HandleClientConnected(ulong clientId)
    {
        if (!networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

        GameManager.Instance.CreatePlayerCharacterOnClientConnected(clientId);
        GameManager.Instance.InitializeServerChunkOnClientConnected(clientId);
    }

    public override void HandleClientDisconnected(ulong clientId)
    {

    }

    public override void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;

        var payload = System.Text.Encoding.UTF8.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
        var connectStatus = GetConnectStatus(connectionPayload);

        if (connectStatus == EConnectStatus.Success)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            return;
        }

        response.Approved = false;
        response.CreatePlayerObject = false;
        response.Reason = JsonUtility.ToJson(connectStatus);
    }

    private EConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
    {
        if (networkManager.ConnectedClientsIds.Count >= connectionManager.MaxConnectedPlayers)
        {
            return EConnectStatus.ServerFull;
        }

        if(connectionPayload.isDebug != Debug.isDebugBuild)
        {
            return EConnectStatus.IncompatibleBuildType;
        }

        // TODO: Handle LoggedInAgain
        // return EConnectStatus.LoggedInAgain
        return EConnectStatus.Success;
    }
}
