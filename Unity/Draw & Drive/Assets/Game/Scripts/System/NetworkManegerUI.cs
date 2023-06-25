using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManegerUI : MonoBehaviour
{
    public static readonly int NUMBER_OF_PLAYERS = 2;
    public NetworkManager networkManager;
    public Image paintingToDraw;
    private void Start()
    {
        paintingToDraw.sprite = Sprite.Create(
            ServerSession.CurrentGamePainting,
            new Rect(0, 0, ServerSession.CurrentGamePainting.width, ServerSession.CurrentGamePainting.height),
            new Vector2(0.5f, 0.5f)
        );

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
