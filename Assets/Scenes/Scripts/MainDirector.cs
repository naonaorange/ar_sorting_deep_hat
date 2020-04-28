using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using GoogleARCore.Examples.AugmentedFaces;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;

public class MainDirector : MonoBehaviour
{
    private Camera arCamera;
    private ARCoreAugmentedFaceRig faceTrack;
    private TextureRenderWapper textureRender;

    private Text logText1;
    private Text logText2;
    private RawImage camImg;

    private float timeSpan = 1.0f;
    private float timeDelta = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.arCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        this.textureRender = GameObject.Find("FaceTrackController").GetComponent<TextureRenderWapper>();
        this.faceTrack = GameObject.Find("fox_sample").GetComponent<ARCoreAugmentedFaceRig>();

        this.logText1 = GameObject.Find("LogText1").GetComponent<Text>();
        this.logText2 = GameObject.Find("LogText2").GetComponent<Text>();
        this.camImg = GameObject.Find("CameraImage").GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        this.timeDelta += Time.deltaTime;

        if(this.timeSpan < this.timeDelta)
        {
            this.timeDelta = 0.0f;
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
                UnityEngine.Rect faceRect = this.DetectFaceRectInScreen();

                Texture2D texture = this.textureRender.FrameTexture;
                if (texture == null) return;

                this.camImg.texture = texture;

                Mat srcImg = new Mat(texture.height, texture.width, CvType.CV_8UC3);
                Utils.texture2DToMat(texture, srcImg);

                this.logText1.GetComponent<Text>().text = srcImg.width() + "  " + srcImg.height() + "  " + srcImg.depth();
                double[] p = srcImg.get(0, 0);
                this.logText2.GetComponent<Text>().text = p[0].ToString("F2") + "  " + p[1].ToString("F2") + "  " + p[2].ToString("F2");

                /*
                Texture2D dispTexture = new Texture2D(srcImg.width(), srcImg.height());
                Utils.matToTexture2D(srcImg, dispTexture);

                //this.logText1.GetComponent<Text>().text = texture.width + "  " + texture.height;
                */

                srcImg.Dispose();
            }
        else
        {
            this.logText1.text = "";
        }

        }
    }

    private UnityEngine.Rect DetectFaceRectInScreen()
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

        float width = headRightPos.x - headLeftPos.x;
        float height = headLeftPos.y - nosePos.y;

        UnityEngine.Rect rect = new UnityEngine.Rect(   centerPos.x - width,
                                centerPos.y - height,
                                width,
                                height);
        return rect;
    }
}
