using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientWorld
{
    public int PlayerId = -1;
    float localTickInterval;
    GameObject clientPrefab;
    public Dictionary<int, ClientPlayer> Players { get; set; } = new Dictionary<int, ClientPlayer>();
    public LocalPlayer LocalPlayer;
    Queue<UserCommand> localPlayerCommands = new Queue<UserCommand>();
    public int LastServerAckedTick { get; set; } = -1;

    InterpolationManager interpolationManager;
    ExtrapolationManager extrapolationManager;

    public ClientWorld()
    {
        localTickInterval = Client.instance.TickInterval;
        clientPrefab = Resources.Load<GameObject>("ClientPlayer");

        interpolationManager = new InterpolationManager(this);
        extrapolationManager = new ExtrapolationManager(this);
    }

    public UserCommand[] GetLocalPlayerCommands(int lastServerAckedTick)
    {
        var command = LocalPlayer.GetUserCommand();
        localPlayerCommands.Enqueue(command);

        if (lastServerAckedTick != -1)
        {
            int commandsToRemove = localPlayerCommands.Count - (Client.instance.LocalTick - lastServerAckedTick);
            localPlayerCommands.DequeueChunk(commandsToRemove);
        }

        return localPlayerCommands.ToArray();
    }

    public void ApplyUserCommand(UserCommand userCommand)
    {
        LocalPlayer.ApplyMovement(userCommand.Buttons, localTickInterval);
    }

    public void Update()
    {
        if(GameConfig.extrapolation)
        {
            extrapolationManager.Update();
        }
        else if (GameConfig.entityInterpolation)
        {
            interpolationManager.Update();
        }
    }

    public void HandleNewSnapshot(Snapshot snapshot)
    {
        // get local player
        foreach (var playerState in snapshot.PlayerStates)
        {
            if (playerState.PlayerId != PlayerId) continue;

            UpdateLocalPlayer(playerState);
            break;
        }

        if(GameConfig.extrapolation)
        {
            extrapolationManager.AddSnapshot(snapshot);
        }
        else if (GameConfig.entityInterpolation)
        {
            interpolationManager.AddSnapshot(snapshot);
        }
        else
        {
            UpdateRemotePlayers(snapshot);
        }
    }

    public void UpdateRemotePlayers(Snapshot snapshot)
    {
        HashSet<int> playersToRemove = new HashSet<int>(Players.Keys);

        foreach (var playerState in snapshot.PlayerStates)
        {
            playersToRemove.Remove(playerState.PlayerId);

            if (playerState.PlayerId != PlayerId)
            {
                UpdateRemotePlayer(playerState);
            }
        }

        // remove players
        foreach (var playerId in playersToRemove)
        {
            RemoveClientPlayer(playerId);
        }
    }

    public void UpdateRemotePlayer(PlayerState playerState)
    {
        if (Players.TryGetValue(playerState.PlayerId, out ClientPlayer player))
        {
            player.ApplyPosition(playerState.Position);
            player.ApplyRotation(playerState.Rotation);
        }
        else
        {
            CreateClientPlayer(playerState);
        }
    }

    public void UpdateLocalPlayer(PlayerState playerState)
    {
        if (LocalPlayer == null)
        {
            CreateLocalPlayer(playerState.Position);
        }
        else
        {
            if (GameConfig.userReconsilation)
            {
                LocalPlayer.ValidatePosition(playerState.Position, LastServerAckedTick);
            }
            else
            {
                LocalPlayer.ApplyPosition(playerState.Position);
            }
        }
    }

    void CreateLocalPlayer(Vector3 position)
    {
        var prefab = Resources.Load<GameObject>("LocalPlayer");
        var player = UnityEngine.Object.Instantiate<GameObject>(prefab, position, Quaternion.identity);
        LocalPlayer = player.GetComponent<LocalPlayer>();
    }

    public void CreateClientPlayer(PlayerState playerState)
    {
        var player = UnityEngine.Object.Instantiate<GameObject>(clientPrefab, playerState.Position, playerState.Rotation);
        ClientPlayer clientPlayer = player.GetComponent<ClientPlayer>();

        Players.Add(playerState.PlayerId, clientPlayer);
    }

    public void RemoveClientPlayer(int playerId)
    {
        if(Players.TryGetValue(playerId, out ClientPlayer clientPlayer))
        {
            clientPlayer.Destroy();
            Players.Remove(playerId);
        }
    }

    public int GetActiveServerTick()
    {
        if(GameConfig.entityInterpolation)
        {
            return interpolationManager.CurrentServerTick - 1; // cuz tick-1 --> tick // TODO make so it is simpply tick
        } else if(GameConfig.extrapolation)
        {
            return extrapolationManager.CurrentTick;
        } else
        {
            return -1;
        }
    }
}
