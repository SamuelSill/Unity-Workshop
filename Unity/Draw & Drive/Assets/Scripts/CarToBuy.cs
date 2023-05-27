using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarToBuy : MonoBehaviour
{
    public TMP_Text carType;
    public TMP_Text carPriceText;
    public TMP_Text carSpecialMessage;
    public GameObject buyButton;

    private int carPriceNumber;
    private CarView carView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCarToBuy(ServerSession.Car car, CarView newCarView)
    {
        carView = newCarView;

        carType.text = car.id;
        carPriceText.text = $"{car.price}$";
        carPriceNumber = car.price;

        var image = GetComponent<Image>();
        image.sprite = CarView.GetCarSprite(car.id, car.skins[0].id);

        bool isCarOwned = ServerSession.OwnedCars.Exists(ownedCar => ownedCar.id == car.id);
        if (ServerSession.Money < carPriceNumber || isCarOwned)
        {
            DisableBuying(isCarOwned ? "PURCHASED" : "CAN'T AFFORD");
        }
    }

    void DisableBuying(string message)
    {
        var image = GetComponent<Image>();
        var tempColor = image.color;
        tempColor.a = 127f;
        image.color = tempColor;
        carSpecialMessage.text = message;
        buyButton.SetActive(false);
    }

    public void BuyCar()
    {
        ServerSession.PurchaseCar(carType.text, () =>
        {
            DisableBuying("PURCHASED");
            carView.Start();
        });
    }
}
