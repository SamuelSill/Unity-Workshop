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
            material.mainTexture = textures[index];
            index++;
        }
    }
}
