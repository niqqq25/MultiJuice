using System.Net;
using UnityEngine;

public class Client
{
    public float TickInterval = 1f / GameConfig.clientTickRate;
    public int LocalTick { get; private set; } = 0;

    public static Client instance;

    ClientWorld clientWorld;
    NetworkClient networkClient;
    public StateMachine<ClientState> StateMachine { get; private set; }
    string disconnectReason = "";

    public Client()
    {
        instance = this;
        clientWorld = new ClientWorld();
        networkClient = new NetworkClient(clientWorld);

        StateMachine = new StateMachine<ClientState>();
        StateMachine.Add(ClientState.Browsing, EnterBrowsingState, null, null);
        StateMachine.Add(ClientState.Connecting, EnterConnectingState, null, null);
        StateMachine.Add(ClientState.Playing, EnterPlayingState, UpdatePlayingState, null);
    }

    public void Start()
    {
        StateMachine.SwitchTo(ClientState.Browsing);
    }

    void EnterBrowsingState()
    {
        ConnectModal.instance.ChangeToBrowsingState(disconnectReason);
    }

    void EnterConnectingState()
    {
        ConnectModal.instance.ChangeToConnectingState();
    }

    void EnterPlayingState()
    {
        ConnectModal.instance.Close();
    }

    void UpdatePlayingState()
    {
        clientWorld.Update();
        networkClient.SendUserCommands();
        LocalTick++;
    }

    public void FixedUpdate()
    {
        networkClient.Update();
        StateMachine.Update();
    }

    public void Connect(string address, int port)
    {
        var _address = IPAddress.Parse(address);
        networkClient.Connect(new IPEndPoint(_address, port));

        StateMachine.SwitchTo(ClientState.Connecting);
    }

    public void OnDisconnect(string reason)
    {
        disconnectReason = reason;
        StateMachine.SwitchTo(ClientState.Browsing);
    }
}
