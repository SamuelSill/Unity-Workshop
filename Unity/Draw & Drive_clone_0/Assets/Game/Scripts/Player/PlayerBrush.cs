using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerBrush : NetworkBehaviour
{
    [SerializeField]
    private List<GameObject> palletObject;

    private PaintCanvas pallet;
    private List<Transform> objectChildren;
    [SerializeField]
    private List<string> ChildrenObjectNames;
    private PlayerCustomisation PlayerCustomisation;
    public override void OnNetworkSpawn()
    {
        //Debug.Log("current server" + ServerSession.CurrentCar.id);
        //Debug.Log("current team" + ServerSession.CurrentTeam);
        //palletObject[0] = GameObject.Find("Paint Canvas Variant 1");
        //palletObject[1] = GameObject.Find("Paint Canvas Variant 2");
        PlayerCustomisation = GetComponent<PlayerCustomisation>();
        if (IsOwner && ServerSession.CurrentTeam.Equals("right"))
        {
            pallet = palletObject[1].GetComponent<PaintCanvas>();
            //Debug.Log("owner right " + OwnerClientId);
            changePlayerPalladServerRpc();
        }
        else
        {

            pallet = palletObject[0].GetComponent<PaintCanvas>();

        }

        objectChildren = new List<Transform>();
        foreach (string name in ChildrenObjectNames) 
        {
            Transform objectChild = transform.Find(name); 
            if (objectChild != null) {
                objectChildren.Add(objectChild);
            }
        }
        //Debug.Log("objectChildren: " + objectChildren.Count);
        if (!IsServer)
        {
            return;
        }
        var data = pallet.GetAllTextureData();
        var zippeddata = data.Compress();

       SendFullTextureClientRpc(zippeddata);

        //data = palletObject[1].GetComponent<PaintCanvas>().GetAllTextureData();
        //zippeddata = data.Compress();

        //SendFullTextureToRightClientRpc(zippeddata);
    }
    [ServerRpc(RequireOwnership = false)]
    private void changePlayerPalladServerRpc()
    {
        //Debug.Log("server right " + OwnerClientId);
        if (!IsClient)
        {
            pallet = palletObject[1].GetComponent<PaintCanvas>();
        }
        changePlayerPalladClientRpc();
    }
    [ClientRpc]
    private void changePlayerPalladClientRpc()
    {
        //Debug.Log("client right "+ OwnerClientId);
        pallet = palletObject[1].GetComponent<PaintCanvas>();
    }
    [ClientRpc]
    private void SendFullTextureClientRpc(byte[] textureData)
    {
        palletObject[0].GetComponent<PaintCanvas>().SetAllTextureData(textureData.Decompress());
        palletObject[1].GetComponent<PaintCanvas>().SetAllTextureData(textureData.Decompress());
    }
    [ClientRpc]
    private void SendFullTextureToRightClientRpc(byte[] textureData)
    {
        palletObject[1].GetComponent<PaintCanvas>().SetAllTextureData(textureData.Decompress());
    }

    private Vector2 WorldToPixelUV(Vector3 worldPosition, int textureWidth, int textureHeight)
    {
        Vector3 localPosition = pallet.transform.InverseTransformPoint(worldPosition);
        Vector2 pixelUV = new Vector2(localPosition.x + 0.5f, localPosition.y + 0.5f);
        pixelUV.x *= textureWidth; 
        pixelUV.y *= textureHeight;
        return pixelUV;
    }

    private void FixedUpdate()
    {
        if (PlayerOptions.PositionNetworkSpawned < NetworkManegerUI.NUMBER_OF_PLAYERS || !TimerStarter.GameStarted)
        {
            return;
        }
        foreach (Transform objectChild in objectChildren)
        {
            Vector3 playerPosition = objectChild.position;

            if (pallet != null)
            {
                Renderer rend = pallet.GetComponent<Renderer>();
                MeshCollider meshCollider = pallet.GetComponent<MeshCollider>();

                if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                    return;

                Texture2D tex = rend.sharedMaterial.mainTexture as Texture2D;
                Vector2 pixelUV = WorldToPixelUV(playerPosition, tex.width, tex.height);

                CmdBrushAreaWithColorOnServer(pixelUV, PlayerCustomisation.getColor(), PlayerCustomisation.BrushSize);
                BrushAreaWithColor(pixelUV, PlayerCustomisation.getColor(), PlayerCustomisation.BrushSize);
            }
        }
    }

    private void CmdBrushAreaWithColorOnServer(Vector2 pixelUV, Color color, int size)
    {
        BrushAreaWithColorOnServerRpc(pixelUV, color, size);
    }

    [ServerRpc(RequireOwnership = false)]
    private void BrushAreaWithColorOnServerRpc(Vector2 pixelUV, Color color, int size) 
    {
        BrushAreaWithColorOnClientRpc(pixelUV, color, size);
        BrushAreaWithColor(pixelUV, color, size);
    }

    [ClientRpc]
    private void BrushAreaWithColorOnClientRpc(Vector2 pixelUV, Color color, int size)
    {
        BrushAreaWithColor(pixelUV, color, size);
    }
    public Color mixColors(Color ground_color, Color car_color)
    {
        if (ground_color == Color.white)
            return car_color;
        return new Color(Mathf.Max(ground_color.r, car_color.r), Mathf.Max(ground_color.g, car_color.g), Mathf.Max(ground_color.b, car_color.b));//car_color;

    }
    private void BrushAreaWithColor(Vector2 pixelUV, Color color, int size)
    {
        //Debug.Log(OwnerClientId + "'s pallet = " + pallet.name);

        Texture2D texture = pallet.Texture;
        for (int x = -size; x < size; x++)
        {
            for (int y = -size; y < size; y++)
            {
                int pixelX = (int)pixelUV.x + x;
                int pixelY = (int)pixelUV.y + y;

                // Ensure the pixel coordinates are within the texture boundaries
                if (pixelX >= 0 && pixelX < texture.width && pixelY >= 0 && pixelY < texture.height)
                {
                    // Get the current pixel color
                    Color currentColor = texture.GetPixel(pixelX, pixelY);
                    // Mix the colors using Color.Lerp
                    Color mixedColor = mixColors(currentColor, color);
                    // Set the mixed color to the pixel
                    texture.SetPixel(pixelX, pixelY, mixedColor);
                }
            }
        }

        pallet.Texture.Apply();
    }
    public List<GameObject> getGameTextures() {
        return palletObject;
    }
}