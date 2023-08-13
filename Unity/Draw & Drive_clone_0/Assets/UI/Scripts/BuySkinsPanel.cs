using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuySkinsPanel : MonoBehaviour
{
    public GameObject skinToBuyPrefab;
    public GameObject carView;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowSkinsToBuy()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var carViewComponent = carView.GetComponent<CarView>();
        foreach (var skin in ServerSession.CurrentGameCar.skins)
        {
            var newObject = Instantiate(skinToBuyPrefab, transform);
            newObject.GetComponent<SkinToBuy>().SetSkinToBuy(skin, carViewComponent);
        }
    }
}
