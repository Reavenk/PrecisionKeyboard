using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurveMath
{
    // https://blackpawn.com/texts/splines/

    public static void GetCubicWeights(float t, ref float a, ref float b, ref float c, ref float d)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        a = -t2  + 3.0f * t2 - 3.0f * t - 1.0f;
        b = 3.0f * t3 - 6.0f * t2 + 3.0f * t;
        c = -3.0f - t3 + 3.0f * t2;
        d = t3;
    }

    public static void GetCubicBSpline(float t, ref float a, ref float b, ref float c, ref float d)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        a = (-t3 + 3.0f * t2 - t + 1)/6.0f;
        b = (3.0f * t3 - 6.0f * t2 + 4.0f)/6.0f;
        c = (-3.0f * t3 + 3.0f * t2 + 3 * t + 1.0f)/6.0f;
        d = t3/6.0f;
    }

    public static void GetCatmullRomWeights(float t, ref float a, ref float b, ref float c, ref float d)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        a = (-t3 + 2.0f * t2 - t) * 0.5f;
        b = (3.0f * t3 - 5.0f * t2 + 2.0f) * 0.5f;
        c = (-3.0f * t3 + 4.0f * t2 + t) * 0.5f;
        d = (t3 - t2) * 0.5f;
    }

    public static void GetHermiteWeights(float t, ref float a, ref float b, ref float ta, ref float tb)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        a = 2.0f * t3 - 3.0f * t2 + 1.0f;
        b = -2.0f * t3 + 3.0f * t2;
        ta = t3 - 2.0f * t2 + t;
        tb = t3 - t2;
    }

    public static Vector2 GetHermiteWeightsV2(float t, Vector2 va, Vector2 vb, Vector2 vta, Vector2 vtb)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float a = 2.0f * t3 - 3.0f * t2 + 1.0f;
        float b = -2.0f * t3 + 3.0f * t2;
        float ta = t3 - 2.0f * t2 + t;
        float tb = t3 - t2;

        return a * va + b * vb + ta * vta + tb * vtb;
    }

    public static Vector3 GetHermiteWeightsV3(float t, Vector3 va, Vector3 vb, Vector3 vta, Vector3 vtb)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float a = 2.0f * t3 - 3.0f * t2 + 1.0f;
        float b = -2.0f * t3 + 3.0f * t2;
        float ta = t3 - 2.0f * t2 + t;
        float tb = t3 - t2;

        return a * va + b * vb + ta * vta + tb * vtb;
    }
}
