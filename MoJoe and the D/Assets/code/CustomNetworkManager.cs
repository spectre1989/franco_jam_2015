using UnityEngine;
using UnityEngine.Networking;
using System;

public class CustomNetworkManager : NetworkManager
{
    public String playerName = "Player";

    // We do all the spawning in GameInfo
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
    }
}
