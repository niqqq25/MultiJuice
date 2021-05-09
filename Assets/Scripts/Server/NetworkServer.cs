using System;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

public class NetworkServer
{
    NetManager netManager;
    Dictionary<int, ServerConnection> connections = new Dictionary<int, ServerConnection>();
    ServerWorld serverWorld;

    HashSet<int> connectionsToRemove = new HashSet<int>();

    public NetworkServer(ServerWorld serverWorld)
    {
        var netListener = new EventBasedNetListener();
        netListener.ConnectionRequestEvent += ConnectionRequestEvent;
        netListener.PeerConnectedEvent += PeerConnectedEvent;
        netListener.NetworkReceiveEvent += OnNetworkReceive;

        netManager = new NetManager(netListener);
        netManager.Start(GameConfig.serverPort);

        this.serverWorld = serverWorld;

        // simulate extra latency
        netManager.SimulateLatency = true;
        netManager.SimulationMinLatency = 100;
        netManager.SimulationMaxLatency = 150;
    }

    public void Update()
    {
        netManager.PollEvents();
        CheckTimeOuts();
    }

    public void SendSnapshot()
    {
        Snapshot snapshot = serverWorld.GenerateWorldSnapshot();

        foreach (var connection in connections.Values)
        {
            connection.SendSnapshot(snapshot);
        }
    }

    void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        MessageTypes messageType = (MessageTypes)reader.GetByte();

        switch (messageType)
        {
            case MessageTypes.ClientCommand:
                OnClientCommand(peer, reader);
                break;
            default:
                break;
        }
    }

    void OnClientCommand(NetPeer peer, NetPacketReader reader)
    {
        UserCommand[] userCommands = connections[peer.Id].ReadUserCommands(reader, out int currentActiveTick);

        if(userCommands != null)
        {
            serverWorld.ApplyUserCommands(connections[peer.Id].PlayerId, userCommands, currentActiveTick);
        }
    }

    void PeerConnectedEvent(NetPeer peer)
    {
        int playerId = serverWorld.CreatePlayer(Vector3.zero);

        var clientConnection = new ServerConnection(peer)
        {
            PlayerId = playerId
        };
        connections.Add(peer.Id, clientConnection);

        clientConnection.SendClientAcceptedPacket();
    }

    void ConnectionRequestEvent(ConnectionRequest request)
    {
        if(connections.Count < GameConfig.maxClients)
        {
            request.Accept();
        }
        else
        {
            request.Reject();
        }
    }

    void CheckTimeOuts()
    {
        connectionsToRemove.Clear();
        foreach (var connection in connections.Values)
        {
            if (connection.IsTimedOut())
            {
                connectionsToRemove.Add(connection.NetworkId);
            }
        }

        foreach (var connectionId in connectionsToRemove)
        {
            DisconnectClient(connections[connectionId]);
        }
    }

    public int GetOldestAckedSnapshot()
    {
        if (connections.Count == 0) return 0;

        float oldestTick = Mathf.Infinity;

        foreach (var connection in connections.Values)
        {
            if(connection.ClientAckedTick < oldestTick)
            {
                oldestTick = connection.ClientAckedTick;
            }
        }

        return (int)oldestTick;
    }

    void DisconnectClient(ServerConnection connection)
    {
        serverWorld.RemovePlayer(connection.PlayerId);
        connection.SendDisconnectPacket();
        connections.Remove(connection.NetworkId);
    }
}
