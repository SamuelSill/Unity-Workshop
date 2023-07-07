using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BuyCarsPanel : MonoBehaviour
{
    public GameObject carToBuyPrefab;
    public GameObject carView;

    // Start is called before the first frame update
    void Start()
    {
        var carViewComponent = carView.GetComponent<CarView>();
        foreach (var car in ServerSession.GameCars)
        {
            var newObject = Instantiate(carToBuyPrefab, transform);
            newObject.GetComponent<CarToBuy>().SetCarToBuy(car, carViewComponent);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
