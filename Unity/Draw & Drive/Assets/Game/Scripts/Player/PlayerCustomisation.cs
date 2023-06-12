using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerCustomisation : NetworkBehaviour
{
    [Serializable]
    public struct TripleObjects
    {
        public Texture2D red;
        public Texture2D green;
        public Texture2D blue;
    }

    [SerializeField]
    private List<TripleObjects> skins;

    [SerializeField]
    private int skinChoice;

    [SerializeField]
    private veichleKind myVeichle;

    public int defultBrushSize = 3;
    public int BrushSize { get; set; }
    private SpriteRenderer moduleSprit;
    private SpriteRenderer[] playerModules;
    private readonly Color blue = new Color(0.2f, 0.4f, 1);
    public  moduleColors currentColor = moduleColors.blue;
    public enum veichleKind { 
        Car,
        Mortor,
        Truck
    }
    public enum moduleColors { 
        red,
        green,
        blue
    }
    public Color getColor() 
    {
        if(currentColor.Equals(moduleColors.blue))
        {
            return blue;
        }
        if (currentColor.Equals(moduleColors.red))
        {
            return Color.red;
        }
            return Color.green;

    }
    private void adjustSize() {
        BrushSize = (int)(defultBrushSize * transform.localScale.x);
    }
    public override void OnNetworkSpawn()
    {
        adjustSize();
        //ChangeSkin();
        Transform carModule = transform.Find("PlayerModule").GetChild(0);
        moduleSprit = carModule.GetComponent<SpriteRenderer>();     
        playerModules = transform.Find("PlayerModule").GetComponentsInChildren<SpriteRenderer>();
        SetMoudleOfSameColor();
        
    }
    private void ChangeSkin() {
        if (skinChoice < skins.Count && skinChoice > 0) {
            addNewSkinsServerRpc(skinChoice);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void addNewSkinsServerRpc(int playerChoice) {
        Transform playerModule = transform.Find("PlayerModule");

        deleteChildern(playerModule);

        TripleObjects veichleSkin = skins[playerChoice];

        CreateChildObject(playerModule.transform, veichleSkin.blue);
        CreateChildObject(playerModule.transform, veichleSkin.green);
        CreateChildObject(playerModule.transform, veichleSkin.red);

    }
    private void deleteChildern(Transform parent) {
        int childCount = parent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            Destroy(child.gameObject);
        }
    }
    private void CreateChildObject(Transform parent, Texture2D texture)
    {
        // Create a child object
        GameObject childObject = new GameObject(texture.name);
        childObject.transform.parent = parent;
        childObject.transform.position = parent.position;
        // Add a MeshRenderer and assign the texture
        SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        childObject.transform.localScale = new Vector3(0.1f, 0.1f, 1);
    }
    private void SetMoudleOfSameColor()
    {
        string colorLetter = currentColor.ToString().Substring(0, 1);
        //Debug.Log("letter : " + colorLetter);
        foreach (SpriteRenderer sprit in playerModules)
        {
            if (sprit.transform.name.Contains("_" + colorLetter.ToUpper()))
            {
                sprit.enabled = true;
            }
            else
            {
                sprit.enabled = false;
            }
        }
    }


// Update is called once per frame
void FixedUpdate()
    {

        adjustSize();
        moduleSprit.color = getColor();
        SetMoudleOfSameColor();
    }
}
