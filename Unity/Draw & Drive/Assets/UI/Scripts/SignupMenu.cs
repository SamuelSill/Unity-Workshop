using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignupMenu : MonoBehaviour
{
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField description;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordConfirmation;
    public Button signupButton;

    public GameObject loginMenu;
    public GameObject signupMenu;

    // Start is called before the first frame update
    void Start()
    {
        signupButton.interactable = true;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Signup()
    {
        if (passwordConfirmation.text != passwordField.text)
        {
            PopupMessage.Display("PASSWORDS DON'T MATCH!");
            return;
        }

        signupButton.interactable = false;

        ServerSession.CreateUser(
            usernameField.text, 
            passwordField.text,
            firstNameField.text, 
            lastNameField.text,
            description.text,
            () => {
                signupMenu.SetActive(false);
                loginMenu.SetActive(true);
            },
            () => {
                signupButton.interactable = true;
                PopupMessage.Display("FAILED TO SIGN UP!");
            }
        );
    }
}
