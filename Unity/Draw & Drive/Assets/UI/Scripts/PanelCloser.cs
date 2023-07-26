using UnityEngine;

public class PanelCloser : MonoBehaviour
{
    private bool isPanelOpen;

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
                PanelClosed();
            }
        }
    }

    public void KeepPanelOpen()
    {
        PanelOpened();
        isPanelOpen = false;
    }

    public void StopKeepingPanelOpen()
    {
        isPanelOpen = true;
    }

    public void PanelOpened()
    {
        gameObject.SetActive(true);
        isPanelOpen = true;
    }

    public void PanelClosed()
    {
        gameObject.SetActive(false);
        isPanelOpen = false;
    }
}
