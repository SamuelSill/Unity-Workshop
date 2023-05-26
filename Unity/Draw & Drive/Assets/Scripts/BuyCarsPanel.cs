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
        StartCoroutine(GetCarsToBuy());
    }

    // Update is called once per frame
    void Update()
    {

    }

    [System.Serializable]
    public class Upgrade
    {
        public string id;
        public List<int> pricing;
    }

    [System.Serializable]
    public class Skin
    {
        public string id;
        public int price;
    }

    [System.Serializable]
    public class Car
    {
        public string id;
        public string description;
        public int baseSpeed;
        public int thickness;
        public int steering;
        public List<Upgrade> upgrades;
        public List<Skin> skins;
        public int price;
    }

    [System.Serializable]
    public class Cars
    {
        public List<Car> cars;
    }

    IEnumerator GetCarsToBuy()
    {
        var getRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/cars");

        //Send the request then wait here until it returns
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            var cars = JsonUtility.FromJson<Cars>($"{{\"cars\": {getRequest.downloadHandler.text}}}");

            foreach (var car in cars.cars)
            {
                var newObject = Instantiate(carToBuyPrefab, transform);
                newObject.GetComponent<CarToBuy>().SetCarToBuy(car, carView.GetComponent<CarView>());
            }
        }
    }
}
