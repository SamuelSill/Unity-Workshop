using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public GameObject startMenu;
    public GameObject gameMenu;

    public TMP_InputField joinCodeInput;

    public GameObject selectedCarObject;
    public TMP_Text usernameText;
    public TMP_Text gameCodeText;

    public GameObject friendBox1;
    public GameObject friendBox2;

    int friendSelected;

    // Start is called before the first frame update
    public void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SelectFriendBox(int friendNumberSelected)
    {
        friendSelected = friendNumberSelected;
    }

    void LoadStartMenu(string gameCode, List<ServerSession.UserGameStats> players)
    {
        startMenu.SetActive(true);
        gameMenu.SetActive(false);

        selectedCarObject.GetComponent<Image>().sprite =
            CarSprites.GetCarSprite(ServerSession.CurrentCar.id, ServerSession.CurrentSkin);
        usernameText.text = ServerSession.Username;

        friendBox1.GetComponent<Image>().sprite = null;
        friendBox1.GetComponentInChildren<TMP_Text>().text = "";

        friendBox2.GetComponent<Image>().sprite = null;
        friendBox2.GetComponentInChildren<TMP_Text>().text = "";

        gameCodeText.text = gameCode;

        foreach (var player in players)
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

    public void CreateButtonPressed()
    {
        if (!ServerSession.IsSocketBusy())
        {
            ServerSession.CreateGame(
                gameCode => LoadStartMenu(gameCode, new List<ServerSession.UserGameStats>()),
                ShowJoinedPlayer,
                RemoveJoinedPlayer
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
                (players) => LoadStartMenu(joinCodeInput.text, players),
                (message) => { Debug.Log(message); },
                ShowJoinedPlayer,
                RemoveJoinedPlayer,
                BackButtonPressed
            );
        }
    }
}
