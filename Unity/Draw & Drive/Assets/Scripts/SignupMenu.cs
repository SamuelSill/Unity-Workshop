using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SignupMenu : MonoBehaviour
{
    public string serverURL = "localhost:5555";
    public string nextScene;
    public string passwordMismatchMessage = "Passwords Don't Match!";
    public string serverConnectionErrorMessage = "Server Connection Error!";
    public TMP_InputField firstNameField;
    public TMP_InputField lastNameField;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordConfirmation;
    public TMP_Text errorMessage;

    // Start is called before the first frame update
    void Start()
    {
        errorMessage.text = "";
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator PostRequest(string username, string password, string firstName, string lastName)
    {
        UnityWebRequest postRequest = new UnityWebRequest($"{LoginMenu.serverURL}/players/register", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(
            $"{{ " +
            $"\"username\": \"{usernameField.text}\", " +
            $"\"password\": \"{passwordField.text}\", " +
            $"\"first_name\": \"{firstNameField.text}\", " +
            $"\"last_name\": \"{lastNameField.text}\"" +
            $"}}"
        );

        postRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        postRequest.downloadHandler = new DownloadHandlerBuffer();
        postRequest.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return postRequest.SendWebRequest();

        if (postRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            errorMessage.text = serverConnectionErrorMessage;
        }
        else if (postRequest.responseCode != 200)
        {
            errorMessage.text = postRequest.downloadHandler.text.Substring(1, postRequest.downloadHandler.text.Length - 2);
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    public void Signup()
    {
        if (passwordConfirmation.text != passwordField.text)
        {
            errorMessage.text = passwordMismatchMessage;
            return;
        }

        StartCoroutine(PostRequest(usernameField.text, passwordField.text, firstNameField.text, lastNameField.text));
    }
}
