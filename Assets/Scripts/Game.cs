using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    Client client;
    //Server server;

    void Awake()
    {
        //server = new Server();
        client = new Client();
    }

    void Start()
    {
        //server.Start();
        client.Start();
    }

    void FixedUpdate()
    {
        //server.FixedUpdate();
        client.FixedUpdate();
    }
}
