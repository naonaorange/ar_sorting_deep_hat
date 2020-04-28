using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using GoogleARCore.Examples.AugmentedFaces;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;

public class MainDirector : MonoBehaviour
{
    private Camera arCamera;
    private ARCoreAugmentedFaceRig faceTrack;
    private TextureRenderWapper textureRender;

    private Text logText1;
    private Text logText2;
    private Text logText3;
    private RawImage camImg;

    private float timeSpan = 3.0f;
    private float timeDelta = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.arCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        this.textureRender = GameObject.Find("FaceTrackController").GetComponent<TextureRenderWapper>();
        this.faceTrack = GameObject.Find("fox_sample").GetComponent<ARCoreAugmentedFaceRig>();

        this.logText1 = GameObject.Find("LogText1").GetComponent<Text>();
        this.logText2 = GameObject.Find("LogText2").GetComponent<Text>();
        this.logText3 = GameObject.Find("LogText3").GetComponent<Text>();
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

                Texture2D texture = this.textureRender.FrameTexture;
                if (texture == null) return;

                Mat srcImg = new Mat(texture.height, texture.width, CvType.CV_8UC3);
                Utils.texture2DToMat(texture, srcImg);

                /*
                Mat dispImg = new Mat(texture.height, texture.width, CvType.CV_8UC3);
                Core.rotate(srcImg, dispImg, Core.ROTATE_90_COUNTERCLOCKWISE);
                */
                Core.rotate(srcImg, srcImg, Core.ROTATE_90_COUNTERCLOCKWISE);

                OpenCVForUnity.CoreModule.Rect faceRect = this.DetectFaceRectInImage(srcImg.height(), srcImg.width());
                /*
                this.logText1.GetComponent<Text>().text = faceRect.ToString();
                this.logText2.GetComponent<Text>().text = Screen.width + "  " + Screen.height;
                this.logText3.GetComponent<Text>().text = arCamera.pixelWidth + "  " + arCamera.pixelHeight;
                Imgproc.rectangle(dispImg, faceRect, new Scalar(0, 0, 200), 3, 4);
                Texture2D dispTexture = new Texture2D(dispImg.width(), dispImg.height(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(dispImg, dispTexture);
                */

                Mat faceImg = new Mat(srcImg, faceRect);
                
                Texture2D dispTexture = new Texture2D(faceImg.width(), faceImg.height(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(faceImg, dispTexture);
                
                this.camImg.texture = dispTexture;

                srcImg.Dispose();
                //dispImg.Dispose();
                faceImg.Dispose();
            }
        else
        {
            this.logText1.text = "";
            this.logText2.text = "";
            this.logText3.text = "";
        }

        }
    }

    private OpenCVForUnity.CoreModule.Rect DetectFaceRectInImage(int height, int width)
    {
        var rect = DetectFaceRect();

        float wScale = (float)width / Screen.width;
        float hScale = (float)height / Screen.height;

        float x = rect.x * wScale;
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

        float scale = 1.0f;
        float width = (headRightPos.x - headLeftPos.x) * scale;
        float height = ((headLeftPos.y - nosePos.y) * 2) * scale;

        return new OpenCVForUnity.CoreModule.Rect(  (int)(centerPos.x - height / 2),
                                                    (int)(centerPos.y + height / 2),
                                                    (int)height,
                                                    (int)height);
    }

}
