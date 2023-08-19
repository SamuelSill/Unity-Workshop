using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CarColor
{
    red,
    green,
    blue
}

public class CarSprites : MonoBehaviour
{
    [System.Serializable]
    public struct TripleObjects
    {
        public string carID;
        public string skinID;
        public Sprite red;
        public Sprite green;
        public Sprite blue;
    }

    [SerializeField]
    private List<TripleObjects> skins;

    public static List<TripleObjects> staticSkins;

    public static Sprite GetCarSprite(string carType, string carSkin, CarColor color = CarColor.red)
    {
        var triple = staticSkins.Find(skin => skin.carID == carType && skin.skinID == carSkin);
        if (color == CarColor.green) return triple.green;
        if (color == CarColor.red) return triple.red;
        return triple.blue;
    }

    // Start is called before the first frame update
    public void Start()
    {
        staticSkins = new List<TripleObjects>();
        foreach (var skin in skins)
        {
            staticSkins.Add(
                new TripleObjects() { 
                    blue = skin.blue, 
                    carID = skin.carID, 
                    green = skin.green, 
                    red = skin.red, 
                    skinID = skin.skinID 
                }
            );
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
