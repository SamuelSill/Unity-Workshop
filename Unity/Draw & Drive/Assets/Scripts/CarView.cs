using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarView : MonoBehaviour
{
    public GameObject selectedCarObject;
    public TMP_Text selectedCarType;
    public TMP_Text selectedCarUpgrades;
    public TMP_Text carSpeed;
    public TMP_Text carSteering;
    public TMP_Text carThickness;
    public GameObject upgradeSpeedButton;
    public GameObject upgradeSteeringButton;
    public GameObject upgradeThicknessButton;

    // Sprites
    public Sprite blueAutoCarSprite;
    public Sprite redAutoCarSprite;

    private static Dictionary<string, Sprite> idToSpriteMap;

    public static Sprite GetCarSprite(string carType, string carSkin)
    {
        return idToSpriteMap[$"{carType}.{carSkin}"];
    }

    // Start is called before the first frame update
    public void Start()
    {
        if (idToSpriteMap == null)
        {
            idToSpriteMap = new()
            {
                ["auto.blue"] = blueAutoCarSprite,
                ["auto.red"] = redAutoCarSprite
            };
        }

        if (ServerSession.OwnedCars.Count > 0)
        {
            ShowSelectedCar();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPressLeft()
    {
        ServerSession.SelectCar((ServerSession.OwnedCars.Count + ServerSession.CurrentCarIndex - 1) % ServerSession.OwnedCars.Count, 
                                ShowSelectedCar);
    }

    public void OnPressRight()
    {
        ServerSession.SelectCar((ServerSession.CurrentCarIndex + 1) % ServerSession.OwnedCars.Count,
                                ShowSelectedCar);
    }

    public void OnPressUp()
    {
        ServerSession.SelectSkin((ServerSession.CurrentCar.selected_skin + 1) % ServerSession.CurrentCar.skins.Count,
                                 ShowSelectedCar);
    }

    public void OnPressDown()
    {
        ServerSession.SelectSkin((ServerSession.CurrentCar.skins.Count + ServerSession.CurrentCar.selected_skin - 1) % ServerSession.CurrentCar.skins.Count,
                                 ShowSelectedCar);
    }

    void ShowSelectedCar()
    {
        // Car
        selectedCarObject.GetComponent<Image>().sprite = 
            GetCarSprite(ServerSession.CurrentCar.id, ServerSession.CurrentSkin);
        selectedCarType.text = ServerSession.CurrentCar.id;

        // Upgrades
        carSpeed.text = ServerSession.CurrentCarSpeed.ToString();
        carSteering.text = ServerSession.CurrentCarSteering.ToString();
        carThickness.text = ServerSession.CurrentCarThickness.ToString();

        // Upgrade Prices
        DisplayUpgradeCost(upgradeSpeedButton, ServerSession.SpeedUpgradeCost);
        DisplayUpgradeCost(upgradeSteeringButton, ServerSession.SteeringUpgradeCost);
        DisplayUpgradeCost(upgradeThicknessButton, ServerSession.ThicknessUpgradeCost);
    }

    void DisplayUpgradeCost(GameObject button, int currentUpgradeCost)
    {
        if (currentUpgradeCost != -1)
        {
            button.GetComponentInChildren<TMP_Text>().text = $"UPGRADE\n{currentUpgradeCost}$";
        }
        else
        {
            button.SetActive(false);
        }
    }

    public void UpgradeSpeed()
    {
        ServerSession.PurchaseUpgrade("speed", ShowSelectedCar);
    }

    public void UpgradeSteering()
    {
        ServerSession.PurchaseUpgrade("steering", ShowSelectedCar);
    }

    public void UpgradeThickness()
    {
        ServerSession.PurchaseUpgrade("thickness", ShowSelectedCar);
    }
}
