using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class BackButton : NetworkBehaviour
{
    public void BackButtonClicked()
    {
        if (!IsServer)
        {
            disconnectClientServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            NetworkManager.SceneManager.LoadScene("Game Menu", LoadSceneMode.Additive);
        }


    }
    [ServerRpc(RequireOwnership = false)]
    private void disconnectClientServerRpc(ulong clientID)
    {
        //NetworkManager.Singleton.OnClientDisconnectCallback += loadNewScene;
        NetworkManager.Singleton.DisconnectClient(clientID);
    }
    private void loadNewScene(ulong ID)
    {
        if (NetworkManager.Singleton.LocalClientId == ID)
        {
            SceneManager.LoadScene("Game Menu");
        }
    }
}
