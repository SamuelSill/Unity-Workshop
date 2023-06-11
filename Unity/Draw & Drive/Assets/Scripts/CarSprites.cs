using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarSprites : MonoBehaviour
{
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
    }

    // Update is called once per frame
    void Update()
    {

    }
}
