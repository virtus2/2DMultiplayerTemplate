using UnityEngine;

public class ClientConnectedState : ConnectionState
{
    public ClientConnectedState(ConnectionManager connectionManager) : base(connectionManager)
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
        connectionManager.ConnectionMethod.SetupDisconnect();
        networkManager.Shutdown();
    }

    public override void OnUserRequestedShutdown()
    {
        connectionManager.OnConnectStatus?.Invoke(EConnectStatus.GenericDisconnect);
        connectionManager.ChangeState(EConnectionState.Offline);
    }

    public override void HandleClientDisconnected(ulong clientId)
    {
        string disconnectReason = networkManager.DisconnectReason;

        Debug.Log($"{clientId} disconnected. reason:{disconnectReason}");

        connectionManager.ChangeState(EConnectionState.Offline);
    }
}
