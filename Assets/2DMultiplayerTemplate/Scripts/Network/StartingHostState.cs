using Unity.Netcode;
using UnityEngine;

public class StartingHostState : ConnectionState
{
    public StartingHostState(ConnectionManager connectionManager) : base(connectionManager) { }

    public override void Enter()
    {
        Host();
    }

    public override void Exit()
    {
    }

    public override void Disconnect()
    {
        connectionManager.ChangeState(EConnectionState.Offline);
    }

    public override void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;

        // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
        if (clientId == networkManager.LocalClientId)
        {
            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            response.Approved = true;
            response.CreatePlayerObject = true;
        }
    }

    private void Host()
    {
        try
        {
            connectionManager.ConnectionMethod.SetupHostConnection();
            bool success = networkManager.StartHost();
            if (success)
            {
            }
            else
            {
                HostFailed();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            HostFailed();
        }
    }

    public override void HandleServerStarted()
    {
        connectionManager.ChangeState(EConnectionState.Hosting);
    }

    private void HostFailed()
    {
        connectionManager.ConnectionMethod.HandleHostStartFailed();
        connectionManager.ChangeState(EConnectionState.Offline);
    }
}
