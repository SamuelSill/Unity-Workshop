using UnityEngine;

public class PaintCanvas : MonoBehaviour
{
    public Texture2D Texture { get; private set; }
    private void InitTexture()
    {
        if (Texture == null)
        {
            Texture = (Texture2D)GameObject.Instantiate(GetComponent<Renderer>().sharedMaterial.mainTexture);
            GetComponent<Renderer>().sharedMaterial.mainTexture = Texture;
        }
    }

    public byte[] GetAllTextureData()
    {
        InitTexture();
        return Texture.GetRawTextureData();
    }

    internal void SetAllTextureData(byte[] textureData)
    {
        InitTexture();
        Texture.LoadRawTextureData(textureData);
        Texture.Apply();
    }
}