using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ConnectionMethodSteam : ConnectionMethod
{
    private NetworkManager networkManager;
    private SteamId playerSteamId;
    private string playerSteamName;
    private bool connectedToSteam = false;
    private FacepunchTransport facepunchTransport;
    private Lobby? currentLobby;

    public ConnectionMethodSteam(ConnectionManager connectionManager, int maxConnectedPlayers, FacepunchTransport transport)
        : base(connectionManager, maxConnectedPlayers)
    {
        networkManager = NetworkManager.Singleton;
        facepunchTransport = transport;

        playerSteamId = SteamClient.SteamId;
        playerSteamName = SteamClient.Name;

        connectedToSteam = true;
        Debug.Log($"Steamworks initialized: playerSteamId({playerSteamId}), playerSteamName({playerSteamName})");

        SteamMatchmaking.OnLobbyCreated += HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += HandleLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += HandleLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected += HandleLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyInvite += HandleLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += HandleGameLobbyJoinRequested;
    }

    private void DisableSteamworks()
    {
        SteamMatchmaking.OnLobbyCreated -= HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= HandleLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= HandleLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected -= HandleLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyInvite -= HandleLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested -= HandleGameLobbyJoinRequested;
        SteamClient.Shutdown();
    }

    public override void HandleApplicationQuit()
    {
        LeaveLobby();
        DisableSteamworks();
    }

    public override void SetupClientConnection()
    {
        if (currentLobby.HasValue)
        {
            facepunchTransport.targetSteamId = currentLobby.Value.Owner.Id;
        }
        else
        {
            Debug.Log($"lobby is null");
        }
    }

    public override void SetupDisconnect()
    {
        currentLobby?.Leave();
    }

    public override Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    {
        // Nothing to do here yet.
        return Task.FromResult((true, true));
    }

    public override void SetupHostConnection()
    {
    }

    public override void OnHostStartedSuccessfully()
    {
        CreateLobby();
    }

    public override void OnHostStartFailed()
    {
        Debug.Log("Host start failed");
    }

    protected override string GetPlayerId()
    {
        return playerSteamId.ToString();
    }

    private async void CreateLobby()
    {
        Task task = SteamMatchmaking.CreateLobbyAsync(maxConnectedPlayers);
        await task;
    }

    private void HandleLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError($"Lobby couldn't be created!, {result}");
            return;
        }

        currentLobby = lobby;
        currentLobby.Value.SetFriendsOnly();
        // currentLobby.SetData("name", "Random Cool Lobby");
        currentLobby.Value.SetJoinable(true);

        SteamFriends.SetRichPresence("connect", "test");

        Debug.Log($"Lobby Created! lobbyId({lobby.Id})");
    }


    private void HandleLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        Debug.Log($"OnLobbyMemberDisconnected: lobby({lobby}), friend({friend})");
    }

    private void HandleLobbyInvite(Friend friend, Lobby lobby)
    {
        // Called when user(friend) invites local client to lobby
        Debug.Log($"OnLobbyInvite: lobby({lobby}), friend({friend})");
    }

    private void HandleLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        // Called when user(friend) leaves the current lobby
        Debug.Log($"OnLobbyMemberLeave: lobby({lobby}), friend({friend})");
    }

    private void HandleLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"OnLobbyMemberJoined: lobby({lobby}), friend({friend})");
    }

    private async void HandleGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        Debug.Log($"HandleGameLobbyJoinRequested: lobby({lobby}), SteamId({steamId})");
        bool isOwner = lobby.Owner.Id.Equals(steamId);

        RoomEnter joinResult = await lobby.Join();
        if (joinResult != RoomEnter.Success)
        {
            Debug.Log($"Failed to join lobby: lobbyId({lobby.Id}), steamId({steamId})");
            return;
        }

        SteamId lobbdyOwnerId = lobby.Owner.Id;
    }

    private void HandleLobbyEntered(Lobby lobby)
    {
        Debug.Log($"HandleLobbyEntered: lobby({lobby})");
        if (networkManager.IsHost)
            return;

        currentLobby = lobby;

        Debug.Log($"Entered Lobby ({lobby.Id})");
        connectionManager.StartClient();
    }

    private void LeaveLobby()
    {
        currentLobby?.Leave();

        networkManager.Shutdown();
        currentLobby = null;
    }

    public void ShowSteamFriendOverlay()
    {
        SteamFriends.OpenOverlay("friends");
    }

    public void OpenFriendOverlayForGameInvite()
    {
        SteamFriends.OpenGameInviteOverlay(currentLobby.Value.Id);
    }
}
