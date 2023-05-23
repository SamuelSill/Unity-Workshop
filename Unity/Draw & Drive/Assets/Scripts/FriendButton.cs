using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class FriendButton : MonoBehaviour
{
    public TMP_Text usernameButton;
    public GameObject friendObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FriendClicked()
    {
        Debug.Log($"Clicking {usernameButton.text}");
    }

    public void RemoveFriend()
    {
        StartCoroutine(DeleteFriend());
    }

    IEnumerator DeleteFriend()
    {
        UnityWebRequest getRequest = UnityWebRequest.Delete($"{LoginMenu.serverURL}/players/friends?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}&friend_username={usernameButton.text}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            Destroy(friendObject);
        }
    }
}
