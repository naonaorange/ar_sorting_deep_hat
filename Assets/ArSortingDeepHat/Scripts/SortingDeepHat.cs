using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.DnnModule;

public class SortingDeepHat : MonoBehaviour
{
    private Net net;
    public const int IMG_WIDTH = 100;
    public const int IMG_HEIGHT = 100;

    private const string MODEL_FILE_PATH = "sorting_deep_hat.pb";

    public enum hogwartsHouse
    {
        Gryffindor,
        Hufflpuff,
        Ravenclaw,
        Slytherin
    }

    public SortingDeepHat()
    {
        LoadModel();
    }


    public void PreProcess(Mat input, Mat output)
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

    public bool Sort(Mat inputData, ref hogwartsHouse house, ref float value)
    {
        Mat blob = Dnn.blobFromImage(inputData);
        if (blob == null) return false;

        net.setInput(blob);
        Mat prob = net.forward();
        if (prob == null) return false;

        int idx = 0;
        (idx, value) = get_max_idx(prob);
        house = ConvertIdx(idx);

        return true;
    }

    private void LoadModel()
    {
        string model_filepath = Utils.getFilePath(MODEL_FILE_PATH);
        this.net = Dnn.readNetFromTensorflow(model_filepath);
    }

    private hogwartsHouse ConvertIdx(int idx)
    {
        hogwartsHouse house = hogwartsHouse.Gryffindor;

        if (idx == 0)
        {
            house = hogwartsHouse.Gryffindor;
        }
        else if (idx == 1)
        {
            house = hogwartsHouse.Hufflpuff;
        }
        else if (idx == 2)
        {
            house = hogwartsHouse.Ravenclaw;
        }
        else if (idx == 3)
        {
            house = hogwartsHouse.Slytherin;
        }

        return house;

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

