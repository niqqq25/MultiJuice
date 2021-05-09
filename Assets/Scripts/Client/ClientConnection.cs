using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class ClientConnection
{
    NetPeer peer;
    NetDataWriter dataWriter = new NetDataWriter();
    public int LastAckedSnapshot { get; private set; } = -1;
    CompressionManager compressionManager;
    long lastReceivedPacketTime = -1;

    public ClientConnection(NetPeer peer)
    {
        this.peer = peer;
        compressionManager = new CompressionManager();
    }

    public void SendUserCommands(UserCommand[] userCommands, int activeServerTick)
    {
        dataWriter.Reset();
        dataWriter.Put((byte)MessageTypes.ClientCommand);
        dataWriter.Put(Client.instance.LocalTick);
        dataWriter.Put(LastAckedSnapshot);
        dataWriter.Put((byte)(LastAckedSnapshot - activeServerTick)); // mask

        dataWriter.Put((byte)userCommands.Length);
        foreach (var userCommand in userCommands)
        {
            userCommand.Serialize(dataWriter);
        }

        peer.Send(dataWriter, DeliveryMethod.Unreliable);
    }

    public bool IsTimedOut()
    {
        long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (lastReceivedPacketTime != -1)
        {
            if (currentTime - lastReceivedPacketTime >= GameConfig.serverTimeout)
            {
                return true;
            }
        }
        return false;
    }

    public Snapshot ReadSnapshot(NetDataReader reader, out int ackedTick)
    {
        lastReceivedPacketTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        var snapshot = compressionManager.DecodeSnapshot(reader.GetRemainingBytes(), out ackedTick);

        if (LastAckedSnapshot < snapshot.Tick)
        {
            LastAckedSnapshot = snapshot.Tick;
        }

        return snapshot;
    }
}
