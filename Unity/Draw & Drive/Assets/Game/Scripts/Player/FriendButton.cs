using TMPro;
using UnityEngine;

public class FriendButton : MonoBehaviour
{
    public TMP_Text usernameButton;
    public GameObject friendObject;
    public GameObject gameMenu;

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
        gameMenu.GetComponent<GameMenu>().ViewProfile(usernameButton.text);
    }

    public void RemoveFriend()
    {
        ServerSession.DeleteFriend(usernameButton.text, () => Destroy(friendObject));
    }
}
