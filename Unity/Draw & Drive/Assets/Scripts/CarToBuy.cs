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

    public void SetCarToBuy(BuyCarsPanel.Car car, CarView newCarView)
    {
        carView = newCarView;

        carType.text = car.id;
        carPriceText.text = $"{car.price}$";
        carPriceNumber = car.price;
        GetComponent<Image>().sprite = CarView.GetCarSprite(car.id, car.skins[0].id);
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
            Destroy(gameObject);
            carView.Start();
        }
    }
}
