using System;
using System.Linq;
using System.Net;
using LiteNetLib;
using UnityEngine;

public class NetworkClient
{
    NetManager netManager;
    ClientConnection connection;
    ClientWorld clientWorld;
    int lastServerAckedTick = -1;

    public NetworkClient(ClientWorld clientWorld)
    {
        this.clientWorld = clientWorld;

        var netListener = new EventBasedNetListener();
        netListener.PeerConnectedEvent += PeerConnectedEvent;
        netListener.PeerDisconnectedEvent += OnPeerDisconnected;
        netListener.NetworkReceiveEvent += OnNetworkReceive;

        netManager = new NetManager(netListener);
        netManager.Start(6667);

        // simulate latency
        netManager.SimulateLatency = true;
        netManager.SimulationMinLatency = 100;
        netManager.SimulationMaxLatency = 150;
    }

    public void Update()
    {
        netManager.PollEvents();
        if(connection != null && connection.IsTimedOut())
        { 
            Client.instance.OnDisconnect("Server timed out");
        }
    }

    public void Connect(IPEndPoint endPoint)
    {
        netManager.Connect(endPoint, "");
    }

    public void SendUserCommands()
    {
        if (clientWorld.LocalPlayer == null) return;

        UserCommand[] userCommands = clientWorld.GetLocalPlayerCommands(lastServerAckedTick);
        int activeServerTick = clientWorld.GetActiveServerTick();
        if(activeServerTick < 0)
        {
            activeServerTick = connection.LastAckedSnapshot;
        }

        if (GameConfig.userReconsilation)
        {
            clientWorld.ApplyUserCommand(userCommands.Last());
        }

        connection.SendUserCommands(userCommands, activeServerTick);
    }

    void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        MessageTypes messageType = (MessageTypes)reader.GetByte();

        switch (messageType)
        {
            case MessageTypes.ClientAccepted:
                OnClientAccepted(peer, reader);
                break;
            case MessageTypes.Snapshot:
                OnSnapshot(reader);
                break;
            default:
                break;
        }
    }

    void OnSnapshot(NetPacketReader packetReader)
    {
        if (connection == null) return;

        StatisticsManager.instance.ReceivedBytes += packetReader.RawDataSize;

        var snapshot = connection.ReadSnapshot(packetReader, out int ackedTick);
        lastServerAckedTick = ackedTick;
        clientWorld.LastServerAckedTick = lastServerAckedTick;

        clientWorld.HandleNewSnapshot(snapshot);
    }

    void OnClientAccepted(NetPeer peer, NetPacketReader reader)
    {
        clientWorld.PlayerId = reader.GetInt();
        connection = new ClientConnection(peer);

        Client.instance.StateMachine.SwitchTo(ClientState.Playing);
    }

    void PeerConnectedEvent(NetPeer peer)
    {
        Debug.Log("Client: Connected");
    }

    void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("Client: Disconnected");
        Client.instance.OnDisconnect(disconnectInfo.Reason.ToString());
    }
}
