using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Linq;

public class PostGameUiActions : NetworkBehaviour
{
    public TextMeshProUGUI WinnerText;
    public TextMeshProUGUI LeftPresantageText;
    public TextMeshProUGUI RightPresantageText;

    public enum Teams { 
    Left,
    Right,
    Draw
    }
    private static Teams winner;
    private static float leftTeamPresentage;
    private static float rightTeamPresentage;
    NetworkManager networkManager;
    void Start()
    {
        WinnerText.text = winner == Teams.Draw ? "Draw!" : winner.ToString() + " Team Won!";
        LeftPresantageText.text = leftTeamPresentage.ToString("F2") + "%";
        RightPresantageText.text = rightTeamPresentage.ToString("F2") + "%";

        if (winner == Teams.Draw || 
            (ServerSession.CurrentTeam != winner.ToString().ToLower()))
        {
            ServerSession.Money += 50;
        }
        else
        {
            ServerSession.Money += 100;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnect;

    }
    public void BackButtonClicked()
    {
        if (!IsServer)
        {
            disconnectClientServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else {
            // NetworkManager.SceneManager.LoadScene("Game Menu", LoadSceneMode.Single);
            if (IsHost)
            {   
                //NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
                SceneManager.LoadScene("Game Menu");
                NetworkManager.Singleton.Shutdown();
            }
            
        }
        
    }
    public override void OnDestroy()
    {
        // Unsubscribe from the event when the script is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnect;
        }
        //base.OnDestroy();
    }

    private void OnPlayerDisconnect(ulong clientId)
    {

        SceneManager.LoadScene("Game Menu", LoadSceneMode.Single);
    }
    [ServerRpc(RequireOwnership = false)]
    private void disconnectClientServerRpc(ulong clientID) {
        
        NetworkManager.Singleton.DisconnectClient(clientID);
    }
    private void loadNewScene(ulong ID) {
        if (NetworkManager.Singleton.LocalClientId == ID)
        {
            SceneManager.LoadScene("Game Menu");
        }
    }

    // Update is called once per frame
    public static void UpdateScore(float left, float right)
    {
        if (right < left)
        {
            winner = Teams.Left;
        }
        else if (right > left)
        {
            winner = Teams.Right;
        }
        else 
        {
            winner = Teams.Draw;
        }

        leftTeamPresentage = left;
        rightTeamPresentage = right;
    }
}
