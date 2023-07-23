using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public string gameScene;

    public GameObject startMenu;
    public GameObject gameMenu;

    public GameObject startButton;
    public TMP_InputField joinCodeInput;

    public GameObject selectedCarObject;
    public TMP_Text usernameText;
    public TMP_Text gameCodeText;
    public GameObject matchingText;

    public GameObject friendBox1;
    public GameObject friendBox2;

    bool isHost;

    // Start is called before the first frame update
    public void Start()
    {
        if (ServerSession.IsInLobby())
        {
            LoadStartMenu();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void LoadStartMenu()
    {
        isHost = (ServerSession.LobbyPlayers.Count == 0);

        startButton.SetActive(isHost);
        startMenu.SetActive(true);
        gameMenu.SetActive(false);
        matchingText.SetActive(false);

        selectedCarObject.GetComponent<Image>().sprite =
            CarSprites.GetCarSprite(ServerSession.CurrentCar.id, ServerSession.CurrentSkin);
        usernameText.text = ServerSession.Username;

        friendBox1.GetComponent<Image>().sprite = null;
        friendBox1.GetComponentInChildren<TMP_Text>().text = "";

        friendBox2.GetComponent<Image>().sprite = null;
        friendBox2.GetComponentInChildren<TMP_Text>().text = "";

        gameCodeText.text = ServerSession.LobbyCode;

        foreach (var player in ServerSession.LobbyPlayers.Values)
        {
            ShowJoinedPlayer(player);
        }
    }

    void ShowJoinedPlayer(ServerSession.UserGameStats player)
    {
        GameObject friendBox = friendBox1.GetComponentInChildren<TMP_Text>().text == null ? friendBox1 : friendBox2;
        friendBox.GetComponent<Image>().sprite =
                    CarSprites.GetCarSprite(player.selected_car.id, player.selected_car.skins[player.selected_car.selected_skin]);
        friendBox.GetComponentInChildren<TMP_Text>().text = player.username;
    }

    void RemoveJoinedPlayer(string username)
    {
        GameObject friendBox = friendBox1.GetComponentInChildren<TMP_Text>().text == username ? friendBox1 : friendBox2;
        friendBox.GetComponent<Image>().sprite = null;
        friendBox.GetComponentInChildren<TMP_Text>().text = "";
    }

    void GameStarted()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void CreateButtonPressed()
    {
        if (!ServerSession.IsSocketBusy())
        {
            ServerSession.CreateGame(
                LoadStartMenu,
                ShowJoinedPlayer,
                RemoveJoinedPlayer,
                GameStarted
            );
        }
    }

    public void BackButtonPressed()
    {
        ServerSession.CloseGameSocket();
        gameMenu.SetActive(true);
        startMenu.SetActive(false);
    }

    public void JoinTextUpdated()
    {
        joinCodeInput.text = joinCodeInput.text.ToUpper().Substring(0, Math.Min(4, joinCodeInput.text.Length));

        if (joinCodeInput.text.Length == 4 && !ServerSession.IsSocketBusy())
        {
            ServerSession.JoinGame(
                joinCodeInput.text,
                LoadStartMenu,
                ShowJoinedPlayer,
                RemoveJoinedPlayer,
                GameStarted,
                BackButtonPressed
            );
        }
    }

    public void StartGame()
    {
        if (isHost)
        {
            matchingText.SetActive(true);
            ServerSession.StartGame();
        }
    }
}
