using System;
using UnityEngine;

public static class DeltaWriter
{
    public static Snapshot WriteDeltaSnapshot(Snapshot newSnapshot, Snapshot baseSnapshot)
    {
        PlayerState basePlayerState;
        PlayerState[] basePlayerStates = baseSnapshot.PlayerStates;
        PlayerState[] playerStateDeltas = new PlayerState[newSnapshot.PlayerStates.Length];

        for (int i = 0; i < playerStateDeltas.Length; i++)
        {
            var playerState = newSnapshot.PlayerStates[i];
            try
            {
                basePlayerState = Array.Find(basePlayerStates, e => e.PlayerId == playerState.PlayerId);
                playerStateDeltas[i] = new PlayerState()
                {
                    PlayerId = playerState.PlayerId,
                    Position = playerState.Position - basePlayerState.Position,
                    Rotation = WriteDeltaQuaternion(playerState.Rotation, basePlayerState.Rotation)
                };
            }
            catch
            {
                playerStateDeltas[i] = playerState;
            }
        }

        return new Snapshot()
        {
            Tick = newSnapshot.Tick,
            PlayerStates = playerStateDeltas
        };
    }

    public static Quaternion WriteDeltaQuaternion(Quaternion quaternion, Quaternion baseQuaternion)
    {
        return quaternion * Quaternion.Inverse(baseQuaternion);
    }
}
