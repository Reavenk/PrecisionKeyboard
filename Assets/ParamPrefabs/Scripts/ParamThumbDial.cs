using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbDial : ParamThumbBase
{
    public ParamInt paramInt = null;
    public ParamFloat paramFloat = null;

    public UnityEngine.UI.Image needle = null;

    public override bool SetParamFloat(IWiringEditorBridge editorBridge, ParamFloat pf)
    {
        base.SetParamFloat(editorBridge, pf);

        this.paramFloat = pf;
        this.UpdateDisplayValue();
        return true;
    }

    public void SetDisplayValue(float lam)
    {
        float angle = Mathf.Lerp(135.0f, -135.0f, lam);
        needle.transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);
    }

    public override bool UpdateDisplayValue()
    { 
        if(this.paramFloat != null)
        {
            this.SetDisplayValue(this.paramFloat.value);
            return true;
        }
        return false;
    }

    public override ParamFloat GetParamFloat()
    {
        return this.paramFloat;
    }

    public override ParamInt GetParamInt()
    {
        return this.paramInt;
    }
}
