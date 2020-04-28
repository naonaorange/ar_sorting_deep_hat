using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using GoogleARCore.Examples.AugmentedFaces;

public class MainDirector : MonoBehaviour
{
    private Camera arCamera;
    private ARCoreAugmentedFaceRig faceTrack;
    private TextureRenderWapper textureRender;

    private Text logText1;
    private RawImage camImg;

    // Start is called before the first frame update
    void Start()
    {
        this.arCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        this.textureRender = GameObject.Find("FaceTrackController").GetComponent<TextureRenderWapper>();
        this.faceTrack = GameObject.Find("fox_sample").GetComponent<ARCoreAugmentedFaceRig>();

        this.logText1 = GameObject.Find("LogText1").GetComponent<Text>();
        this.camImg = GameObject.Find("CameraImage").GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
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
            Rect faceRect = this.DetectFaceRectInScreen();

            Texture2D texture = this.textureRender.FrameTexture;
            if (texture == null) return;

            this.camImg.texture = texture;

            this.logText1.GetComponent<Text>().text = faceRect.ToString();
        }
        else
        {
            this.logText1.text = "";
        }
    }

    private Rect DetectFaceRectInScreen()
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

        Rect rect = new Rect(   centerPos.x - width,
                                centerPos.y - height,
                                width,
                                height);
        return rect;
    }
}
