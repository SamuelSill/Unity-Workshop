using TMPro;
using UnityEngine;

public class UserDetailsPanel : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text moneyText;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        usernameText.text = ServerSession.Username;
        moneyText.text = $"{ServerSession.Money}$";
    }
}
