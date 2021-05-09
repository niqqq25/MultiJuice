
using UnityEngine;

public class Server
{
    public static Server instance;

    float tickInterval = 1f / GameConfig.serverTickRate;
    float tickTime = 0;
    public int LocalTick { get; private set; } = 0;

    ServerWorld serverWorld;
    NetworkServer networkServer;

    public Server()
    {
        instance = this;

        serverWorld = new ServerWorld();
        networkServer = new NetworkServer(serverWorld);
    }

    public void Start()
    {
    }

    public void FixedUpdate()
    {
        networkServer.Update();

        while (tickTime >= tickInterval)
        {
            if(LocalTick % 20 == 0)
            {
                serverWorld.ClearSnapshots(networkServer.GetOldestAckedSnapshot());
            }
            serverWorld.Update();
            networkServer.SendSnapshot();
            LocalTick++;
            tickTime = 0;
        }
        tickTime += Time.fixedDeltaTime;
    }

    public void AddFakePlayers(int playerCount, int movingPlayerCount)
    {
        serverWorld.AddFakePlayers(playerCount, movingPlayerCount);
    }

    public void RemoveFakePlayers()
    {
        serverWorld.RemoveFakePlayers();
    }
}