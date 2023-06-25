using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public string logOutScene;

    public GameObject gameMenu;
    public GameObject profileMenu;

    [Header("Profile Data")]
    public TMP_Text profileUsername;
    public TMP_Text profileDescription;
    public TMP_Text profileGamesWon;
    public TMP_Text profileGamesLost;
    public TMP_Text profileWinLoseRatio;
    public TMP_Text profileAverageAccuracy;
    public Image profileSelectedCar;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LogOut()
    {
        ServerSession.LogOut();
        SceneManager.LoadScene(logOutScene);
    }

    public void Exit()
    {
        Application.Quit(0);
    }

    public void ViewProfile(string username)
    {
        ServerSession.GetUserDetails(
            username, 
            (userDetails) =>
            {
                profileUsername.text = username;
                profileDescription.text = $"\"{userDetails.description}\"";
                profileGamesWon.text = userDetails.games_won.ToString();
                profileGamesLost.text = userDetails.games_lost.ToString();
                profileWinLoseRatio.text =
                    userDetails.games_lost == 0 ?
                    "N/A" :
                    (userDetails.games_won * 1.0 / userDetails.games_lost).ToString("N2");
                profileAverageAccuracy.text = 
                    (userDetails.games_won + userDetails.games_lost) == 0 ? 
                    "N/A" :
                    (
                        userDetails.sum_accuracy * 1.0 / 
                        (userDetails.games_won + userDetails.games_lost)
                    ).ToString("N2");
                profileSelectedCar.sprite = CarSprites.GetCarSprite(
                    userDetails.selected_car.id, 
                    userDetails.selected_car.skins[userDetails.selected_car.selected_skin]
                );

                gameMenu.SetActive(false);
                profileMenu.SetActive(true);
            }
        );
    }

    public void ViewMyProfile()
    {
        ViewProfile(ServerSession.Username);
    }
}
