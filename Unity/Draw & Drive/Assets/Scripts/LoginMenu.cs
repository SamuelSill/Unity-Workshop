using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    public string backScene;
    public string loginScene;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Back()
    {
        SceneManager.LoadScene(backScene);
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest getRequest = UnityWebRequest.Get(uri);
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + getRequest.error);
        }
        else
        {
            SceneManager.LoadScene(getRequest.responseCode == 200 ? loginScene : backScene);
        }
    }

    public void Login()
    {
        StartCoroutine(GetRequest($"127.0.0.1:9583/players/login?username={usernameField.text}&password={passwordField.text}"));
    }
}
