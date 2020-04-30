using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;

public class MainDirector : MonoBehaviour
{
    private Text debugText1;
    private Text debugText2;
    private Text debugText3;
    private RawImage debugImage;

    //実行周期
    private float timeSpan = 5.0f;
    private float timeDelta = 0;

    private Net net;
    private const int IMG_WIDTH = 100;
    private const int IMG_HEIGHT = 100;
    private const string MODEL_FILE_PATH = "sorting_deep_hat.pb";

    // Start is called before the first frame update
    void Start()
    {
        this.debugText1 = GameObject.Find("DebugText1").GetComponent<Text>();
        this.debugText2 = GameObject.Find("DebugText2").GetComponent<Text>();
        this.debugText3 = GameObject.Find("DebugText3").GetComponent<Text>();
        this.debugImage = GameObject.Find("DebugImage").GetComponent<RawImage>();

        string model_filepath = Utils.getFilePath(MODEL_FILE_PATH);
        this.net = Dnn.readNetFromTensorflow(model_filepath);



        Texture2D texture = Resources.Load("harry") as Texture2D;
        this.debugText1.text = texture.width + "  " + texture.height;
        this.debugImage.texture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
