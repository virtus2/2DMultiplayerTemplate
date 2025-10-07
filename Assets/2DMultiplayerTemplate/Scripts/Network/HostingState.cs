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

    public override void Disconnect()
    {
        connectionManager.ChangeState(EConnectionState.Offline);
    }

    public override void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"HandleClientConnected: clientId({clientId})");
        if (!networkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

        // bool hasAuthority = networkManager.IsServer || networkManager.IsHost;
        // if (hasAuthority)
        {
            Debug.Log($"CreatePlayerCharacter: clientId({clientId})");
            GameManager.Instance.HandleClientConnected(clientId);
        }
    }

    public override void HandleClientDisconnected(ulong clientId)
    {

    }

    public override void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;

        if (clientId == networkManager.LocalClientId)
        {
            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            Debug.Log($"{connectionPayload}");
            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        response.Approved = true;
        response.CreatePlayerObject = true;
    }
}
