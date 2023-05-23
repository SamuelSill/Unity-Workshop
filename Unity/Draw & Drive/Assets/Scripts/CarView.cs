using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarView : MonoBehaviour
{
    public GameObject selectedCarObject;
    public TMP_Text selectedCarType;
    public TMP_Text selectedCarUpgrades;

    // Sprites
    public Sprite blueAutoCarSprite;
    public Sprite redAutoCarSprite;

    private int selectedCarIndex;
    private List<PlayerCar> ownedCars;

    private Dictionary<string, Sprite> idToSpriteMap;

    // Start is called before the first frame update
    void Start()
    {
        idToSpriteMap = new Dictionary<string, Sprite>();
        idToSpriteMap["auto.blue"] = blueAutoCarSprite;
        idToSpriteMap["auto.red"] = redAutoCarSprite;
        StartCoroutine(GetCars());
    }

    // Update is called once per frame
    void Update()
    {

    }

    [System.Serializable]
    class PlayerCarUpgrade
    {
        public int speed;
        public int thickness;
        public int steering;
    }

    [System.Serializable]
    class PlayerCar
    {
        public string id;
        public PlayerCarUpgrade upgrades;
        public List<string> skins;
        public int selected_skin;
    }

    [System.Serializable]
    class PlayerCars
    {
        public List<PlayerCar> cars;
    }

    [System.Serializable]
    class SelectedCar
    {
        public int selectedCarIndex;
    }

    IEnumerator GetCars()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/players/cars?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            ownedCars = JsonUtility.FromJson<PlayerCars>($"{{\"cars\": {getRequest.downloadHandler.text}}}").cars;

            UnityWebRequest selectedCarRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/players/cars/selected?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}");
            yield return selectedCarRequest.SendWebRequest();

            if (selectedCarRequest.result != UnityWebRequest.Result.ConnectionError && selectedCarRequest.responseCode == 200)
            {
                selectedCarIndex = JsonUtility.FromJson<SelectedCar>($"{{\"selectedCar\": {selectedCarRequest.downloadHandler.text}}}").selectedCarIndex;
                ShowSelectedCar();
            }
        }
    }

    public void OnPressLeft()
    {
        if (selectedCarIndex > 0)
        {
            StartCoroutine(NotifySelectedCar(selectedCarIndex - 1));
        }
    }

    IEnumerator NotifySelectedCar(int newCarIndex)
    {
        UnityWebRequest getRequest = UnityWebRequest.Put($"{LoginMenu.serverURL}/players/cars?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}&car_index={newCarIndex}", "");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            selectedCarIndex = newCarIndex;
            ShowSelectedCar();
        }
    }

    public void OnPressRight()
    {
        if (selectedCarIndex < ownedCars.Count - 1)
        {
            StartCoroutine(NotifySelectedCar(selectedCarIndex + 1));
        }
    }

    public void OnPressUp()
    {
        if (ownedCars[selectedCarIndex].selected_skin < ownedCars[selectedCarIndex].skins.Count - 1)
        {
            StartCoroutine(NotifySelectedSkin(ownedCars[selectedCarIndex].selected_skin + 1));
        }
    }

    public void OnPressDown()
    {
        if (ownedCars[selectedCarIndex].selected_skin > 0)
        {
            StartCoroutine(NotifySelectedSkin(ownedCars[selectedCarIndex].selected_skin - 1));
        }
    }

    IEnumerator NotifySelectedSkin(int newSkinIndex)
    {
        UnityWebRequest getRequest = UnityWebRequest.Put($"{LoginMenu.serverURL}/players/cars/skins?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}&car_index={selectedCarIndex}&skin_index={newSkinIndex}", "");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            ownedCars[selectedCarIndex].selected_skin = newSkinIndex;
            ShowSelectedCar();
        }
    }

    void ShowSelectedCar()
    {
        selectedCarObject.GetComponent<Image>().sprite = idToSpriteMap[$"{ownedCars[selectedCarIndex].id}.{ownedCars[selectedCarIndex].skins[ownedCars[selectedCarIndex].selected_skin]}"];
        selectedCarType.text = ownedCars[selectedCarIndex].id;
        selectedCarUpgrades.text = 
            "Speed: " + ownedCars[selectedCarIndex].upgrades.speed.ToString() + "\n" +
            "Steering: " + ownedCars[selectedCarIndex].upgrades.steering.ToString() + "\n" +
            "Thickness: " + ownedCars[selectedCarIndex].upgrades.thickness.ToString() + "\n";
    }
}
