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

    private GameObject guideFace;
    private Text msgText;
    private GameObject curtainImage;

    private SortingDeepHat hat;
    private FaceTrackDirector faceTrack;

    private Text debugText1;
    private Text debugText2;
    private Text debugText3;
    private RawImage debugImg1;
    private RawImage debugImg2;

    //実行周期
    private float timeSpan = 5.0f;
    private float timeDelta = 0;


    // Start is called before the first frame update
    void Start()
    {

        this.guideFace = GameObject.Find("GuideFace");
        this.msgText = GameObject.Find("MessageText").GetComponent<Text>();
        //this.curtainImage = GameObject.Find("CurtainImage");


        this.debugText1 = GameObject.Find("DebugText1").GetComponent<Text>();
        this.debugText2 = GameObject.Find("DebugText2").GetComponent<Text>();

        this.debugText3 = GameObject.Find("DebugText3").GetComponent<Text>();
        this.debugImg1 = GameObject.Find("DebugImage1").GetComponent<RawImage>();
        this.debugImg2 = GameObject.Find("DebugImage2").GetComponent<RawImage>();

        this.hat = new SortingDeepHat();
        this.faceTrack = new FaceTrackDirector();

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
        
        SortingDeepHat.hogwartsHouse house = SortingDeepHat.hogwartsHouse.Gryffindor;
        bool isOk = Predict(ref house);

        this.msgText.text = house.ToString();
    }


    private bool Predict(ref SortingDeepHat.hogwartsHouse house)
    {
        bool ret = false;

        bool isStartOk = this.faceTrack.GetFaceTrackOk();

        if (isStartOk)
        {
            Mat faceImg = new Mat(SortingDeepHat.IMG_WIDTH, SortingDeepHat.IMG_HEIGHT, CvType.CV_8UC3);
            bool isOk = this.faceTrack.GetFaceImg(faceImg);
            if (isOk == false) return false;

            Mat inputData = new Mat(faceImg.width(), faceImg.height(), CvType.CV_32FC3);
            this.hat.PreProcess(faceImg, inputData);

            SortingDeepHat.hogwartsHouse h = SortingDeepHat.hogwartsHouse.Gryffindor;
            float value = 0.0f;
            this.hat.Sort(inputData, ref h, ref value);
            house = h;
            this.msgText.text = house.ToString();

            Texture2D t = new Texture2D(faceImg.cols(), faceImg.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(faceImg, t);
            this.debugImg2.texture = t;

            faceImg.Dispose();
            inputData.Dispose();

            ret = true;
        }

        return ret;
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


}
