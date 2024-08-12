using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbRoller : ParamThumbBase
{
    public ParamInt paramInt;
    public ParamFloat paramFloat;

    public UnityEngine.UI.Image thumb;

    //float f = 0.0f;
    //private void OnGUI()
    //{
    //    float of = f;
    //    f = GUILayout.HorizontalSlider(f, 0.0f, 1.0f, GUILayout.Width(200.0f));
    //    if(f != of)
    //        this.SetDisplayValue(f);
    //}

    void SetDisplayValue(float val)
    { 
        float x = Mathf.Lerp(-15.0f, 15.0f, val);

        float ylam = -Mathf.Pow(val * 2.0f - 1.0f, 2.0f) + 1.0f;
        float y = Mathf.Lerp(-0.2f, 3.4f, ylam);

        this.thumb.rectTransform.anchoredPosition = new Vector2(x, y);
    }
}
