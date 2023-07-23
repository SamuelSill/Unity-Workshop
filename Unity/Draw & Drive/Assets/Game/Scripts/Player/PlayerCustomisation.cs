using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerCustomisation : NetworkBehaviour
{
    [SerializeField]
    private int skinChoice;

    public int defultBrushSize = 3;
    public int BrushSize { get; set; }
    //private SpriteRenderer moduleSprit;
    private SpriteRenderer[] playerModules;

    private readonly Color blue = new Color(0.2f, 0.4f, 1);
    public CarColor currentColor = CarColor.blue;

    public Color getColor() 
    {
        if(currentColor.Equals(CarColor.blue))
        {
            return blue;
        }
        if (currentColor.Equals(CarColor.red))
        {
            return Color.red;
        }

        return Color.green;
    }

    private void adjustSize() {
        BrushSize = (int)(ServerSession.CurrentCarThickness * transform.localScale.x);
    }
    public override void OnNetworkSpawn()
    {
        adjustSize();
        ChangeSkin();
        playerModules = transform.Find("PlayerModule").GetComponentsInChildren<SpriteRenderer>();
        SetMoudleOfSameColor();
        
    }
    private void ChangeSkin() {
        addNewSkinsServerRpc(ServerSession.CurrentCar.id, ServerSession.CurrentSkin);
    }

    [ServerRpc(RequireOwnership = false)]
    private void addNewSkinsServerRpc(string carID, string skinID) {
        UpdateSkinsClientRpc(carID, skinID);
    }

    [ClientRpc]
    private void UpdateSkinsClientRpc(string carID, string skinID)
    {
        Transform playerModule = transform.Find("PlayerModule");
        deleteChildern(playerModule);

        if (playerModule.childCount > 2) {
            return;
        }

        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.blue));
        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.green));
        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.red));        
    }

    private void deleteChildern(Transform parent) {
        parent.Find("DefultView").GetComponent<SpriteRenderer>().enabled = false;
    }
    private void CreateChildObject(Transform parent, Sprite sprite)
    {
        // Create a child object
        GameObject childObject = new GameObject(sprite.name);
        childObject.transform.parent = parent;
        childObject.transform.position = parent.position;
        // Add a MeshRenderer and assign the texture
        SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }
    private void SetMoudleOfSameColor()
    {
        if (IsOwner) {
            char colorLetter = currentColor.ToString()[0];
            updateSkinColorServerRpc(colorLetter);
        }
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void updateSkinColorServerRpc(char colorLetter)
    {
        foreach (SpriteRenderer sprit in playerModules)
        {
            if (sprit.transform.name.Contains("_" + colorLetter.ToString().ToUpper()))
            {
                sprit.enabled = true;
            }
            else
            {
                sprit.enabled = false;
            }
        }

        updateSkinColorClientRpc(colorLetter);
    }
    [ClientRpc]
    private void updateSkinColorClientRpc(char colorLetter)
    {
        playerModules = transform.Find("PlayerModule").GetComponentsInChildren<SpriteRenderer>();

        if (playerModules != null)
        {
            //Debug.Log(playerModules.Length);
            foreach (SpriteRenderer sprit in playerModules)
            {
                if (sprit.transform.name.Contains("_" + colorLetter.ToString().ToUpper()))
                {
                    sprit.enabled = true;
                }
                else
                {
                    sprit.enabled = false;
                }
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        adjustSize();
        
        //moduleSprit.color = getColor();
        SetMoudleOfSameColor();
    }
}
