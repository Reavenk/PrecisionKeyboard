using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamThumbFreq : ParamThumbBase
{
    public ParamTimeLen paramTimeLen = null;

    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Image unit;

    public Sprite icoHz;
    public Sprite icoSeconds;
    public Sprite icoFreqOct;
    public Sprite icoFreqMul;
    public Sprite icoBeatOct;
    public Sprite icoBeatMul;

    public override ParamTimeLen GetParamTimeLen()
    {
        return this.paramTimeLen;
    }

    public override bool SetParamTimeLen(IWiringEditorBridge editorBridge, ParamTimeLen ptl)
    {
        base.SetParamTimeLen(editorBridge, ptl);

        this.paramTimeLen = ptl;
        this.UpdateDisplayValue();
        return true;
    }

    public override bool UpdateDisplayValue()
    {
        this.SetDisplayValue();
        return true;
    }

    public void SetDisplayValue()
    {
        text.text = this.paramTimeLen.value.ToString("0.00");

        switch(this.paramTimeLen.freqTy)
        { 
            case ParamTimeLen.TimeLenType.Hertz:
                this.unit.sprite = this.icoHz;
                break;

            case ParamTimeLen.TimeLenType.Seconds:
                this.unit.sprite = this.icoSeconds;
                break;

            case ParamTimeLen.TimeLenType.FrequencyOctave:
                this.unit.sprite = this.icoFreqOct;
                break;

            case ParamTimeLen.TimeLenType.FrequencyMul:
                this.unit.sprite = this.icoFreqMul;
                break;

            case ParamTimeLen.TimeLenType.BeatOctave:
                this.unit.sprite = this.icoBeatOct;
                break;

            case ParamTimeLen.TimeLenType.BeatMul:
                this.unit.sprite = this.icoBeatMul;
                break;
        }
    }
}
