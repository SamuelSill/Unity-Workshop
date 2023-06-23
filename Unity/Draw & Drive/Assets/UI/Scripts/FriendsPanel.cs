using TMPro;
using UnityEngine;

public class FriendsPanel : MonoBehaviour
{
    public GameObject grid;
    public GameObject friendPrefab;
    public TMP_InputField friendToAddField;
    public GameObject gameMenu;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var friend in ServerSession.Friends)
        {
            DisplayFriend(friend);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisplayFriend(string friendUsername)
    {
        var newObject = Instantiate(friendPrefab, grid.transform);
        newObject.GetComponent<FriendButton>().usernameButton.text = friendUsername;
        newObject.GetComponent<FriendButton>().friendObject = newObject;
        newObject.GetComponent<FriendButton>().gameMenu = gameMenu;
    }

    public void AddFriend()
    {
        ServerSession.AddNewFriend(friendToAddField.text, () => DisplayFriend(friendToAddField.text));
    }
}
