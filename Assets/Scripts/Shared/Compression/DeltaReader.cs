using System;
using LiteNetLib.Utils;
using UnityEngine;

public static class DeltaReader
{
    public static NetDataReader reader = new NetDataReader(); 

    public static Snapshot ReadDeltaSnapshot(Snapshot snapshot, Snapshot baseSnapshot)
    {
        PlayerState basePlayerState;
        PlayerState[] basePlayerStates = baseSnapshot.PlayerStates;
        PlayerState[] playerStateDeltas = new PlayerState[snapshot.PlayerStates.Length];

        for (int i = 0; i < playerStateDeltas.Length; i++)
        {
            var playerState = snapshot.PlayerStates[i];

            try
            {
                basePlayerState = Array.Find(basePlayerStates, e => e.PlayerId == playerState.PlayerId);
                playerStateDeltas[i] = new PlayerState()
                {
                    PlayerId = playerState.PlayerId,
                    Position = basePlayerState.Position + playerState.Position,
                    Rotation = ReadDeltaQuaternion(playerState.Rotation, basePlayerState.Rotation)
                };
            }
            catch
            {
                playerStateDeltas[i] = playerState;
            }
        }

        return new Snapshot()
        {
            Tick = snapshot.Tick,
            PlayerStates = playerStateDeltas
        };
    }

    public static Quaternion ReadDeltaQuaternion(Quaternion quaternion, Quaternion baseQuaternion)
    {
        return quaternion * baseQuaternion;
    }
}
