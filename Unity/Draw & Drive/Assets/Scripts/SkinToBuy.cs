using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinToBuy : MonoBehaviour
{
    public TMP_Text skinID;
    public TMP_Text skinPriceText;
    public TMP_Text skinSpecialMessage;
    public GameObject buyButton;

    private int skinPriceNumber;
    private CarView carView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSkinToBuy(ServerSession.Skin skin, CarView newCarView)
    {
        carView = newCarView;

        skinID.text = skin.id;
        skinPriceText.text = $"{skin.price}$";
        skinPriceNumber = skin.price;

        var image = GetComponent<Image>();
        image.sprite = CarView.GetCarSprite(ServerSession.CurrentCar.id, skin.id);

        bool isSkinOwned = ServerSession.CurrentCar.skins.Exists(ownedSkin => ownedSkin == skin.id);
        if (ServerSession.Money < skinPriceNumber || isSkinOwned)
        {
            DisableBuying(isSkinOwned ? "PURCHASED" : "CAN'T AFFORD");
        }
    }

    void DisableBuying(string message)
    {
        var image = GetComponent<Image>();
        var tempColor = image.color;
        tempColor.a = 127f;
        image.color = tempColor;
        skinSpecialMessage.text = message;
        buyButton.SetActive(false);
    }

    public void BuySkin()
    {
        ServerSession.PurchaseSkin(skinID.text, () =>
        {
            DisableBuying("PURCHASED");
            carView.Start();
        });
    }
}
