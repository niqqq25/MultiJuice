using System;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class ServerWorld
{
    const int snapshotOffset = 10;
    GameObject playerPrefab;
    Dictionary<int, ServerPlayer> players = new Dictionary<int, ServerPlayer>();
    float clientTickInterval = 1f / GameConfig.clientTickRate;
    int clientServerTickRatio = GameConfig.clientTickRate / GameConfig.serverTickRate;
    Dictionary<int, Snapshot> snapshots = new Dictionary<int, Snapshot>();
    PlayerId playerId;
    Bounds worldBounds;

    bool fakePlayers = false;
    HashSet<int> fakePlayerIds = new HashSet<int>();
    HashSet<ServerPlayer> movingFakePlayers = new HashSet<ServerPlayer>();

    public ServerWorld()
    {
        playerId = new PlayerId();
        playerPrefab = Resources.Load<GameObject>("ServerPlayer");

        worldBounds = Camera.main.OrthographicBounds();
        var worldBoundExtents = worldBounds.extents;
        worldBoundExtents.z = float.PositiveInfinity;
        worldBounds.extents = worldBoundExtents;
    }

    public void Update()
    {
        if(fakePlayers)
        {
            foreach (var fakePlayer in movingFakePlayers)
            {
                fakePlayer.ApplyFakeMovement(clientTickInterval);
                fakePlayer.ApplyFakeRotation(5);
                if(!fakePlayer.InBounds(worldBounds))
                {
                    fakePlayer.Velocity *= -1;
                }
            }
        }
    }

    public void ApplyUserCommands(int playerId, UserCommand[] userCommands, int currentActiveTick) // TODO fix this
    {
        if (players.TryGetValue(playerId, out ServerPlayer serverPlayer))
        {
            for (int i = 0; i < userCommands.Length; i++)
            {
                UserCommand userCommand = userCommands[i];

                if (userCommand.Buttons.Fire)
                {
                    if (GameConfig.lagCompensation)
                    {
                        int tick = GetCommandTick(currentActiveTick, userCommands.Length, i);
                        serverPlayer.CheckForHit(snapshots[tick], Server.instance.LocalTick - tick);
                    }
                    else
                    {
                        serverPlayer.CheckForHitSimple();
                    }
                }

                serverPlayer.ApplyMovement(userCommand.Buttons, clientTickInterval);
                serverPlayer.ApplyRotation(userCommand.Rotation);
            }
        }
    }

    public Snapshot GenerateWorldSnapshot()
    {
        PlayerState[] playerStates = new PlayerState[players.Count];

        int index = 0;
        foreach (var player in players.Values)
        {
            playerStates[index] = player.ToPlayerState();
            index++;
        }

        var snapshot = new Snapshot() {
            Tick = Server.instance.LocalTick,
            PlayerStates = playerStates
        };

        snapshots.Add(snapshot.Tick, snapshot);

        return snapshot;
    }

    public int CreatePlayer(Vector3 position, bool fakePlayer = false)
    {
        int _playerId = playerId.GetNext();
        var player = UnityEngine.Object.Instantiate<GameObject>(playerPrefab, position, Quaternion.identity);
        ServerPlayer serverPlayer = player.GetComponent<ServerPlayer>();

        if(!GameConfig.showServerPlayers)
        {
            Utils.ChangeObjectOpacity(serverPlayer.transform, 0);
        }

        serverPlayer.Initialize(fakePlayer);
        serverPlayer.name = _playerId.ToString();
        serverPlayer.PlayerId = _playerId;

        players.Add(_playerId, serverPlayer);

        return _playerId;
    }

    public void RemovePlayer(int playerId)
    {
        if(players.TryGetValue(playerId, out ServerPlayer serverPlayer))
        {
            serverPlayer.Destroy();
        }
        players.Remove(playerId);
    }

    public void AddFakePlayers(int playerCount, int movingPlayerCount)
    {
        fakePlayers = true;
        float x, y;

        int _movingPlayerCount = Math.Min(playerCount, movingPlayerCount);

        int playerId;
        for (int i = 0; i < _movingPlayerCount; i++)
        {
            x = UnityEngine.Random.Range(-worldBounds.extents.x, worldBounds.extents.x);
            y = UnityEngine.Random.Range(-worldBounds.extents.y, worldBounds.extents.y);

            playerId = CreatePlayer(new Vector3(x, y), true);
            fakePlayerIds.Add(playerId);

            players[playerId].Velocity = new Vector3(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-4f, 4f));
            movingFakePlayers.Add(players[playerId]);
        }

        int frozenPlayerCount = playerCount - _movingPlayerCount;
        for (int i = 0; i < frozenPlayerCount; i++)
        {
            x = UnityEngine.Random.Range(-worldBounds.extents.x, worldBounds.extents.x);
            y = UnityEngine.Random.Range(-worldBounds.extents.y, worldBounds.extents.y);

            playerId = CreatePlayer(new Vector3(x, y), true);
            fakePlayerIds.Add(playerId);
        }
    }

    public void RemoveFakePlayers()
    {
        foreach (var fakePlayer in fakePlayerIds)
        {
            RemovePlayer(fakePlayer);
        }

        fakePlayers = false;
        fakePlayerIds.Clear();
        movingFakePlayers.Clear();
    }

    public void ClearSnapshots(int oldestAckedSnapshot)
    {
        snapshots.RemoveLowerThanAnd(oldestAckedSnapshot - snapshotOffset);
    }

    int GetCommandTick(int latestTick, int commandCount, int commandIndex)
    {
        return latestTick - (int)Mathf.Floor((commandCount - commandIndex) / clientServerTickRatio);
    }
}
