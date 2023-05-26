using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene;
    public string credentialsFile;

    // Start is called before the first frame update
    void Start()
    {
        string destination = Application.persistentDataPath + "/" + credentialsFile;
        FileStream file;

        if (File.Exists(destination))
        {
            file = File.OpenRead(destination);
            BinaryFormatter bf = new BinaryFormatter();
            UserCredentials credentials = (UserCredentials)bf.Deserialize(file);
            file.Close();

            StartCoroutine(GetRequest(credentials.username, credentials.password));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetRequest(string username, string password)
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/players/login?username={username}&password={password}");
        yield return getRequest.SendWebRequest();


        if (getRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + getRequest.error);

            string destination = Application.persistentDataPath + "/" + credentialsFile;
            File.Delete(destination);
        }
        else if (getRequest.responseCode == 200)
        {
            SceneManager.LoadScene(gameScene);
            LoginMenu.loggedUsername = username;
            LoginMenu.loggedPassword = password;
        }
    }

    public void Exit()
    {
        Application.Quit(0);
    }
}
