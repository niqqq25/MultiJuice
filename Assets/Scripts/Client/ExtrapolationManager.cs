using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtrapolationManager
{
    const float maxPositionErrorTolerance = 0.04f;
    float extrapolationDelay = 0.05f;
    bool shouldStart = false;
    bool extrapolationStarted = false;
    float startTime = 0;

    public int CurrentTick { get; private set; } = -1;
    float tickTime = 0;
    float serverTickInterval = 1f / GameConfig.serverTickRate;
    float localTickInterval = Time.fixedDeltaTime;
    float simulationStep;

    public static ExtrapolationManager instance;
    ClientWorld clientWorld;
    Dictionary<int, Snapshot> snapshots = new Dictionary<int, Snapshot>();

    public ExtrapolationManager(ClientWorld clientWorld)
    {
        instance = this;
        this.clientWorld = clientWorld;
        simulationStep = localTickInterval / serverTickInterval;
    }

    void OnStart()
    {
        if (!shouldStart || extrapolationStarted) return;

        if (startTime >= extrapolationDelay)
        {
            extrapolationStarted = true;
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
        if (!extrapolationStarted) return;

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

        if (CurrentTick == -1)
        {
            shouldStart = true;
            CurrentTick = snapshotTick - 1;
            snapshots.Add(snapshotTick, snapshot);
            return;
        }

        if ((snapshotTick <= CurrentTick) || (snapshots.ContainsKey(snapshotTick))) return; // too old or duplicate

        snapshots.Add(snapshotTick, snapshot);
    }

    void NextTick()
    {
        CurrentTick++;

        if (!snapshots.ContainsKey(CurrentTick))
        {
            Debug.Log("Missing packet");
            return;
        }

        HashSet<int> playersToRemove = new HashSet<int>(clientWorld.Players.Keys);

        foreach (var playerState in snapshots[CurrentTick].PlayerStates)
        {
            playersToRemove.Remove(playerState.PlayerId);

            if (playerState.PlayerId != clientWorld.PlayerId)
            {
                if (clientWorld.Players.ContainsKey(playerState.PlayerId))
                {
                    ApplyExtrapolation(playerState);
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

        snapshots.Remove(CurrentTick);
    }

    void ApplyExtrapolation(PlayerState playerState)
    {
        var player = clientWorld.Players[playerState.PlayerId];

        Vector3 testErrorPosition = playerState.Position - player.GetPosition();

        if (Vector3.SqrMagnitude(testErrorPosition) < maxPositionErrorTolerance)
        {
            player.MovementVelocity = (playerState.Position - player.PrvsPosition) + testErrorPosition;
            player.PrvsPosition = playerState.Position;
        }
        else
        {
            player.MovementVelocity = playerState.Position - player.PrvsPosition;
            player.ApplyPosition(playerState.Position);
        }

        player.RotationVelocity = Utils.EulerAnglesBetween(player.PrvsRotation, playerState.Rotation);
        player.ApplyRotation(playerState.Rotation);
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
        CurrentTick = -1;
        tickTime = 0;
        startTime = 0;
        shouldStart = false;
        extrapolationStarted = false;
        snapshots.Clear();

        foreach (var player in clientWorld.Players.Values)
        {
            player.Reset();
        }
    }
}