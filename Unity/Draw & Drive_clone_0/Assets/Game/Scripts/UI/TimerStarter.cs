using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

public class TimerStarter : NetworkBehaviour
{
    public static bool GameStarted = false;
    private Timer timer;
    //private int delay = 30;
    [SerializeField]
    private GameObject trafficLight;
    private float startTime;
    private float duration = 6f;
    

    public override void OnNetworkSpawn()
    {
        GameStarted = false;
         timer = GetComponent<Timer>();
        startTime = Time.time;
    }
    private void FixedUpdate()
    {
        if (!GameStarted)
        {
            float timePassed = Time.time - startTime;
            // Check if the desired duration has passed
            if (timePassed < duration || PlayerOptions.PositionNetworkSpawned < NetworkManegerUI.NUMBER_OF_PLAYERS)
            {
                if (IsServer)
                {
                    int index = Mathf.FloorToInt(timePassed / 2);
                    try
                    {
                        Transform childTransform = trafficLight.transform.GetChild(index);
                        childTransform.gameObject.SetActive(true);
                        activeTraficLightsClientRpc(index);
                    }
                    catch (Exception) { }
                }
                return;
            }
           
            trafficLight.SetActive(false);
            DisableTrafficLightsClientRpc();
            adjustTimer();
            GameStarted = true;
        }

    }
    [ClientRpc]
    private void activeTraficLightsClientRpc(int index)
    {

        Transform childTransform = trafficLight.transform.GetChild(index);
        childTransform.gameObject.SetActive(true);

    }
    [ClientRpc]
    private void DisableTrafficLightsClientRpc()
    {

        trafficLight.SetActive(false);
        GameStarted = true;

    }
    private void adjustTimer() { 
    
            if (PlayerOptions.PositionNetworkSpawned >= NetworkManegerUI.NUMBER_OF_PLAYERS) //&& delay <= 0)
            {
                timer.StartTimer();
                runClientsClocksClientRpc(Time.time); 
                timer.onTimerEnd.AddListener(GameEnded);
            }
            //delay--;

    }
    [ClientRpc]
    private void runClientsClocksClientRpc(float startTime) {
        
        timer.StartTimer();
        timer.onTimerEnd.AddListener(GameEnded);
    }
    private void GameEnded() {
        List<GameObject> gameTextures = GameObject.FindWithTag("Player").GetComponent<PlayerBrush>().getGameTextures();
        Image gameImage = GameObject.FindWithTag("GamePicture").GetComponent<Image>();
        float left, right;
        left = CompareTexturesByColorPercentage((Texture2D)gameImage.mainTexture, gameTextures[0].GetComponent<PaintCanvas>().Texture);
        right = CompareTexturesByColorPercentage((Texture2D)gameImage.mainTexture, gameTextures[1].GetComponent<PaintCanvas>().Texture);
        PostGameUiActions.UpdateScore(left, right);
        //ChangeSceneServerRpc();
        if (IsServer)
        {
            NetworkManager.SceneManager.LoadScene("PostGame", LoadSceneMode.Single);
              
        }
    }
    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    private float CompareTexturesByColorPercentage(Texture2D tex1, Texture2D tex2)
    {
        float rowPixels = 0;
        float totalPixelAvg = 0;
        //int matchingPixels = 0;
        //int totalPixels = tex1.width * tex1.height;
        tex2 = Resize(tex2, tex1.width, tex1.height);
        // Loop through each pixel of the textures
        
        for (int x = 0; x < tex1.width; x++)
        {
            for (int y = 0; y < tex1.height; y++)
            {
                // Get the color of each pixel from the textures
                Color color1 = tex1.GetPixel(x, y);
                Color color2 = tex2.GetPixel(x, y);
                // Compare the colors of the pixels

                // if (Mathf.Sqrt(Mathf.Pow(color1.r - color2.r, 2) + Mathf.Pow(color1.g - color2.g, 2) + Mathf.Pow(color1.b - color2.b, 2)) <= Mathf.Sqrt(2))
                // {
                //     matchingPixels++;
                //}

                if (!color1.Equals(Color.white) && !color2.Equals(Color.white))
                {
                    rowPixels += (Mathf.Abs(color1.r - color2.r) + Mathf.Abs(color1.g - color2.g) + Mathf.Abs(color1.b - color2.b)) / 3;
                }
                else {
                    //Pixel not Colored is scored 0.
                    rowPixels += 1;
                }
            }
            totalPixelAvg += rowPixels / tex1.height;
            rowPixels = 0;
        }

        // Calculate the percentage of matching pixels
        //float similarityPercentage = (float)matchingPixels / totalPixels;

        return (1-(totalPixelAvg / tex1.width)) * 100;
    }
}

