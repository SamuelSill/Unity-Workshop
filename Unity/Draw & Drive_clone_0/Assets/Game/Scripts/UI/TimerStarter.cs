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
    Timer timer;
    bool adjusted;
    int delay = 30;
    public override void OnNetworkSpawn()
    {
        
        timer = GetComponent<Timer>();
        timer.StartTimer();
        adjusted = false;
    }
    private void FixedUpdate()
    {

        if (!adjusted)
        {
            if (PlayerOptions.PositionNetworkSpawned >= NetworkManegerUI.NUMBER_OF_PLAYERS && delay <= 0)
            {
                timer.StartTimer();
                runClientsClocksClientRpc(Time.time); 
                timer.onTimerEnd.AddListener(GameEnded);
                 adjusted = true;
            }
            delay--;

        }
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


    private float CompareTexturesByColorPercentage(Texture2D tex1, Texture2D tex2)
    {
        int matchingPixels = 0;
        int totalPixels = tex1.width * tex1.height;

        // Loop through each pixel of the textures
        for (int x = 0; x < tex1.width; x++)
        {
            for (int y = 0; y < tex1.height; y++)
            {
                // Get the color of each pixel from the textures
                Color color1 = tex1.GetPixel(x, y);
                Color color2 = tex2.GetPixel(x, y);

                // Compare the colors of the pixels
                if (color1 == color2)
                {
                    matchingPixels++;
                }
            }
        }

        // Calculate the percentage of matching pixels
        float similarityPercentage = (float)matchingPixels / totalPixels;

        return similarityPercentage;
    }
}

