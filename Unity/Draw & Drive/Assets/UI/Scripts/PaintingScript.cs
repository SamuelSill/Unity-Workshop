using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PaintingScript : MonoBehaviour
{
    public TMP_Text paintingName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeletePainting()
    {
        ServerSession.DeletePainting(paintingName.text, 
                                     () => Destroy(gameObject));
    }
}
