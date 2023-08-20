using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerCustomisation : NetworkBehaviour
{
    static Dictionary<ulong, Tuple<string, string, int>> clientCarStats;

    private int myThickness;

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
        BrushSize = (int)(myThickness * transform.localScale.x);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && !gameObject.name.Contains("Mobile") && IsOwner)
        {
            clientCarStats = new();
        }

        if (IsOwner)
        {
            if (gameObject.name.Contains("Mobile"))
            {
                var player = ServerSession.GetUser(gameObject.GetComponent<PlayerOptions>().UserName);
                var matchingPlayerCar = ServerSession.GameCars.Find(car => car.id.Equals(player.selected_car.id));

                InformServerOfCarStatsServerRpc(
                    player.selected_car.id,
                    player.selected_car.skins[player.selected_car.selected_skin],
                    matchingPlayerCar.thickness + player.selected_car.upgrades.thickness,
                    NetworkObjectId
                );
            }
            else
            {
                InformServerOfCarStatsServerRpc(
                    ServerSession.CurrentCar.id, 
                    ServerSession.CurrentSkin, 
                    ServerSession.CurrentCarThickness,
                    NetworkObjectId
                );
            }
        }
        else
        {
            GetCarStatsServerRpc(NetworkObjectId);
        }

        playerModules = transform.Find("PlayerModule").GetComponentsInChildren<SpriteRenderer>();
        SetModuleOfSameColor();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InformServerOfCarStatsServerRpc(
        string carID, 
        string skinID, 
        int thickness,
        ulong networkId
    )
    {
        clientCarStats.Add(networkId, Tuple.Create(carID, skinID, thickness));
        //Debug.Log("ServerRPC: " + carID + " " + skinID);
        UpdateSkinsClientRpc(carID, skinID, thickness);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetCarStatsServerRpc(ulong networkId)
    {
        if (clientCarStats.ContainsKey(networkId))
        {
            UpdateSkinsClientRpc(
                clientCarStats[networkId].Item1, 
                clientCarStats[networkId].Item2,
                clientCarStats[networkId].Item3
            );
        }
    }

    [ClientRpc]
    private void UpdateSkinsClientRpc(string carID, string skinID, int thickness)
    {
        Transform playerModule = transform.Find("PlayerModule");
        //Debug.Log("Client RPC called for instance " + playerModule.GetInstanceID() + " with " + carID + " " + skinID);

        playerModule.Find("DefultView").GetComponent<SpriteRenderer>().enabled = false;

        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.blue));
        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.green));
        CreateChildObject(playerModule.transform, CarSprites.GetCarSprite(carID, skinID, CarColor.red));

        myThickness = thickness;

        AdjustSize();
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
