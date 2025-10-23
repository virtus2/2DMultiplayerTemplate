using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
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
    private ulong targetSteamId = 0;

    private const string kRichPresense_SteamDisplay = "steam_display";

    public ConnectionMethodSteam(ConnectionManager connectionManager, int maxConnectedPlayers, FacepunchTransport transport)
        : base(connectionManager, maxConnectedPlayers)
    {
        networkManager = NetworkManager.Singleton;
        facepunchTransport = transport;

        playerSteamId = SteamClient.SteamId;
        playerSteamName = SteamClient.Name;

        connectedToSteam = true;
        Debug.Log($"Steamworks initialized: playerSteamId({playerSteamId}), playerSteamName({playerSteamName})");
        // SteamFriends.SetRichPresence("steam_display", "In MainMenu");

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
        networkManager.Shutdown();
        DisableSteamworks();
    }

    public override void SetupClientConnection()
    {
        SetConnectionPayload(playerSteamId.ToString(), playerSteamName);
        if (targetSteamId != 0)
        {
            Debug.Log($"SetupClientConnection: targetSteamId is ({targetSteamId}) - by user");
            facepunchTransport.targetSteamId = targetSteamId;
        }
        else
        {
            if (currentLobby.HasValue)
            {
                Debug.Log($"SetupClientConnection: targetSteamId is ({currentLobby.Value.Owner.Id}) - by lobby join");
                facepunchTransport.targetSteamId = currentLobby.Value.Owner.Id;
            }
            else
            {
                Debug.Log($"lobby is null");
            }
        }
    }

    public void SetLobby(SteamId steamId)
    {
        targetSteamId = steamId;
    }

    public override void SetupDisconnect()
    {
        targetSteamId = 0;
        LeaveLobby();
    }

    public override Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    {
        // Nothing to do here yet.
        return Task.FromResult((true, true));
    }

    public override void SetupHostConnection()
    {
        SetConnectionPayload(playerSteamId.ToString(), playerSteamName);
    }

    public override void HandleHostStartedSuccessfully()
    {
        CreateLobby();
    }

    public override void HandleHostStartFailed()
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
        // SteamFriends.SetRichPresence("steam_display", "In Lobby");

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
        // Called when the user tries to join a lobby from their friends list. game client should attempt to connect to specified lobby when this is received
        Debug.Log($"HandleGameLobbyJoinRequested: lobby({lobby}), SteamId({steamId})");
        bool isOwner = lobby.Owner.Id.Equals(steamId);

        RoomEnter joinResult = await lobby.Join();
        if (joinResult != RoomEnter.Success)
        {
            Debug.Log($"Failed to join lobby: lobbyId({lobby.Id}), steamId({steamId})");
            return;
        }

        currentLobby = lobby;
        SteamId lobbdyOwnerId = lobby.Owner.Id;
        connectionManager.StartClient();
    }

    private void HandleLobbyEntered(Lobby lobby)
    {
        Debug.Log($"HandleLobbyEntered: lobby({lobby})");
        if (networkManager.IsHost)
            return;

        currentLobby = lobby;

        Debug.Log($"Entered Lobby ({lobby.Id})");
    }

    private void LeaveLobby()
    {
        if (currentLobby.HasValue)
        {
            Debug.Log($"Leave Lobby ({currentLobby.Value.Id})");
        }
        currentLobby?.Leave();
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
