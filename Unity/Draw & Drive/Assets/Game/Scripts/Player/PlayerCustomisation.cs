using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerCustomisation : NetworkBehaviour
{
    public int defultBrushSize = 3;
    public int BrushSize { get; set; }
    private SpriteRenderer moduleSprit;
    private SpriteRenderer[] playerModules;
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
        playerModules = transform.Find("PlayerModule").GetComponentsInChildren<SpriteRenderer>();
        SetMoudleOfSameColor();
    }

    private void SetMoudleOfSameColor() 
    {
        string colorLetter = currentColor.ToString().Substring(0,1);
        Debug.Log("letter : "+ colorLetter);
        foreach (SpriteRenderer sprit in playerModules)
        {
            if (sprit.transform.name.Contains("_" + colorLetter.ToUpper()))
            {
                sprit.enabled = true;
            }
            else {
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
