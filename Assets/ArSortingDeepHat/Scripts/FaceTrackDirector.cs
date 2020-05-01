using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.AugmentedFaces;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;

public class FaceTrackDirector : MonoBehaviour
{
    private Camera arCamera;
    private ARCoreAugmentedFaceRig faceTrack;
    private TextureRenderWapper textureRender;

    public FaceTrackDirector()
    {
        this.arCamera = GameObject.Find("First Person Camera").GetComponent<Camera>();
        this.textureRender = GameObject.Find("ARFaceTrackController").GetComponent<TextureRenderWapper>();
        this.faceTrack = GameObject.Find("ModelRoot").GetComponent<ARCoreAugmentedFaceRig>();
    }

    public bool GetFaceTrackOk()
    {
        bool ret = false;

        if (this.textureRender.GetIsCameraCaptureOk())
        {
            if (this.faceTrack.GetIsFaceTrackOk())
            {
                ret = true;
            }
        }
        return ret;
    }

    public bool GetFaceImg(Mat faceImg)
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

        Imgproc.resize(img, faceImg, new Size(faceImg.width(), faceImg.height()));

        srcImg.Dispose();
        img.Dispose();

        return true;
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


}
