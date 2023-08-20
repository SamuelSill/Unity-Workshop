using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Netcode;

public class MobileClient : MonoBehaviour
{
    
    void Start()
    {
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(ServerSession.HostIp, unityTransport.ConnectionData.Port);
        NetworkManager.Singleton.StartClient();
    }
}
