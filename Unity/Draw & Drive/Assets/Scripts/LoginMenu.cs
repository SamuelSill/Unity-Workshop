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
    public TMP_Text errorMessage;

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
            () => errorMessage.text = "FAILED TO LOGIN!"
        );
    }
}
