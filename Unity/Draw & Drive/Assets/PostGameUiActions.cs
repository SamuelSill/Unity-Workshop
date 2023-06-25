using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PostGameUiActions : MonoBehaviour
{
    public TextMeshProUGUI WinnerText;
    public TextMeshProUGUI LeftPresantageText;
    public TextMeshProUGUI RightPresantageText;
    public Button BackButton;

    public enum Teams { 
    Left,
    Right,
    Draw
    }
    private static Teams winner;
    private static float leftTeamPresentage;
    private static float rightTeamPresentage;
    void Start()
    {
        WinnerText.text = winner.ToString() + " Team";
        LeftPresantageText.text = rightTeamPresentage + "%";
        RightPresantageText.text = leftTeamPresentage + "%";
        BackButton.onClick.AddListener(BackButtonClicked);
        
    }
    private void BackButtonClicked()
    {
        SceneManager.LoadScene("Game Menu");
    }
    // Update is called once per frame
    public static void UpdateScore(float left, float right)
    {
        if (right < left)
        {
            winner = Teams.Left;
        }
        if (right > left)
        {
            winner = Teams.Right;
        }
        else {
            winner = Teams.Draw;
        }
        leftTeamPresentage = left;
        rightTeamPresentage = right;
    }
}
