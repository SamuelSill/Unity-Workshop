using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerCustomisation : NetworkBehaviour
{
    public int defultBrushSize = 3;
    public int BrushSize { get; set; }
    private SpriteRenderer moduleSprit;
    private readonly Color blue = new Color(0.2f, 0.4f, 1);
    public  moduleColors currentColor = moduleColors.blue;
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
        Transform carModule = transform.Find("PlayerModule").GetChild(0);
        moduleSprit = carModule.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        adjustSize();
        moduleSprit.color = getColor();
    }
}
