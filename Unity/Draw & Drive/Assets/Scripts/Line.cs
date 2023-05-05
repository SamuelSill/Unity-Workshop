using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    List<Vector2> points;

     public void UpdateLine(Vector2 Position, Color lineColor)
    {
        //Set line color
        lineRenderer.material.color = lineColor;

        //make a list of points at start
        if (points == null)
        {
            points = new List<Vector2>();
            SetPoint(Position);
            return;
        }

        //add possition if it is a noticeable diffrents
        if (Vector2.Distance(points.Last(), Position) > .1f) 
        {
            SetPoint(Position);
        }

    }

    //add the point to the list
    void SetPoint(Vector2 point) 
    {
        points.Add(point);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);
    }
}
