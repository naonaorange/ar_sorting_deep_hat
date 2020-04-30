using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainDirector : MonoBehaviour
{
    private Text debugText1;
    private Text debugText2;
    private Text debugText3;
    private RawImage debugImage;

    // Start is called before the first frame update
    void Start()
    {
        this.debugText1 = GameObject.Find("DebugText1").GetComponent<Text>();
        this.debugText2 = GameObject.Find("DebugText2").GetComponent<Text>();
        this.debugText3 = GameObject.Find("DebugText3").GetComponent<Text>();
        this.debugImage = GameObject.Find("DebugImage").GetComponent<RawImage>();


        Texture2D texture = Resources.Load("harry") as Texture2D;
        this.debugText1.text = texture.width + "  " + texture.height;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
