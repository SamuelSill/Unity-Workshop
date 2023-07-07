using System;
using TMPro;
using UnityEngine;

public class PopupMessage : MonoBehaviour
{
    public TMP_Text popupMessageText;

    private static float currentDuration;
    private static string currentMessage;

    public static void Display(string message, float duration = 2)
    {
        currentDuration = duration;
        currentMessage = message;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        currentDuration -= Math.Min(Time.deltaTime, currentDuration);
        if (currentDuration > 0)
        {
            popupMessageText.text = currentMessage;
        }
        else
        {
            popupMessageText.text = "";
        }
    }
}
