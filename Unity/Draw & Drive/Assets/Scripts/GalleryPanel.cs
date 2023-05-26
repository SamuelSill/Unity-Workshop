using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GalleryPanel : MonoBehaviour
{
    public GameObject paintingPrefab;
    public GameObject grid;
    public GameObject addPaintingPanel;

    public TMP_Text fileLocation;
    public TMP_InputField paintingName;
    public TMP_InputField description;

    private int width;
    private int height;

    // Start is called before the first frame update
    void Start()
    {
        width = (int)paintingPrefab.GetComponent<RectTransform>().rect.width;
        height = (int)paintingPrefab.GetComponent<RectTransform>().rect.height;
        StartCoroutine(GetPaintings());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [System.Serializable]
    public class JsonPainting
    {
        public string name;
        public List<int> data;
        public string description;
        public float difficulty;
    }

    [System.Serializable]
    class JsonPaintings
    {
        public JsonPainting[] paintings;
    }

    IEnumerator GetPaintings()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{LoginMenu.serverURL}/players/paintings?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            var paintings = JsonUtility.FromJson<JsonPaintings>($"{{\"paintings\": {getRequest.downloadHandler.text}}}");

            foreach (var painting in paintings.paintings)
            {
                Texture2D texture = new Texture2D(width, height);
                byte[] data = new byte[painting.data.Count];
                for (int index = 0; index < data.Length; index++)
                {
                    data[index] = (byte)painting.data[index];
                }

                texture.LoadImage(data);

                AddPaintingToView(texture, painting.name, painting.description, painting.difficulty);
            }
        }
    }

    void AddPaintingToView(Texture2D tex, string name, string description, float difficulty)
    {
        var newObject = Instantiate(paintingPrefab, grid.transform);

        newObject.GetComponent<Image>().sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        newObject.GetComponentInChildren<TMP_Text>().text = name + " " + difficulty.ToString();
    }

    public void OnSubmit()
    {
        if (paintingName.text.Length > 0 && description.text.Length > 0 && fileLocation.text.Length > 0)
        {
            StartCoroutine(AddPainting(paintingName.text, description.text, fileLocation.text));
        }
    }


    [System.Serializable]
    public class AddNewPainting
    {
        public string name;
        public List<int> data;
        public string description;
    }

    IEnumerator AddPainting(string paintingName, string description, string fileLocation)
    {
        if (File.Exists(fileLocation))
        {
            byte[] data = File.ReadAllBytes(fileLocation);
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(data);

            AddNewPainting painting = new AddNewPainting();
            painting.name = paintingName;
            painting.description = description;
            painting.data = new List<int>();
            foreach (byte b in data)
            {
                painting.data.Add(b);
            }

            var uwr = new UnityWebRequest($"{LoginMenu.serverURL}/players/paintings?username={LoginMenu.loggedUsername}&password={LoginMenu.loggedPassword}", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(painting));

            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.responseCode == 200)
            {
                addPaintingPanel.SetActive(false);

                AddPaintingToView(texture, painting.name, painting.description, float.Parse(System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data)));
            }
        }
    }


    public void Browse()
    {
        fileLocation.text = EditorUtility.OpenFilePanel("Painting File", "", "png");
    }
}
