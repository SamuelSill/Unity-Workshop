using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCloser : MonoBehaviour
{
    private bool isPanelOpen;
    public GameObject panel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (isPanelOpen && Input.GetMouseButtonDown(0))
        {
            // Check if the mouse click position is outside the panel's RectTransform
            RectTransform panelRectTransform = GetComponent<RectTransform>();
            Vector2 mousePosition = Input.mousePosition;

            if (!RectTransformUtility.RectangleContainsScreenPoint(panelRectTransform, mousePosition))
            {
                panel.SetActive(false);
                isPanelOpen = false;
            }
        }
    }

    public void PanelOpened()
    {
        panel.SetActive(true);
        isPanelOpen = true;
    }
}
