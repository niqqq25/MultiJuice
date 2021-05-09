using System;
using LiteNetLib.Utils;
using UnityEngine;

public struct PlayerState
{
    public int PlayerId;
    public Vector3 Position;
    public Quaternion Rotation;

    public void Serialize(INetBuffer writer)
    {
        writer.Put(PlayerId);
        writer.Put(Position, true);
        writer.Put(Rotation, true);
    }

    public void Deserialize(INetBuffer reader)
    {
        PlayerId = reader.GetInt();
        Position = reader.GetVector3(true);
        Rotation = reader.GetQuaternion(true);
    }
}

public struct Snapshot
{
    public int Tick;
    public PlayerState[] PlayerStates;

    public void Serialize(INetBuffer writer)
    {
        writer.Put(Tick);
        writer.Put(PlayerStates.Length);
        foreach (var playerState in PlayerStates)
        {
            playerState.Serialize(writer);
        }
    }

    public void Deserialize(INetBuffer reader)
    {
        Tick = reader.GetInt();
        int playerStateCount = reader.GetInt();
        PlayerStates = new PlayerState[playerStateCount];
        PlayerState playerState = new PlayerState();

        for (int i = 0; i < playerStateCount; i++)
        {
            playerState.Deserialize(reader);
            PlayerStates[i] = playerState;
        }
    }
}
