using TMPro;
using UnityEngine;

public class InviteFriendButton : MonoBehaviour
{
    public TMP_Text usernameButton;
    public GameObject startMenu;
    public GameObject inviteFriendsPanel;

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
        startMenu.GetComponent<StartMenu>().DisplayFriend(usernameButton.text, () => inviteFriendsPanel.GetComponent<PanelCloser>().PanelClosed());
    }
}
