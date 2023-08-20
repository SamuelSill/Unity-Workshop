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

    Dictionary<ulong, int> clientpallets;

    public override void OnNetworkSpawn()
    {
        
        PlayerCustomisation = GetComponent<PlayerCustomisation>();
        if (IsServer && clientpallets == null)
        {
            clientpallets = new Dictionary<ulong, int>();
        }

        if (IsOwner)
        {
            int teamID = 0;
            if (ServerSession.CurrentTeam.Equals("right"))
            {
                teamID = 1;
            }
            if (IsHost && gameObject.name.Contains("Mobile") && ServerSession.EnemyLobbyPlayers.ContainsKey(gameObject.GetComponent<PlayerOptions>().UserName))
            {
                    // On enemy canvas (not the host canvas)
                    teamID = (teamID + 1) % 2;
            }
            //if(gameObject.name.Contains("Mobile")) {}
            InformServerOfCarSkinServerRpc(OwnerClientId, teamID);
        }
        else
        {
            GetCarSkinOfObjectServerRpc(OwnerClientId);
        }

        objectChildren = new List<Transform>();
        foreach (string name in ChildrenObjectNames)
        {
            Transform objectChild = transform.Find(name);
            if (objectChild != null)
            {
                objectChildren.Add(objectChild);
            }
        }
        //Debug.Log("objectChildren: " + objectChildren.Count);
        if (!IsServer)
        {
            return;
        }

        if (pallet == null)
        {
            pallet = palletObject[0].GetComponent<PaintCanvas>();
        }

        var data = palletObject[0].GetComponent<PaintCanvas>().GetAllTextureData();
        var zippeddata = data.Compress();

        SendFullTextureClientRpc(zippeddata);

        data = palletObject[1].GetComponent<PaintCanvas>().GetAllTextureData();
        zippeddata = data.Compress();

        SendFullTextureToRightClientRpc(zippeddata);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InformServerOfCarSkinServerRpc(ulong clientID, int teamID)
    {

        clientpallets.Add(clientID, teamID);
        UpdateSkinsClientRpc(teamID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetCarSkinOfObjectServerRpc(ulong clientID)
    {
        if (clientpallets.ContainsKey(clientID))
        {
            UpdateSkinsClientRpc(clientpallets[clientID]);
        }
    }

    [ClientRpc]
    private void UpdateSkinsClientRpc(int teamID)
    {
        pallet = palletObject[teamID].GetComponent<PaintCanvas>();
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
        //Debug.Log("player position: " + worldPosition);
        Vector3 localPosition = pallet.transform.InverseTransformPoint(worldPosition);
        //Debug.Log("local player position: " + localPosition +" width: "+ textureWidth+" hieght: "+ textureHeight);
        Vector2 pixelUV = new Vector2(localPosition.x + 0.5f, localPosition.y + 0.5f);
        //pixelUV = new Vector2( 0.5f, 0.5f);
        //Debug.Log("pixelUV position: " + pixelUV);
        pixelUV.x *= textureWidth;
        pixelUV.y *= textureHeight;
        return pixelUV;
    }
    private void FixedUpdate()
    {
        if (!TimerStarter.GameStarted) {
            return;
        }
        //if (gameObject.name.Contains("Mobile"))
        //{

        //}
        foreach (Transform objectChild in objectChildren) {
            Vector3 playerPosition = objectChild.position;

            //Debug.Log("search PaintCanvas");
            if (pallet != null)
            {
                //Debug.Log("pallet possition" + pallet.transform.position);
                
                //Debug.Log("PaintCanvas Found");
                Renderer rend = pallet.GetComponent<Renderer>();
                MeshCollider meshCollider = pallet.GetComponent<MeshCollider>();

                if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                {
                    return;
                }
                    
            //Debug.Log("rend Found, meshCollider Found");
            Texture2D tex = rend.sharedMaterial.mainTexture as Texture2D;
                //Debug.Log("rend.material.mainTexture Found width= "+ tex.width+ " height="+ tex.height);
                Vector2 pixelUV = WorldToPixelUV(playerPosition, tex.width, tex.height);

                //Debug.Log("pixelUV position:" + pixelUV);
                CmdBrushAreaWithColorOnServer(pixelUV, PlayerCustomisation.getColor(), PlayerCustomisation.BrushSize);
                //BrushAreaWithColor(pixelUV, PlayerCustomisation.getColor(), PlayerCustomisation.BrushSize);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void BrushAreaWithColorOnServerRpc(Vector2 pixelUV, Color color, int size) {

        BrushAreaWithColorOnClientRpc(pixelUV, color, size);
       // BrushAreaWithColor(pixelUV, color, size);
    }

    private void CmdBrushAreaWithColorOnServer(Vector2 pixelUV, Color color, int size)
    {
        BrushAreaWithColorOnServerRpc(pixelUV, color, size);

    }

    [ClientRpc]
    private void BrushAreaWithColorOnClientRpc(Vector2 pixelUV, Color color, int size)
    {
        BrushAreaWithColor(pixelUV, color, size);
    }
    public float min = 0.01f;
    public float max = 0.04f;
    public float threshhold = 0.5f;
    private float mixingStrengthUp;
    private float mixingStrengthDown;
    public Color mixColors(Color ground_color, Color car_color)
    {
        if (ground_color.r > threshhold && ground_color.g > threshhold && ground_color.b > threshhold)
        {
            mixingStrengthUp = max;
            mixingStrengthDown = max;
        }
        else {
            mixingStrengthUp = min;
            mixingStrengthDown = min;
        }
        if (car_color == Color.blue) 
        {
            return new Color(Mathf.Max(ground_color.r - mixingStrengthDown * ground_color.r, 0), Mathf.Max(ground_color.g - mixingStrengthDown * ground_color.g, 0), Mathf.Min(ground_color.b + mixingStrengthUp * (1/(ground_color.b+0.01f)), 1));
        }
        if (car_color == Color.green) 
        {
            return new Color(Mathf.Max(ground_color.r - mixingStrengthDown * ground_color.r, 0), Mathf.Min(ground_color.g + mixingStrengthUp * (1 / (ground_color.g + 0.01f)), 1), Mathf.Max(ground_color.b - mixingStrengthDown * ground_color.b, 0));
        }
        return new Color(Mathf.Min(ground_color.r + mixingStrengthUp * (1 / (ground_color.r + 0.01f)), 1), Mathf.Max(ground_color.g - mixingStrengthDown * ground_color.g, 0), Mathf.Max(ground_color.b - mixingStrengthDown * ground_color.b, 0));
       
        // simple 8 colors
        //if (ground_color == Color.white)
        //    return car_color;
        //return new Color(Mathf.Max(ground_color.r, car_color.r), Mathf.Max(ground_color.g, car_color.g), Mathf.Max(ground_color.b, car_color.b));

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