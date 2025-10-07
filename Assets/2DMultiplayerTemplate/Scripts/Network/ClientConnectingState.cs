using UnityEngine;

public class ClientConnectingState : ConnectionState
{
    public ClientConnectingState(ConnectionManager connectionManager) : base(connectionManager)
    {
    }

    public override void Enter()
    {
        Client();
    }

    public override void Exit()
    {
    }

    public override void Disconnect()
    {
        connectionManager.ChangeState(EConnectionState.Offline);
    }

    public override void HandleClientConnected(ulong clientId)
    {
        connectionManager.ChangeState(EConnectionState.ClientConnected);
    }

    public override void HandleClientDisconnected(ulong clientId)
    {
        ClientFailed();
    }

    private void Client()
    {
        try
        {
            connectionManager.ConnectionMethod.SetupClientConnection();
            bool success = networkManager.StartClient();
            if (success)
            {
            }
            else
            {
                ClientFailed();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            ClientFailed();
        }
    }

    private void ClientFailed()
    {
        string disconnectReason = networkManager.DisconnectReason;

        connectionManager.ChangeState(EConnectionState.Offline);
    }
}
