using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SignupMenu : MonoBehaviour
{
    public string nextScene;
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordConfirmation;
    public TMP_Text errorMessage;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Signup()
    {
        if (passwordConfirmation.text != passwordField.text)
        {
            errorMessage.text = "PASSWORDS DON'T MATCH!";
            return;
        }

        ServerSession.CreateUser(
            usernameField.text, 
            passwordField.text, 
            firstNameField.text, 
            lastNameField.text,
            () => SceneManager.LoadScene(nextScene),
            () => errorMessage.text = "FAILED TO SIGN UP!"
        );
    }
}
