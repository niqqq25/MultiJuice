using System;
using System.Collections.Generic;
using UnityEngine;

public class InterpolationManager
{
    float interpolationDelay = 0.2f;
    bool shouldStart = false;
    bool interpolationStarted = false;
    float startTime = 0;

    float serverTickInterval = 1f / GameConfig.serverTickRate;
    float localTickInterval = Time.fixedDeltaTime;
    float simulationStep;
    float tickTime = 0;
    public int CurrentServerTick { get; private set; } = -1;

    public static InterpolationManager instance;
    ClientWorld clientWorld;
    Dictionary<int, Snapshot> snapshots = new Dictionary<int, Snapshot>();

    public InterpolationManager(ClientWorld clientWorld)
    {
        instance = this;
        this.clientWorld = clientWorld;
        simulationStep = localTickInterval / serverTickInterval;
    }

    void OnStart()
    {
        if (!shouldStart || interpolationStarted) return;

        if (startTime >= interpolationDelay)
        {
            interpolationStarted = true;
            NextTick();
        }
        else
        {
            startTime += Time.fixedDeltaTime;
        }
    }

    public void Update()
    {
        OnStart();
        if (!interpolationStarted) return;

        Simulate();

        if (tickTime >= serverTickInterval)
        {
            NextTick();
            tickTime = 0;
        }

        tickTime += localTickInterval;
    }


    public void AddSnapshot(Snapshot snapshot)
    {
        int snapshotTick = snapshot.Tick;

        if (CurrentServerTick == -1) // first tick, so we need to apply interpolation delay
        {
            CurrentServerTick = snapshotTick;
            shouldStart = true;
            ProcessFirstSnapshot(snapshot);
            return;
        }

        if ((snapshotTick < CurrentServerTick) || (snapshots.ContainsKey(snapshotTick))) return; // too old or duplicate

        snapshots.Add(snapshotTick, snapshot);
    }

    public void NextTick()
    {
        CurrentServerTick++;

        if (!snapshots.ContainsKey(CurrentServerTick))
        {
            Debug.Log("No next snapshot");
            return;
        }

        HashSet<int> playersToRemove = new HashSet<int>(clientWorld.Players.Keys);

        foreach (var playerState in snapshots[CurrentServerTick].PlayerStates)
        {
            playersToRemove.Remove(playerState.PlayerId);

            if (playerState.PlayerId != clientWorld.PlayerId)
            {
                if (clientWorld.Players.ContainsKey(playerState.PlayerId))
                {
                    ApplyInterpolation(playerState);
                }
                else
                {
                    clientWorld.CreateClientPlayer(playerState);
                }
            }
        }

        foreach (var playerId in playersToRemove)
        {
            clientWorld.RemoveClientPlayer(playerId);
        }

        snapshots.Remove(CurrentServerTick);
    }

    void ApplyInterpolation(PlayerState playerState)
    {
        var player = clientWorld.Players[playerState.PlayerId];
        player.MovementVelocity = playerState.Position - player.GetPosition();
        player.RotationVelocity = Utils.EulerAnglesBetween(player.GetRotation(), playerState.Rotation);
    }

    void ProcessFirstSnapshot(Snapshot snapshot)
    {
        foreach (var playerState in snapshot.PlayerStates)
        {
            if (playerState.PlayerId != clientWorld.PlayerId)
            {
                if (clientWorld.Players.ContainsKey(playerState.PlayerId))
                {
                    clientWorld.Players[playerState.PlayerId].ApplyPosition(playerState.Position);
                }
                else
                {
                    clientWorld.CreateClientPlayer(playerState);
                }
            }
        }
    }

    void Simulate()
    {
        foreach (var player in clientWorld.Players.Values)
        {
            player.Simulate(simulationStep);
        }
    }

    public void Reset()
    {
        shouldStart = false;
        interpolationStarted = false;
        startTime = 0;
        tickTime = 0;
        CurrentServerTick = -1;
        snapshots.Clear();
    }
}
