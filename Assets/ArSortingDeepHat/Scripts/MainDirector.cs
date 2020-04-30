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
using GoogleARCore;
using GoogleARCore.Examples.AugmentedFaces;

public class MainDirector : MonoBehaviour
{
    private Camera arCamera;
    private ARCoreAugmentedFaceRig faceTrack;
    private TextureRenderWapper textureRender;

    private Text debugText1;
    private Text debugText2;
    private Text debugText3;
    private RawImage debugImg1;
    private RawImage debugImg2;

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
        this.arCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        this.textureRender = GameObject.Find("ARFaceTrackController").GetComponent<TextureRenderWapper>();
        this.faceTrack = GameObject.Find("fox_sample").GetComponent<ARCoreAugmentedFaceRig>();

        this.debugText1 = GameObject.Find("DebugText1").GetComponent<Text>();
        this.debugText2 = GameObject.Find("DebugText2").GetComponent<Text>();
        this.debugText3 = GameObject.Find("DebugText3").GetComponent<Text>();
        this.debugImg1 = GameObject.Find("DebugImage1").GetComponent<RawImage>();
        this.debugImg2 = GameObject.Find("DebugImage2").GetComponent<RawImage>();

        string model_filepath = Utils.getFilePath(MODEL_FILE_PATH);
        this.net = Dnn.readNetFromTensorflow(model_filepath);
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

            Mat blob = Dnn.blobFromImage(inputImg);
            this.net.setInput(blob);
            Mat prob = net.forward();
            
            (int max_idx, float max_value) = get_max_idx(prob);
            this.debugText2.text = "idx : " + max_idx + " , value : " + max_value.ToString("F2");
            
            Texture2D dispTexture = new Texture2D(srcImg.cols(), srcImg.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(srcImg, dispTexture);
            this.debugImg1.texture = dispTexture;
        }
        catch(Exception e)
        {
            Debug.Log(e);

        }
    }

    public void Predict2()
    {
        bool isProcessOk = false;

        if (this.textureRender.GetIsCameraCaptureOk())
        {
            if (this.faceTrack.GetIsFaceTrackOk())
            {
                isProcessOk = true;
            }
        }

        if (isProcessOk)
        {
            Texture2D srcTexture = this.textureRender.FrameTexture;
            if (srcTexture == null) return;

            Mat srcImg = new Mat(srcTexture.height, srcTexture.width, CvType.CV_8UC3);
            Utils.texture2DToMat(srcTexture, srcImg);

            //Mat dispImg = new Mat(texture.height, texture.width, CvType.CV_8UC3);
            //Core.rotate(srcImg, dispImg, Core.ROTATE_90_COUNTERCLOCKWISE);

            Core.rotate(srcImg, srcImg, Core.ROTATE_90_COUNTERCLOCKWISE);
            
            OpenCVForUnity.CoreModule.Rect faceRect = this.GetFaceRectInImage(srcImg.height(), srcImg.width());

            //Imgproc.rectangle(dispImg, faceRect, new Scalar(0, 0, 200), 3, 4);
            //Texture2D dispTexture = new Texture2D(dispImg.width(), dispImg.height(), TextureFormat.RGBA32, false);
            //Utils.matToTexture2D(dispImg, dispTexture);

            Mat faceImg = new Mat(srcImg, faceRect);


            Mat srcImg2 = new Mat(IMG_WIDTH, IMG_HEIGHT, CvType.CV_8UC3);
            Size s = new Size(IMG_WIDTH, IMG_HEIGHT);
            Imgproc.resize(faceImg, srcImg2, s);

            Texture2D t = new Texture2D(srcImg2.cols(), srcImg2.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(srcImg2, t);
            this.debugImg2.texture = t;

            Mat inputMat = new Mat(srcImg2.width(), srcImg2.height(), CvType.CV_32FC3);

            for (int h = 0; h < srcImg2.height(); h++)
            {
                for (int w = 0; w < srcImg2.width(); w++)
                {
                    double[] p = srcImg2.get(w, h);
                    double b = p[0];
                    double g = p[1];
                    double r = p[2];

                    double[] pp = new double[] { b / 255.0, g / 255.0, r / 255.0 };
                    inputMat.put(w, h, pp);
                }
            }

            Mat blob = Dnn.blobFromImage(inputMat);
            net.setInput(blob);
            Mat prob = net.forward();

            (int max_idx, float max_value) = get_max_idx(prob);
            this.debugText3.text = "idx : " + max_idx + " , value : " + max_value.ToString("F2");

            srcImg.Dispose();
            srcImg2.Dispose();
            faceImg.Dispose();
            inputMat.Dispose();
        }
        else
        {
        }
    }


    private OpenCVForUnity.CoreModule.Rect GetFaceRectInImage(int height, int width)
    {
        var rect = DetectFaceRect();

        float wScale = (float)width / Screen.width;
        float hScale = (float)height / Screen.height;

        float x = rect.x * wScale;
        //画像をX軸で反転させる
        //Unityの左下が[0, 0]の座標系から、OpenCVの左上が[0, 0]の座標系に変換する
        float y = (Screen.height - rect.y) * hScale;
        float w = rect.width * wScale;
        float h = rect.height * hScale;

        return new OpenCVForUnity.CoreModule.Rect((int)x, (int)y, (int)w, (int)h);
    }

    private OpenCVForUnity.CoreModule.Rect DetectFaceRect()
    {
        var faceTrackComp = this.faceTrack.GetComponent<ARCoreAugmentedFaceRig>();
        var arCameraComp = this.arCamera.GetComponent<Camera>();

        Vector3 centerPos = faceTrackComp.GetFacePosition();
        Vector3 nosePos = faceTrackComp.GetFaceRegionPosition(AugmentedFaceRegion.NoseTip);
        Vector3 headLeftPos = faceTrackComp.GetFaceRegionPosition(AugmentedFaceRegion.ForeheadLeft);
        Vector3 headRightPos = faceTrackComp.GetFaceRegionPosition(AugmentedFaceRegion.ForeheadRight);

        centerPos = arCameraComp.WorldToScreenPoint(centerPos);
        nosePos = arCameraComp.WorldToScreenPoint(nosePos);
        headLeftPos = arCameraComp.WorldToScreenPoint(headLeftPos);
        headRightPos = arCameraComp.WorldToScreenPoint(headRightPos);

        //ARCoreで検出した顔範囲からの範囲拡大倍率
        float scale = 1.0f;

        float width = (headRightPos.x - headLeftPos.x) * scale;
        float height = ((headLeftPos.y - nosePos.y) * 2) * scale;

        return new OpenCVForUnity.CoreModule.Rect((int)(centerPos.x - height / 2),
                                                    (int)(centerPos.y + height / 2),
                                                    (int)height,
                                                    (int)height);
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
