using Unity.Netcode;

public abstract class ConnectionState
{
    protected ConnectionManager connectionManager;
    protected NetworkManager networkManager;

    public ConnectionState(ConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
        networkManager = NetworkManager.Singleton;
    }

    public abstract void Enter();
    public abstract void Exit();

    public virtual void StartServer() { }
    public virtual void StartHost() { }
    public virtual void StartClient() { }

    public virtual void OnClientConnected(ulong clientId) { }
    public virtual void OnClientDisconnect(ulong clientId) { }

    public virtual void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }
    public virtual void HandleServerStarted() { }
    public virtual void HandleServerStopped(bool isHost) { }
    public virtual void HandleClientConnected(ulong clientId) { }
    public virtual void HandleClientDisconnected(ulong clientId) { }

    public virtual void OnUserRequestedShutdown() { }
    public virtual void OnTransportFailure() { }
}