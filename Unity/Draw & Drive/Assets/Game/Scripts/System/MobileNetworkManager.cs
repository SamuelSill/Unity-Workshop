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
        Debug.Log("number of mobile users: " + ServerSession.PlayerMobileControls.Keys.Count);
        if (IsHost)
        {
            CreateMobileCars();
        }
        else if (ServerSession.PlayerMobileControls.Count > 0) 
        {
            AddClientMobileUsers();
        }
    }
    private void CreateMobileCars()
    {
        
        if (!IsHost)
        {
            return;
        }
        
        foreach (string userName in ServerSession.PlayerMobileControls.Keys)
        {
            //Debug.Log("mobile user names :" + userName);
            //NetworkManegerUI.AddUserToMobileUsers(userName);

            GameObject mobilePlayer = Instantiate(clientPrefab);
            mobilePlayer.GetComponent<NetworkObject>().Spawn();
        }
    }
    private void AddClientMobileUsers()
    {
        foreach (string userName in ServerSession.PlayerMobileControls.Keys)
        {
            //Debug.Log("mobile user names :" + userName);
            //NetworkManegerUI.AddUserToMobileUsers(userName);
            AddMobileUseServerRpc(userName);
            
            
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void AddMobileUseServerRpc(string username)
    {
        //try
       // {
        //    Queue<ServerSession.MobileControls> q = new Queue<ServerSession.MobileControls>();
        //    object que = RawDeserializeEx(queue, q.GetType());
        //    q = (Queue<ServerSession.MobileControls>)que;
       //     ServerSession.PlayerMobileControls.Add(username, q);
       // }
        //catch {
        //    Debug.Log("AddMobileUseServerRpc Deserialization faild ");
        // } 
        ServerSession.PlayerMobileControls.Add(username, new Queue<ServerSession.MobileControls>());
        GameObject mobilePlayer = Instantiate(clientPrefab);
        mobilePlayer.GetComponent<NetworkObject>().Spawn();

    }
    public static object RawDeserializeEx(byte[] arrBytes, Type anytype)
    {
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            return obj;
        }
    }

    public static byte[] RawSerializeEx(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
}