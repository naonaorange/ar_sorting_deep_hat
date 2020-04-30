using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;
using System;

public class MainDirector : MonoBehaviour
{

    private Text debugText1;
    private Text debugText2;
    private Text debugText3;
    private RawImage debugImg;

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
        this.debugImg = GameObject.Find("DebugImage").GetComponent<RawImage>();

        try
        {
            string model_filepath = Utils.getFilePath(MODEL_FILE_PATH);
            this.net = Dnn.readNetFromTensorflow(model_filepath);
        }
        catch(Exception e)
        {
            Debug.Log(e);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Precict()
    {
        try
        {
            Texture2D srcTexture = Resources.Load("harry") as Texture2D;
            this.debugText1.text = srcTexture.width + "  " + srcTexture.height;

            Mat srcImg = new Mat(srcTexture.width, srcTexture.height, CvType.CV_8UC3);
            Utils.texture2DToMat(createReadabeTexture2D(srcTexture), srcImg);

            Mat inputImg = new Mat(srcImg.width(), srcImg.height(), CvType.CV_32FC3);
            for (int w = 0; w < inputImg.width(); w++)
            {
                for (int h = 0; h < inputImg.height(); h++)
                {
                    double[] p = srcImg.get(w, h);
                    double b = p[0];
                    double g = p[1];
                    double r = p[2];

                    double[] pp = new double[] { b / 255.0, g / 255.0, r / 255.0 };
                    inputImg.put(w, h, pp);
                }
            }
            this.debugText2.text = inputImg.width() + "  " + inputImg.height();


            Mat blob = Dnn.blobFromImage(inputImg);
            this.net.setInput(blob);
            Mat prob = net.forward();

            
            (int max_idx, float max_value) = get_max_idx(prob);
            this.debugText3.text = "idx : " + max_idx + " , value : " + max_value.ToString("F2");
            

            Texture2D dispTexture = new Texture2D(srcImg.cols(), srcImg.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(srcImg, dispTexture);
            this.debugImg.texture = dispTexture;
            //this.debugText3.text = "HOGE";
            
        }
        catch(Exception e)
        {
            Debug.Log(e);

        }

    }

    Texture2D createReadabeTexture2D(Texture2D texture2d)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
                    texture2d.width,
                    texture2d.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(texture2d, renderTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Texture2D readableTextur2D = new Texture2D(texture2d.width, texture2d.height);
        readableTextur2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        readableTextur2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readableTextur2D;
    }

    (int idx, float value) get_max_idx(Mat prob)
    {
        int max_idx = 0;
        float max_value = 0.0f;

        for (int i = 0; i < prob.width(); i++)
        {
            float tmp = (float)prob.get(0, i)[0];
            if (max_value < tmp)
            {
                max_value = tmp;
                max_idx = i;
            }
            Debug.Log(i + " : " + tmp.ToString("F4"));
        }
        return (max_idx, max_value);
    }
}
