using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreBuild : MonoBehaviour
{
    [SerializeField]
    private List<Material> matials;
    [SerializeField]
    private List<Texture2D> textures;

    private void Awake()
    {
        int index = 0;
        foreach(Material material in matials) {
            if (material.mainTexture == null)
            {
                material.mainTexture = textures[index];
            }
            else
            {
                     SetTextureToColor((Texture2D)material.mainTexture, Color.white);
            }
            index++;
        }
    }
    private void SetTextureToColor(Texture2D texture, Color color)
    {
        // Get the width and height of the texture.
        int width = texture.width;
        int height = texture.height;

        // Create an array to store the color data for all pixels.
        Color32[] colors = new Color32[width * height];

        // Set all the pixels to the specified color.
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        // Apply the new color data to the texture.
        texture.SetPixels32(colors);
        texture.Apply();
    }

}

