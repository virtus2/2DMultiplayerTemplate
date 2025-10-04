using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Simple IP connection setup with UTP
/// </summary>
public class ConnectionMethodUnityTransport : ConnectionMethod
{
    private string ipAddress;
    private ushort port;

    public ConnectionMethodUnityTransport(ConnectionManager connectionManager, int maxConnectedPlayers, string ipAddress, ushort port)
        : base(connectionManager, maxConnectedPlayers)
    {
        this.ipAddress = ipAddress;
        this.port = port;
    }

    public override void HandleApplicationQuit()
    {
        // Do nothing
    }

    public override void SetupClientConnection()
    {
        SetConnectionPayload(GetPlayerId(), playerName);
        var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        utp.SetConnectionData(ipAddress, port);
    }

    public override void SetupDisconnect()
    {
        
    }

    public override void OnHostStartedSuccessfully()
    {
        Debug.Log($"Unity Transport Host Success");
    }

    public override void OnHostStartFailed()
    {
        Debug.Log($"Unity Transport Host Failed");
    }

    public override Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    {
        // Nothing to do here
        return Task.FromResult((true, true));
    }

    public override void SetupHostConnection()
    {
        SetConnectionPayload(GetPlayerId(), playerName); // Need to set connection payload for host as well, as host is a client too
        var utp = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        utp.SetConnectionData(ipAddress, port);
    }

    protected override string GetPlayerId()
    {
        return ClientPrefs.GetGuid();
    }

    public void SetIpAddress(string ipAddress)
    {
        this.ipAddress = ipAddress;
    }

    public void SetPort(ushort port)
    {
        this.port = port;
    }
}
