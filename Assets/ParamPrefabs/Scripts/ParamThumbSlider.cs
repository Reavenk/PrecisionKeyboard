using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbSlider : ParamThumbBase
{
    public ParamFloat paramFloat;
    public ParamInt paramInt;

    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Image thumb;


    public override bool SetParamFloat(IWiringEditorBridge editorBridge, ParamFloat pf)
    {
        base.SetParamFloat(editorBridge, pf);

        this.paramFloat = pf;
        this.UpdateDisplayValue();
        return true;
    }

    public override ParamFloat GetParamFloat()
    {
        return this.paramFloat;
    }

    public override bool SetParamInt(IWiringEditorBridge editorBridge, ParamInt pi)
    {
        base.SetParamInt( editorBridge, pi);

        this.paramInt = pi;
        this.UpdateDisplayValue();
        return true;
    }

    public override ParamInt GetParamInt()
    {
        return this.paramInt;
    }

    public override bool UpdateDisplayValue()
    {
        if(this.paramFloat != null)
        {
            this.SetDisplayValue(this.paramFloat.value);
            return true;
        }
        else if(this.paramInt != null)
        { 
            this.SetDisplayValue(this.paramInt.value);
            return true;
        }
        return false;
    }

    public void SetDisplayValue(float val)
    {
        text.text = val.ToString("0.00");
        float lam = Mathf.InverseLerp(this.paramFloat.min, this.paramFloat.max, val);
        float x = Mathf.Lerp(-14.5f, 14.5f, lam);
        thumb.rectTransform.anchoredPosition = new Vector2(x, 1.0f);
    }

    public void SetDisplayValue(int val)
    {
        text.text = val.ToString();
        float lam = Mathf.InverseLerp(this.paramInt.min, this.paramInt.max, val);
        float x = Mathf.Lerp(-14.5f, 14.5f, lam);
        thumb.rectTransform.anchoredPosition = new Vector2(x, 1.0f);
    }
}
