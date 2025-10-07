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
    }

    public override void Disconnect()
    {
        connectionManager.ChangeState(EConnectionState.Offline);
    }

    public override void HandleClientDisconnected(ulong clientId)
    {
        string disconnectReason = networkManager.DisconnectReason;

        Debug.Log(disconnectReason);

        connectionManager.ChangeState(EConnectionState.Offline);
    }
}
