using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManegerUI : NetworkBehaviour
{
    public GameObject clientPrefab;
    public static readonly int NUMBER_OF_PLAYERS = 2;
    //public NetworkManager networkManager;
    public Image paintingToDraw;

    private static int currentMobileUser = 0;
    private static int totalMobileUsers = 0;
    private static Dictionary<int, string> mobileUserUsername = new Dictionary<int, string>();
    public static void AddUserToMobileUsers(string userName) 
    {
        Debug.Log("adding mobile user " + userName);
        if (mobileUserUsername == null) {
            mobileUserUsername = new Dictionary<int, string>();
        }
        mobileUserUsername.Add(totalMobileUsers, userName);
        totalMobileUsers++;
    }
    public static string GetCurrentMobileUser()  // property
    {
        if (mobileUserUsername.Count == 0) {
            totalMobileUsers = 0;
            foreach (string  mobileUserName in ServerSession.PlayerMobileControls.Keys)
            {
                NetworkManegerUI.AddUserToMobileUsers(mobileUserName);
            }
        }
        //Debug.Log("GetCurrentMobileUser: number of users: " + mobileUserUsername.Count);
        string userName = "";
        try{
            userName = mobileUserUsername[currentMobileUser];
            currentMobileUser++;
            currentMobileUser %= totalMobileUsers;
        }
        catch(Exception){
        }
        return userName;  // get method
    }

    private void Start()
    {
        paintingToDraw.sprite = Sprite.Create(
            ServerSession.CurrentGamePainting,
            new Rect(0, 0, ServerSession.CurrentGamePainting.width, ServerSession.CurrentGamePainting.height),
            new Vector2(0.5f, 0.5f)
        );

        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.SetConnectionData(ServerSession.HostIp, unityTransport.ConnectionData.Port);
        
        if (ServerSession.IsUnityHost)
        {
            NetworkManager.Singleton.StartHost();

            mobileUserUsername = new Dictionary<int, string>();

        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

    }
    public override void OnNetworkSpawn()
    {
        if (IsHost) {
            CreateMobileCars();
        }
    }
    private void CreateMobileCars() {
        foreach (string userName in ServerSession.PlayerMobileControls.Keys)
        {
            mobileUserUsername.Add(totalMobileUsers, userName);
            totalMobileUsers++;

            GameObject mobilePlayer = Instantiate(clientPrefab);
        }
    }
}
