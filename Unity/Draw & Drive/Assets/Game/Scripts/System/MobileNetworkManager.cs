using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class MobileNetworkManager : NetworkBehaviour
{
    public GameObject clientPrefab;
    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            foreach (string userName in ServerSession.PlayerMobileControls.Keys)
            {
                GameObject mobilePlayer = Instantiate(clientPrefab);
                mobilePlayer.GetComponent<PlayerOptions>().UserName = userName;
                mobilePlayer.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}