using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
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

    public void SetCarToBuy(BuyCarsPanel.Car car, CarView newCarView, bool isCarOwned)
    {
        carView = newCarView;

        carType.text = car.id;
        carPriceText.text = $"{car.price}$";
        carPriceNumber = car.price;

        var image = GetComponent<Image>();
        image.sprite = CarView.GetCarSprite(car.id, car.skins[0].id);

        if (!UserDetailsPanel.CanBuy(carPriceNumber) || isCarOwned)
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
        if (UserDetailsPanel.CanBuy(carPriceNumber))
        {
            StartCoroutine(BuyCarCoroutine());
        }
    }

    IEnumerator BuyCarCoroutine()
    {
        var uwr = UnityWebRequest.Post($"{LoginMenu.serverURL}/players/cars?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}&car_id={carType.text}", "");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.responseCode == 200)
        {
            UserDetailsPanel.Buy(carPriceNumber);
            DisableBuying("PURCHASED");
            carView.Start();
        }
    }
}
