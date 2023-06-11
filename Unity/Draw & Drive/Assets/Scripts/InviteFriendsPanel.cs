using TMPro;
using UnityEngine;

public class InviteFriendsPanel : MonoBehaviour
{
    public GameObject grid;
    public GameObject friendPrefab;
    public GameObject startMenu;

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
        newObject.GetComponent<InviteFriendButton>().usernameButton.text = friendUsername;
        newObject.GetComponent<InviteFriendButton>().startMenu = startMenu;
        newObject.GetComponent<InviteFriendButton>().inviteFriendsPanel = gameObject;
    }
}
