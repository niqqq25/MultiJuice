using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class ServerConnection
{
    public int NetworkId { get; private set; }
    public int PlayerId { get; set; }
    NetPeer peer;
    NetDataWriter dataWriter = new NetDataWriter();
    CompressionManager compressionManager;

    long lastReceivedPacketTime = -1;
    int lastReceivedTick = -1;
    public int ClientAckedTick { get; private set; } = -1;

    public ServerConnection(NetPeer peer)
    {
        this.peer = peer;
        NetworkId = peer.Id;

        compressionManager = new CompressionManager();
    }

    public void SendClientAcceptedPacket()
    {
        dataWriter.Reset();
        dataWriter.Put((byte)MessageTypes.ClientAccepted);
        dataWriter.Put(PlayerId);
        peer.Send(dataWriter, DeliveryMethod.ReliableUnordered);
    }

    public void SendDisconnectPacket()
    {
        peer.Disconnect();
    }

    public void SendSnapshot(Snapshot snapshot)
    {
        byte[] data = compressionManager.EncodeSnapshot(snapshot, lastReceivedTick, ClientAckedTick);

        peer.Send(data, DeliveryMethod.ReliableUnordered);
    }

    public bool IsTimedOut()
    {
        long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (lastReceivedPacketTime != -1)
        {
            if (currentTime - lastReceivedPacketTime >= GameConfig.clientTimeout)
            {
                return true;
            }
        }
        return false;
    }

    public UserCommand[] ReadUserCommands(NetDataReader reader, out int currentActiveTick)
    {
        lastReceivedPacketTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        int clientTick = reader.GetInt();
        ClientAckedTick = reader.GetInt();
        int activeTickMask = reader.GetByte();
        currentActiveTick = ClientAckedTick - activeTickMask; // active tick, at the moment user command was generated

        if (lastReceivedTick >= clientTick) return null;

        int tickGap = 1; // for first command tick gap is 1
        if (lastReceivedTick != -1)
        {
            tickGap = clientTick - lastReceivedTick;
        }
        lastReceivedTick = clientTick;

        int commandCount = reader.GetByte();
        int startIndexToProcede = commandCount - tickGap;
        UserCommand userCommand = new UserCommand();
        UserCommand[] userCommands = new UserCommand[tickGap];

        int index = 0;
        for (int i = 0; i < commandCount; i++)
        {
            userCommand.Deserialize(reader);
            if (i >= startIndexToProcede)
            {
                userCommands[index] = userCommand;
                index++;
            }
        }

        return userCommands;
    }
}