using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class FriendsPanel : MonoBehaviour
{
    public GameObject grid;
    public GameObject friendPrefab;
    public TMP_InputField friendToAddField;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetFriends());
    }

    [System.Serializable]
    class JsonFriends
    {
        public string[] names;
    }

    IEnumerator GetFriends()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/players/friends?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            var friendsList = JsonUtility.FromJson<JsonFriends>($"{{\"names\": {getRequest.downloadHandler.text}}}");

            foreach (var friend in friendsList.names)
            {
                var newObject = Instantiate(friendPrefab, grid.transform);
                newObject.GetComponent<FriendButton>().usernameButton.text = friend;
                newObject.GetComponent<FriendButton>().friendObject = newObject;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddFriend()
    {
        StartCoroutine(PostFriend(friendToAddField.text));
    }

    IEnumerator PostFriend(string username)
    {
        UnityWebRequest getRequest = UnityWebRequest.Post($"{LoginMenu.serverURL}/players/friends?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}&friend_username={username}", "");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            var newObject = Instantiate(friendPrefab, grid.transform);
            newObject.GetComponent<FriendButton>().usernameButton.text = username;
            newObject.GetComponent<FriendButton>().friendObject = newObject;
        }
    }
}
