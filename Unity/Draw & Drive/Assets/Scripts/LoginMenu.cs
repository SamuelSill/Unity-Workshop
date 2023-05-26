using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    public static string serverURL = "localhost:5555";
    public string serverConnectionErrorMessage = "SERVER CONNECTION ERROR";
    public string failedLoginMessage = "INVALID USERNAME OR PASSWORD";
    public string gameScene;
    public string savedCredentialsFilename;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Toggle rememberMeToggle;
    public TMP_Text errorMessage;

    public static string loggedUsername = "";
    public static string loggedPassword = "";

    // Start is called before the first frame update
    void Start()
    {
        errorMessage.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GetRequest(string username, string password)
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{serverURL}/players/login?username={username}&password={password}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            errorMessage.text = serverConnectionErrorMessage;
            Debug.Log("Error While Sending: " + getRequest.error);
        }
        else if (getRequest.responseCode == 200)
        {
            if (rememberMeToggle.isOn)
            {
                string destination = Application.persistentDataPath + "/" + savedCredentialsFilename;
                FileStream file;

                if (File.Exists(destination)) file = File.OpenWrite(destination);
                else file = File.Create(destination);

                UserCredentials data = new UserCredentials(username, password);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, data);
                file.Close();
            }

            loggedUsername = username;
            loggedPassword = password;

            SceneManager.LoadScene(gameScene);
        }
        else
        {
            errorMessage.text = failedLoginMessage;
        }
    }

    public void Login()
    {
        StartCoroutine(GetRequest(usernameField.text, passwordField.text));
    }
}
