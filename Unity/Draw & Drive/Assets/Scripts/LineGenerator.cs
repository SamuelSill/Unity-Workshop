using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MonoBehaviour
{
    public Color lineColor;

    public GameObject linePrefab;

    Line activeLine;

    int colorMash = 0;

    Rigidbody2D carRigidbody2D;
    private void Awake()
    {
        lineColor = Color.blue;

        carRigidbody2D = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update()
    {
        
        //Make new Line
        if (activeLine == null)
        {
            colorMash++;
            GameObject newLine = Instantiate(linePrefab);
            activeLine = newLine.GetComponent<Line>();
        }
        //update Line
        if(activeLine != null)
        {
             Vector2 carPosition = carRigidbody2D.position;
            carPosition.x += 0.1f;
            activeLine.UpdateLine(carPosition, lineColor);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("Line")) {
          //  lineColor = Color.yellow;
          //  activeLine = null;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("Line"))
        {
         //   lineColor = Color.blue;
          //  activeLine = null;
        }
    }
}
