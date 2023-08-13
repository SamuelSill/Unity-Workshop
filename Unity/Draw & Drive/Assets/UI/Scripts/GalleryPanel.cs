using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

public class GalleryPanel : MonoBehaviour
{
    public GameObject paintingPrefab;
    public GameObject grid;
    public GameObject addPaintingPanel;

    public TMP_Text fileLocation;
    public TMP_InputField paintingName;
    public TMP_InputField description;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var painting in ServerSession.Paintings)
        {
            AddPaintingToView(painting);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddPaintingToView(ServerSession.Painting painting)
    {
        var newObject = Instantiate(paintingPrefab, grid.transform);

        newObject.GetComponent<Image>().sprite = Sprite.Create(
            painting.pngData,
            new Rect(0, 0, painting.pngData.width, painting.pngData.height),
            new Vector2(0.5f, 0.5f)
        );

        newObject.GetComponentInChildren<TMP_Text>().text = painting.name + " " + painting.difficulty.ToString();
    }

    public void OnSubmit()
    {
        if (paintingName.text.Length > 0 && description.text.Length > 0 && fileLocation.text.Length > 0)
        {
            ServerSession.UploadPainting(
                paintingName.text, 
                description.text, 
                fileLocation.text,
                AddPaintingToView
            );
        }
        else
        {
            PopupMessage.Display("Please fill all fields!");
        }
    }

	public void Browse()
    {
        addPaintingPanel.GetComponent<PanelCloser>().KeepPanelOpen();
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg"));
        FileBrowser.SetDefaultFilter(".jpg");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.ShowLoadDialog((path) => { 
            fileLocation.text = path[0];
            addPaintingPanel.GetComponent<PanelCloser>().StopKeepingPanelOpen();
        }, () => {
            addPaintingPanel.GetComponent<PanelCloser>().StopKeepingPanelOpen();
        }, FileBrowser.PickMode.Files);
    }
}
