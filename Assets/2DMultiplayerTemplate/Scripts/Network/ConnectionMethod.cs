using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public abstract class ConnectionMethod
{
    protected ConnectionManager connectionManager;

    protected readonly string playerName;
    protected readonly int maxConnectedPlayers;

    public abstract void HandleApplicationQuit();

    /// <summary>
    /// Setup the host connection prior to starting the NetworkManager
    /// </summary>
    /// <returns></returns>
    public abstract void SetupHostConnection();

    public abstract void OnHostStartedSuccessfully();
    public abstract void OnHostStartFailed();

    /// <summary>
    /// Setup the client connection prior to starting the NetworkManager
    /// </summary>
    /// <returns></returns>
    public abstract void SetupClientConnection();

    public abstract void SetupDisconnect();

    /// <summary>
    /// Setup the client for reconnection prior to reconnecting
    /// </summary>
    /// <returns>
    /// success = true if succeeded in setting up reconnection, false if failed.
    /// shouldTryAgain = true if we should try again after failing, false if not.
    /// </returns>
    public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

    public ConnectionMethod(ConnectionManager connectionManager, int maxConnectedPlayers)
    {
        this.connectionManager = connectionManager;
        this.maxConnectedPlayers = maxConnectedPlayers;
    }

    protected void SetConnectionPayload(string playerId, string playerName)
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload
        {
            playerId = playerId,
            playerName = playerName,
            isDebug = Debug.isDebugBuild
        });

        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }

    protected abstract string GetPlayerId();
}