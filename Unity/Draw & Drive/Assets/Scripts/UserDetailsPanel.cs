using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UserDetailsPanel : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text moneyText;

    private static int money = 0;

    // Start is called before the first frame update
    void Start()
    {
        usernameText.text = LoginMenu.loggedUsername;
        StartCoroutine(GetUserMoney());
    }

    IEnumerator GetUserMoney()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/players/stats/money?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            money = int.Parse(getRequest.downloadHandler.text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        moneyText.text = $"{money}$";
    }

    public static void Buy(int price)
    {
        money -= price;
    }

    public static void Earn(int price)
    {
        money += price;
    }

    public static bool CanBuy(int price)
    {
        return money >= price;
    }
}
