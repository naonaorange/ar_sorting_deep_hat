using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.ComputerVision;
using System;

public class TextureRenderWapper : MonoBehaviour
{
    /// <summary>
    /// 取得するTextureのサイズの、カメラ画像に対する割合
    /// </summary>
    public float TextureSizeRatio = 1.0f;

    /// <summary>
    /// カメラ画像のデータ群
    /// </summary>
    private TextureReaderApi.ImageFormatType format;
    private int width;
    private int height;
    private IntPtr pixelBuffer;
    private int bufferSize = 0;

    /// <summary>
    /// カメラ画像取得用API
    /// </summary>
    private TextureReader TextureReader = null;


    /// <summary>
    /// カメラ画像のサイズに合わせてTextureReaderをセットしたかどうかのフラグ
    /// </summary>
    private bool setFrameSizeToTextureReader = false;
    // Start is called before the first frame update

    public void Awake()
    {
        // // カメラ画像取得時に呼ばれるコールバック関数を定義
        // TextureReader = GetComponent<TextureReader>();
        // TextureReader.OnImageAvailableCallback += OnImageAvailableCallbackFunc;
    }

    private void OnImageAvailableCallbackFunc(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize)
    {
        this.format = format;
        this.width = width;
        this.height = height;
        this.pixelBuffer = pixelBuffer;
        this.bufferSize = bufferSize;
    }



    void Start()
    {
        // AwakeからStartへ初期化を移動（TextureRender.csの初期化がStartのため）
        TextureReader = GetComponent<TextureReader>();
        TextureReader.OnImageAvailableCallback += OnImageAvailableCallbackFunc;
    }

    // Update is called once per frame
    void Update()
    {
        // TextureReaderにカメラ画像のサイズをセットする。実行は一回だけ
        if (!setFrameSizeToTextureReader)
        {
            using (var image = Frame.CameraImage.AcquireCameraImageBytes())
            {
                if (!image.IsAvailable)
                {
                    return;
                }

                TextureReader.ImageWidth = (int)(image.Width * TextureSizeRatio);
                TextureReader.ImageHeight = (int)(image.Height * TextureSizeRatio);
                TextureReader.Apply();

                setFrameSizeToTextureReader = true;
            }
        }
    }

    public bool GetIsCameraCaptureOk()
    {
        if (this.bufferSize == 0) return false;

        return true;
    }

    public bool isRed(float x, float y)
    {
        // グレイスケールが選択されているとき
        if (format == TextureReaderApi.ImageFormatType.ImageFormatGrayscale)
        {
            return false;
        }

        // TextureReaderが取得した画像データのポインタからデータを取得
        byte[] data = new byte[bufferSize];
        System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, data, 0, bufferSize);
        // 向きが270回転と反転しているので補正する
        byte[] correctedData = Rotate90AndFlip(data, width, height, format == TextureReaderApi.ImageFormatType.ImageFormatGrayscale);

        // サイズを合わせる
        int x_arg = (int)(x / 2.25);
        int y_arg = (int)(y / 3);
        int w_arg = height;
        int h_arg = width;
        int ch = 4;
        int th = 50;
        int idx = (y_arg * w_arg * ch) + (x_arg * ch);

        // 画素値を取得
        int r = correctedData[idx + 0];
        int g = correctedData[idx + 1];
        int b = correctedData[idx + 2];

        // 簡易的に red 検出
        int diff_rg = r - g;
        int diff_rb = r - b;
        if ((diff_rg > th) & (diff_rb > th))
        {
            return true;
        }
        return false;
    }

    public Texture2D FrameTexture
    {
        get
        {
            if (bufferSize != 0)
            {
                // TextureReaderが取得した画像データのポインタからデータを取得
                byte[] data = new byte[bufferSize];
                System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, data, 0, bufferSize);

                // 向きが270回転と反転しているので補正する
                byte[] correctedData = Rotate90AndFlip(data, width, height, format == TextureReaderApi.ImageFormatType.ImageFormatGrayscale);

                // Texture2Dを作成 90度回転させているのでwidth/heightを入れ替える
                //Texture2D _tex = new Texture2D(height, width, TextureFormat.RGBA32, false, false);
                Texture2D _tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);

                _tex.LoadRawTextureData(correctedData);
                _tex.Apply();

                return _tex;
            }
            else
            {
                return null;
            }
        }
    }

    private byte[] Rotate90AndFlip(byte[] img, int width, int height, bool isGrayscale)
    {
        int srcChannels = isGrayscale ? 1 : 4;
        int dstChannels = 4; //出力は常にRGBA32にする
        byte[] newImg = new byte[width * height * dstChannels];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int p = (i * width + j) * srcChannels;
                int np = ((width - j - 1) * height + (height - i - 1)) * dstChannels;

                for (int c = 0; c < dstChannels; c++)
                {
                    newImg[p + c] = img[p + c];
                }

                /*
                //imgのindex
                int p = (i * width + j) * srcChannels;

                //newImgに対するindex. 90度回転と反転を入れている
                int np = ((width - j - 1) * height + (height - i - 1)) * dstChannels;

                // グレースケールでもRGBで扱えるようにしておく
                if (isGrayscale)
                {
                    newImg[np] = img[p]; // R
                    newImg[np + 1] = img[p]; // G
                    newImg[np + 2] = img[p]; // B
                    newImg[np + 3] = 255; // A
                }
                else
                {
                    for (int c = 0; c < dstChannels; c++)
                    {
                        newImg[np + c] = img[p + c];
                    }
                }
                */
            }
        }

        return newImg;
    }
}
