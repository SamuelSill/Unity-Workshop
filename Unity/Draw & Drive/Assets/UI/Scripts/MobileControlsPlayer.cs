using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MobileControlsPlayer : MonoBehaviour
{
    public VideoPlayer MobileControlls;
    public GameObject MobileControllsScreen;

    public void PlayMobileVideo()
    {
        MobileControllsScreen.SetActive(true);
        MobileControlls.Play();
    }
    public void CloseMobileVideo()
    {
        MobileControllsScreen.SetActive(false);
        MobileControlls.Stop();
    }
    private void OnDisable()
    {
        MobileControllsScreen.SetActive(false);
        MobileControlls.Stop();
    }
}
