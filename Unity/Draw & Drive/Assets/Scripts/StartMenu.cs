using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public GameObject selectedCarObject;
    public TMP_Text usernameText;

    public GameObject friendBox1;
    public GameObject friendBoxButton1;
    public GameObject friendBox2;
    public GameObject friendBoxButton2;
    public GameObject FriendBoxSelected => friendSelected == 1 ? friendBox1 : friendBox2;
    public GameObject FriendBoxButtonSelected => friendSelected == 1 ? friendBoxButton1 : friendBoxButton2;

    int friendSelected;

    // Start is called before the first frame update
    public void Start()
    {
        if (ServerSession.OwnedCars.Count > 0)
        {
            selectedCarObject.GetComponent<Image>().sprite =
                CarSprites.GetCarSprite(ServerSession.CurrentCar.id, ServerSession.CurrentSkin);
            usernameText.text = ServerSession.Username;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectFriendBox(int friendNumberSelected)
    {
        friendSelected = friendNumberSelected;
    }

    public void DisplayFriend(string friendUsername, Action OnSuccess)
    {
        if (friendBox1.GetComponentInChildren<TMP_Text>().text != friendUsername &&
            friendBox2.GetComponentInChildren<TMP_Text>().text != friendUsername)
        {
            FriendBoxButtonSelected.SetActive(false);
            ServerSession.GetUserDetails(
                friendUsername,
                (userDetails) =>
                {
                    FriendBoxSelected.GetComponent<Image>().sprite =
                        CarSprites.GetCarSprite(userDetails.selected_car.id, userDetails.selected_car.skins[userDetails.selected_car.selected_skin]);
                    FriendBoxSelected.GetComponentInChildren<TMP_Text>().text = friendUsername;
                    OnSuccess.Invoke();
                }
            );
        }
    }

    public void ClearStartMenu()
    {
        friendBox1.GetComponent<Image>().sprite = null;
        friendBox1.GetComponentInChildren<TMP_Text>().text = "";
        friendBoxButton1.SetActive(true);

        friendBox2.GetComponent<Image>().sprite = null;
        friendBox2.GetComponentInChildren<TMP_Text>().text = "";
        friendBoxButton2.SetActive(true);
    }

    public void StartButtonPressed()
    {

    }
}
