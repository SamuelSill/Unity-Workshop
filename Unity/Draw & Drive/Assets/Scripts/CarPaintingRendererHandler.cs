using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPaintingRendererHandler : MonoBehaviour
{
    // float timer = 0.0f;


    TrailRenderer TrailRenderer;
    // Start is called before the first frame update
    void Start()
    {
        TrailRenderer = GetComponent<TrailRenderer>();
        TrailRenderer.material.color = Color.red;

    }

    // Update is called once per frame
    void Update()
    {
        //test
        //timer += Time.deltaTime;
        //int seconds = (int)timer % 60;
        //if (timer > 5)
        //  TrailRenderer.material.color = Color.blue;
    }

}
