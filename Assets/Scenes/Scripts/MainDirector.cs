using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using GoogleARCore.Examples.AugmentedFaces;

public class MainDirector : MonoBehaviour
{
    private GameObject arCamera;
    private GameObject faceTrack;

    private GameObject logText1;

    // Start is called before the first frame update
    void Start()
    {
        this.arCamera = GameObject.Find("First Person Camera");
        this.faceTrack = GameObject.Find("fox_sample");
        this.logText1 = GameObject.Find("LogText1");
    }

    // Update is called once per frame
    void Update()
    {
        var faceTrackComp = this.faceTrack.GetComponent<ARCoreAugmentedFaceRig>();
        var arCameraComp = this.arCamera.GetComponent<Camera>();

        if (faceTrackComp.GetIsFaceTrackOk())
        {
            Rect faceRect = this.DetectFaceRectInScreen();
            this.logText1.GetComponent<Text>().text = faceRect.ToString();
        }
        else
        {
            this.logText1.GetComponent<Text>().text = "";
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
