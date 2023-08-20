using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerCustomisation : NetworkBehaviour
{
    Dictionary<ulong, Tuple<string, string>> clientSkins;

    [SerializeField]
    private int skinChoice;

    public int BrushSize { get; set; }
    private SpriteRenderer[] playerModules;

    public CarColor currentColor = CarColor.blue;

    public Color getColor()
    {
        if (currentColor.Equals(CarColor.blue))
        {
            return Color.blue;
        }
        if (currentColor.Equals(CarColor.red))
        {
            return Color.red;
        }

        return Color.green;
    }

    private void AdjustSize()
    {
        BrushSize = (int)(ServerSession.CurrentCarThickness * transform.localScale.x);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            clientSkins = new Dictionary<ulong, Tuple<string, string>>();
        }

        //Debug.Log("Network Spawn: " + IsOwner.ToString());
        AdjustSize();
        if (IsOwner)
        {
            InformServerOfCarSkinServerRpc(ServerSession.CurrentCar.id, ServerSession.CurrentSkin, OwnerClientId);
        }
        else
        {
            GetCarSkinOfObjectServerRpc(OwnerClientId);
        }

        playerModules = transform.Find("PlayerModule").GetComponentsInChildren<SpriteRenderer>();
        SetModuleOfSameColor();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InformServerOfCarSkinServerRpc(string carID, string skinID, ulong clientID)
    {
        clientSkins.Add(clientID, Tuple.Create(carID, skinID));
        //Debug.Log("ServerRPC: " + carID + " " + skinID);
        UpdateSkinsClientRpc(carID, skinID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetCarSkinOfObjectServerRpc(ulong clientID)
    {
        if (clientSkins.ContainsKey(clientID))
        {
            UpdateSkinsClientRpc(clientSkins[clientID].Item1, clientSkins[clientID].Item2);
        }
    }

    [ClientRpc]
    private void UpdateSkinsClientRpc(string carID, string skinID, ClientRpcParams clientRpcParams = default)
    {
        Transform playerModule = transform.Find("PlayerModule");
        //Debug.Log("Client RPC called for instance " + playerModule.GetInstanceID() + " with " + carID + " " + skinID);

        playerModule.Find("DefultView").GetComponent<SpriteRenderer>().enabled = false;

        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.blue));
        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.green));
        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.red));
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

    private void SetModuleOfSameColor()
    {
        if (IsOwner)
        {
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
                sprit.enabled = sprit.transform.name.Contains("_" + colorLetter.ToString().ToUpper());
            }
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        AdjustSize();
        SetModuleOfSameColor();
    }

}
