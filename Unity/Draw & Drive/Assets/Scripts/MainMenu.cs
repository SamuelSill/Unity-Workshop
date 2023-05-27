using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene;

    // Start is called before the first frame update
    void Start()
    {
        ServerSession.Initialize(() => SceneManager.LoadScene(gameScene), () => { });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Exit()
    {
        Application.Quit(0);
    }
}
