using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MobileNetworkManager : NetworkBehaviour
{
    public GameObject clientPrefab;
    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            CreateMobileCars();
        }
    }
    private void CreateMobileCars()
    {
        if (!IsHost)
        {
            return;
        }
        Debug.Log("number of mobile users: " + ServerSession.PlayerMobileControls.Keys.Count);
        foreach (string userName in ServerSession.PlayerMobileControls.Keys)
        {
            Debug.Log("mobile user names :" + userName);
            NetworkManegerUI.AddUserToMobileUsers(userName);

            GameObject mobilePlayer = Instantiate(clientPrefab);
            mobilePlayer.GetComponent<NetworkObject>().Spawn();
        }
    }
}