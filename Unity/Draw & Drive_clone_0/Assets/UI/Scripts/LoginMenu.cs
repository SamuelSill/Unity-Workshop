using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    public string gameScene;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Toggle rememberMeToggle;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Login()
    {
        ServerSession.Initialize(
            usernameField.text,
            passwordField.text,
            rememberMeToggle.isOn,
            () => SceneManager.LoadScene(gameScene),
            () => PopupMessage.Display("FAILED TO LOGIN!")
        );
    }
}
