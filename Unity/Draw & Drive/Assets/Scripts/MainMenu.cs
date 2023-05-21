using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string loginScene;
    public string signupScene;
    public string aboutScene;

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
        SceneManager.LoadScene(loginScene);
    }

    public void Signup()
    {
        SceneManager.LoadScene(signupScene);
    }

    public void About()
    {
        SceneManager.LoadScene(aboutScene);
    }

    public void Exit()
    {
        Application.Quit(0);
    }
}
