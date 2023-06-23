using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManegerUI : MonoBehaviour
{
    public NetworkManager networkManager;

    private void Start()
    {
        var unityTransport = networkManager.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(ServerSession.HostIp, unityTransport.ConnectionData.Port);

        if (ServerSession.IsHost)
        {
            networkManager.StartHost();
        }
        else
        {
            networkManager.StartClient();
        }
    }
}
