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
    private GameObject guideFace;
    private Text msgText;
    private GameObject curtainImage;

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
        this.guideFace = GameObject.Find("GuideFace");
        this.msgText = GameObject.Find("MessageText").GetComponent<Text>();
        //this.curtainImage = GameObject.Find("CurtainImage");


        this.textureRender = GameObject.Find("ARFaceTrackController").GetComponent<TextureRenderWapper>();
        this.faceTrack = GameObject.Find("ModelRoot").GetComponent<ARCoreAugmentedFaceRig>();

        this.debugText1 = GameObject.Find("DebugText1").GetComponent<Text>();
        this.debugText2 = GameObject.Find("DebugText2").GetComponent<Text>();
        this.debugText3 = GameObject.Find("DebugText3").GetComponent<Text>();
        this.debugImg1 = GameObject.Find("DebugImage1").GetComponent<RawImage>();
        this.debugImg2 = GameObject.Find("DebugImage2").GetComponent<RawImage>();

        string model_filepath = Utils.getFilePath(MODEL_FILE_PATH);
        this.net = Dnn.readNetFromTensorflow(model_filepath);


        this.msgText.text = "ボタンを押して寮を決めよう！";

        //this.curtainImage.SetActive(false);
        this.guideFace.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Debug()
    {
    }

    public void Execute()
    {
        this.debugText1.text = "0";

        this.guideFace.SetActive(true);
        /*
        Texture2D t = Resources.Load("red_curtain") as Texture2D;
        this.curtainImage.GetComponent<RawImage>().texture = t;
        this.curtainImage.SetActive(true);
        */
        hogwartsHouse house = hogwartsHouse.Gryffindor;
        float value = 0.0f;

        bool isOk = Predict(ref house, ref value);

        this.msgText.text = house.ToString();
    }

    private enum hogwartsHouse
    {
        Gryffindor,
        Hufflpuff,
        Ravenclaw,
        Slytherin
    }

    private hogwartsHouse ConvertIdx(int idx)
    {
        hogwartsHouse house = hogwartsHouse.Gryffindor;

        if(idx == 0)
        {
            house = hogwartsHouse.Gryffindor;
        }
        else if(idx == 1)
        {
            house = hogwartsHouse.Hufflpuff;
        }
        else if(idx == 2)
        {
            house = hogwartsHouse.Ravenclaw;
        }
        else if(idx == 3)
        {
            house = hogwartsHouse.Slytherin;
        }

        return house;

    }

    private bool Predict(ref hogwartsHouse house, ref float value)
    {
        bool ret = false;
        bool isStartOk = false;

        this.debugText1.text = "1";

        if (this.textureRender.GetIsCameraCaptureOk())
        {
            if (this.faceTrack.GetIsFaceTrackOk())
            {
                isStartOk = true;
            }
        }

        if (isStartOk)
        {
            this.debugText1.text = "2";
            Mat faceImg = new Mat(IMG_WIDTH, IMG_HEIGHT, CvType.CV_8UC3);
            bool isOk = GetFaceImg(faceImg);
            if (isOk == false) return false;

            this.debugText1.text = "3";
            Mat inputData = new Mat(faceImg.width(), faceImg.height(), CvType.CV_32FC3);
            ConvertToInputData(faceImg, inputData);

            this.debugText1.text = "4";
            Mat blob = Dnn.blobFromImage(inputData);
            if (blob == null) return false;

            this.debugText1.text = "5";
            net.setInput(blob);
            Mat prob = net.forward();
            if (prob == null) return false;

            this.debugText1.text = "6";
            int idx = 0;
            (idx, value) = get_max_idx(prob);
            house = ConvertIdx(idx);
            this.debugText3.text = "idx : " + idx + " , value : " + value.ToString("F2");

            this.debugText1.text = "7";
            Texture2D t = new Texture2D(faceImg.cols(), faceImg.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(faceImg, t);
            this.debugImg2.texture = t;

            faceImg.Dispose();
            inputData.Dispose();

            ret = true;
        }

        return ret;
    }

    private bool GetFaceImg(Mat faceImg)
    {
        Texture2D texture = this.textureRender.FrameTexture;
        if (texture == null) return false;

        Mat srcImg = new Mat(texture.height, texture.width, CvType.CV_8UC3);
        Utils.texture2DToMat(texture, srcImg);

        Core.rotate(srcImg, srcImg, Core.ROTATE_90_COUNTERCLOCKWISE);
        
        OpenCVForUnity.CoreModule.Rect faceRect = this.GetFaceRectInImage(srcImg.height(), srcImg.width());
        if ((faceRect.x < 0) || (faceRect.y < 0)) return false;
        if (srcImg.width() < (faceRect.x + faceRect.width)) return false;
        if (srcImg.height() < (faceRect.y + faceRect.height)) return false;

        Mat img = new Mat(srcImg, faceRect);

        Imgproc.resize(img, faceImg, new Size(IMG_WIDTH, IMG_HEIGHT));

        srcImg.Dispose();
        img.Dispose();

        return true;
    }

    private void ConvertToInputData(Mat input, Mat output)
    {
        for (int h = 0; h < input.height(); h++)
        {
            for (int w = 0; w < input.width(); w++)
            {
                double[] p = input.get(w, h);
                double b = p[0];
                double g = p[1];
                double r = p[2];

                double[] pp = new double[] { b / 255.0, g / 255.0, r / 255.0 };
                output.put(w, h, pp);
            }
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


    Texture2D CreateReadabeTexture2D(Texture2D texture2d)
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
        }
        return (max_idx, max_value);
    }
}
