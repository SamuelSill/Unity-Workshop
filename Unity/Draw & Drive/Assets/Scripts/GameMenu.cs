using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public string logOutScene;

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
}
