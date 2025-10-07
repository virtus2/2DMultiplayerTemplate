public class OfflineState : ConnectionState
{
    public OfflineState(ConnectionManager connectionManager) : base(connectionManager) { }

    public override void Enter()
    {
    }

    public override void Exit() { }

    public override void StartServer()
    {
        connectionManager.ChangeState(EConnectionState.StartingServer);
    }

    public override void StartHost()
    {
        connectionManager.ChangeState(EConnectionState.StartingHost);
    }

    public override void StartClient()
    {
        connectionManager.ChangeState(EConnectionState.ClientConnecting);
    }
}
