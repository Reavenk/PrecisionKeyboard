using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbToggle : ParamThumbBase
{
    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Image thumb;

    public ParamBool paramBool;

    public Color trueColor = Color.green;
    public Color falseColor = Color.red;

    public string trueText = "On";
    public string falseText = "Off";

    //private void OnGUI()
    //{
    //    if(GUILayout.Button("On") == true)
    //        this.SetTrue();
    //
    //    if(GUILayout.Button("Off") == true)
    //        this.SetFalse();
    //}

    public override bool SetParamBool(IWiringEditorBridge editorBridge, ParamBool pb)
    {
        base.SetParamBool(editorBridge, pb);

        this.paramBool = pb;
        this.UpdateDisplayValue();
        return true;
    }

    public override bool UpdateDisplayValue()
    { 
        if(this.paramBool == null)
        {
            this.thumb.color = Color.white;
            this.text.text = "--";
            return false;
        }

        if(this.paramBool.Value == true)
            this.SetTrue();
        else
            this.SetFalse();

        return true;
    }

    public void SetTrue()
    {
        this.thumb.color = this.trueColor;
        this.text.text = this.trueText;
        this.thumb.rectTransform.anchoredPosition = new Vector2(-5.8f, 0.0f);
    }

    public void SetFalse()
    {
        this.thumb.color = this.falseColor;
        this.text.text = this.falseText;
        this.thumb.rectTransform.anchoredPosition = new Vector2(5.8f, 0.0f);
    }

    public void SetValue(bool b)
    { 
        if(this.paramBool == null)
            return;

        this.paramBool.value = b;

        if(b == true)
            this.SetTrue();
        else
            this.SetFalse();

        this.editorBridge.FlagOutputNetworkDirty();
    }
}
