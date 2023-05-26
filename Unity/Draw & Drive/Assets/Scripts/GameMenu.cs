using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public string savedCredentialsFilename;
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
        string destination = Application.persistentDataPath + "/" + savedCredentialsFilename;
        if (File.Exists(destination)) File.Delete(destination);

        SceneManager.LoadScene(logOutScene);
    }
}
