using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        Debug.Log("Server started");
    }

    public override void OnStopServer()
    {
        Debug.Log("Server stopped");
    }

    public override void OnClientConnect()
    {
        Debug.Log("Client connected");
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("Client disconnected");
    }
}
