using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamEditSlider : ParamEditBase
{
    string origStringValue = string.Empty;

    ParamFloat paramFloat = null;
    float origFloat = 0.0f;

    ParamInt paramInt = null;
    int origInt = 0;

    ParamTimeLen paramTimeLen = null;
    ParamTimeLen.Value origTime;

    ParamTimeLen.Value origHertz;
    ParamTimeLen.Value origSeconds;
    ParamTimeLen.Value origFreqOct;
    ParamTimeLen.Value origFreqMul;
    ParamTimeLen.Value origBeatOct;
    ParamTimeLen.Value origBeatMul;

    public UnityEngine.UI.Slider slider;
    public UnityEngine.UI.InputField input;
    public Sprite tickSprite;

    public UnityEngine.UI.Toggle toggleFreqOct;
    public UnityEngine.UI.Toggle toggleFreqMul;
    public UnityEngine.UI.Toggle toggleBeatOct;
    public UnityEngine.UI.Toggle toggleBeatMul;
    public UnityEngine.UI.Toggle toggleFreq;
    public UnityEngine.UI.Toggle toggleSeconds;

    public UnityEngine.UI.Toggle toggleMultiply;
    public UnityEngine.UI.Toggle toggleDivide;

    public UnityEngine.UI.Text descriptionText;
    public UnityEngine.UI.Text contextText;

    // A sempahore that diables updating the text input when 
    // the slider changes if true - this is needed because
    // if we edit the text, it can change the slider, which
    // can then change the text (if we didn't have this) and
    // interupt the user's editing.
    public bool sliderLock = false;

    public RectTransform typeArea;
    public UnityEngine.UI.Text unitsText;
    public UnityEngine.UI.Text typeText;

    // Sometimes the callback of toggles get set when we don't want to, we're just
    // reacting to anything, just initalizing something - and vibration is already
    // handled elsewhere in a more general manner.
    //
    // Only works if it's set to 0, it's a counter so multiple things can contribute
    // to suppressing the vibration. It's incremented on initialization.
    int doVibrate = -1;

    public override bool SetParam(ParamFloat pf)
    {
        this.paramFloat = pf;
        this.input.contentType = UnityEngine.UI.InputField.ContentType.DecimalNumber;
        this.UpdateInput();
        this.UpdateSlider();
        this.origFloat = pf.value;
        this.origStringValue = pf.GetStringValue();
        this.unitsText.text = pf.unit;
        this.typeArea.gameObject.SetActive(false);

        this.toggleMultiply.gameObject.SetActive(false);
        this.toggleDivide.gameObject.SetActive(false);

        this.descriptionText.gameObject.SetActive(false);
        this.contextText.gameObject.SetActive(false);

        ++this.doVibrate;
        return true;
    }

    public override bool SetParam(ParamInt pi)
    {
        this.paramInt = pi;
        this.input.contentType = UnityEngine.UI.InputField.ContentType.IntegerNumber;
        this.UpdateInput();
        this.UpdateSlider();
        this.origInt = pi.value;
        this.origStringValue = pi.GetStringValue();
        this.unitsText.text = pi.unit;
        this.typeArea.gameObject.SetActive(false);

        for(int i = pi.min; i <= pi.max; ++i)
        {
            float f = Mathf.InverseLerp((float)pi.min, (float)pi.max, (float)i);
            GameObject go = new GameObject("Tick");
            go.transform.SetParent(this.slider.transform, false);
            UnityEngine.UI.Image imgTick = go.AddComponent<UnityEngine.UI.Image>();
            imgTick.color = Color.white;
            imgTick.sprite = this.tickSprite;
            RectTransform rtTick = imgTick.rectTransform;
            rtTick.anchorMin = new Vector2(f, 1.0f);
            rtTick.anchorMax = new Vector2(f, 1.0f);
            rtTick.pivot = new Vector2(0.5f, 0.0f);

            float offx = Mathf.Lerp(25.0f, -25.0f, f);
            rtTick.anchoredPosition = new Vector2(offx, 10.0f);
            rtTick.sizeDelta = new Vector2(10.0f, 10.0f);
        }

        this.descriptionText.gameObject.SetActive(false);
        this.contextText.gameObject.SetActive(false);

        this.toggleMultiply.gameObject.SetActive(false);
        this.toggleDivide.gameObject.SetActive(false);

        ++this.doVibrate;
        return true;
    }

    public override bool SetParam(ParamTimeLen ptl) 
    { 
        this.paramTimeLen = ptl;
        this.input.contentType = UnityEngine.UI.InputField.ContentType.DecimalNumber;
        this.unitsText.text = ptl.unit;

        const float stdFreq = 440.0f;
        float bps = this.dlgRoot.application.BeatsPerSecond();
        float evFrq = ptl.GetFrequency(stdFreq, bps);
        this.origTime = ptl.GetValue();

        this.origStringValue = ptl.GetStringValue();

        this.origHertz      = ParamTimeLen.CalcValFromFrequency(origFloat, stdFreq, bps, ParamTimeLen.TimeLenType.Hertz);
        this.origSeconds    = ParamTimeLen.CalcValFromFrequency(origFloat, stdFreq, bps, ParamTimeLen.TimeLenType.Seconds);
        this.origFreqOct    = ParamTimeLen.CalcValFromFrequency(origFloat, stdFreq, bps, ParamTimeLen.TimeLenType.FrequencyOctave);
        this.origFreqMul    = ParamTimeLen.CalcValFromFrequency(origFloat, stdFreq, bps, ParamTimeLen.TimeLenType.FrequencyMul);
        this.origBeatOct    = ParamTimeLen.CalcValFromFrequency(origFloat, stdFreq, bps, ParamTimeLen.TimeLenType.BeatOctave);
        this.origBeatMul    = ParamTimeLen.CalcValFromFrequency(origFloat, stdFreq, bps, ParamTimeLen.TimeLenType.BeatMul);

        // The the unconverted pure value for what we can.
        switch (ptl.freqTy)
        { 
            case ParamTimeLen.TimeLenType.Hertz:
                this.origHertz = ptl.GetValue();
                break;

            case ParamTimeLen.TimeLenType.Seconds:
                this.origSeconds = ptl.GetValue();
                break;

            case ParamTimeLen.TimeLenType.FrequencyOctave:
                this.origFreqOct = ptl.GetValue();
                break;

            case ParamTimeLen.TimeLenType.FrequencyMul:
                this.origFreqMul = ptl.GetValue();
                break;

            case ParamTimeLen.TimeLenType.BeatOctave:
                this.origBeatOct = ptl.GetValue();

                break;

            case ParamTimeLen.TimeLenType.BeatMul:
                this.origBeatMul = ptl.GetValue();
                break;
        }

        if (ptl.mul == true)
            this.toggleMultiply.isOn = true;
        else
            this.toggleDivide.isOn = true;

        this.UpdateInput();
        this.UpdateSlider();

        this.UpdateUnits(true);

        ++this.doVibrate;
        return true; 
    }

    public void UpdateUnits(bool set)
    { 
        if(this.paramTimeLen != null)
        { 
            this.unitsText.text = this.paramTimeLen.unit;

            switch(this.paramTimeLen.freqTy)
            { 
                case ParamTimeLen.TimeLenType.Hertz:
                    if(set == true)
                        this.toggleFreq.isOn = true;
                    break;

                case ParamTimeLen.TimeLenType.Seconds:
                    if(set == true)
                        this.toggleSeconds.isOn = true;
                    break;

                case ParamTimeLen.TimeLenType.FrequencyOctave:
                    if(set == true)
                        this.toggleFreqOct.isOn = true;
                    break;

                case ParamTimeLen.TimeLenType.FrequencyMul:
                    if(set == true)
                        this.toggleFreqMul.isOn = true;
                    break;

                case ParamTimeLen.TimeLenType.BeatOctave:
                    if(set == true)
                        this.toggleBeatOct.isOn = true;
                    break;

                case ParamTimeLen.TimeLenType.BeatMul:
                    if(set == true)
                        this.toggleBeatMul.isOn = true;
                    break;
            }

            if(
                this.paramTimeLen.freqTy == ParamTimeLen.TimeLenType.BeatMul ||
                this.paramTimeLen.freqTy == ParamTimeLen.TimeLenType.FrequencyMul)
            { 
                this.toggleMultiply.gameObject.SetActive(true);
                this.toggleDivide.gameObject.SetActive(true);

                if(this.paramTimeLen.mul == true)
                    this.toggleMultiply.isOn = true;
                else
                    this.toggleDivide.isOn = true;
            }
            else
            {
                this.toggleMultiply.gameObject.SetActive(false);
                this.toggleDivide.gameObject.SetActive(false);
            }

            this.baseThumb.UpdateDisplayValue();
            this.UpdateDescription();
        }
        else
        { 
            this.typeArea.gameObject.SetActive(false);
            return;
        }

        this.typeArea.gameObject.SetActive(true);
    }

    public void OnToggle_Mul()
    { 
        if(this.paramTimeLen == null)
            return;

        this.paramTimeLen.mul = true;

        this.baseThumb.UpdateDisplayValue();
        this.UpdateDescription();

        this.Vibrate();
    }

    public void OnToggle_Div()
    { 
        if(this.paramTimeLen == null)
            return;

        this.paramTimeLen.mul = false;

        this.baseThumb.UpdateDisplayValue();
        this.UpdateDescription();

        this.Vibrate();
    }

    public void UpdateDescription()
    { 
        if(this.paramTimeLen == null)
            return;

        const string tsTrunc = "0.000";

        switch(this.paramTimeLen.freqTy)
        { 
            case ParamTimeLen.TimeLenType.Seconds:
                if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetFreq)
                { 
                    this.descriptionText.text = (1.0f / this.paramTimeLen.value).ToString(tsTrunc) + " hz.";
                    this.contextText.text = $"1.0 / {this.paramTimeLen.value.ToString(tsTrunc)} = {(1.0f / this.paramTimeLen.value).ToString("0.000")}";
                }
                else //if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetTime)
                {
                    this.descriptionText.text = this.paramTimeLen.value.ToString(tsTrunc) + " second(s).";
                    this.contextText.text = string.Empty;
                }
                break;

            case ParamTimeLen.TimeLenType.Hertz:
                if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetFreq)
                {
                    this.descriptionText.text = this.paramTimeLen.value.ToString(tsTrunc) + " hz.";
                    this.contextText.text = string.Empty;
                }
                else //if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetTime)
                { 
                    this.descriptionText.text = (1.0f/this.paramTimeLen.value).ToString(tsTrunc) + " seconds(s).";
                    this.contextText.text = $"1/{this.paramTimeLen.value} = {(1.0f / this.paramTimeLen.value).ToString("0.000")}";
                }
                break;

            case ParamTimeLen.TimeLenType.FrequencyMul:

                if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetFreq)
                {
                    if(this.paramTimeLen.mul == true)
                        this.descriptionText.text = $"note * { this.paramTimeLen.value.ToString(tsTrunc) } hz.";
                    else
                        this.descriptionText.text = $"note / { this.paramTimeLen.value.ToString(tsTrunc) } hz.";
                }
                else //if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetTime)
                {
                    if(this.paramTimeLen.mul == true)
                        this.descriptionText.text = $"1.0 / (note * { this.paramTimeLen.value.ToString(tsTrunc) }) second(s).";
                    else
                        this.descriptionText.text = $"1.0 / (note / { this.paramTimeLen.value.ToString(tsTrunc) }) second(s).";
                }
                
                this.contextText.text = string.Empty;
                break;

            case ParamTimeLen.TimeLenType.FrequencyOctave:

                if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetFreq)
                    this.descriptionText.text = $"note * {Mathf.Pow(2, this.paramTimeLen.value).ToString(tsTrunc)} hz";
                else
                    this.descriptionText.text = $"1.0 / (note * {Mathf.Pow(2, this.paramTimeLen.value).ToString(tsTrunc)}) seconds";

                this.contextText.text = $"POW(2.0, {this.paramTimeLen.value.ToString(tsTrunc)}) = {Mathf.Pow(2, this.paramTimeLen.value).ToString(tsTrunc)}";
                break;

            case ParamTimeLen.TimeLenType.BeatOctave:

                if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetFreq)
                    this.descriptionText.text = $"BPS * {Mathf.Pow(2, this.paramTimeLen.value).ToString(tsTrunc)} hz";
                else
                    this.descriptionText.text = $"1.0 / (BPS * {Mathf.Pow(2, this.paramTimeLen.value).ToString(tsTrunc)}) seconds";

                this.contextText.text = 
                    $"BPS = BPM / 60.0 = {this.dlgRoot.application.BeatsPerMinute().ToString(tsTrunc)} / 60.0 = {(this.dlgRoot.application.BeatsPerMinute() / 60.0f).ToString(tsTrunc)}\n"+
                    $"POW(2.0, {this.paramTimeLen.value.ToString(tsTrunc)}) = {Mathf.Pow(2, this.paramTimeLen.value).ToString(tsTrunc)}";
                break;

            case ParamTimeLen.TimeLenType.BeatMul:

                if(this.paramTimeLen.widgetType == ParamTimeLen.WidgetFreq)
                {
                    if(this.paramTimeLen.mul == true)
                        this.descriptionText.text = $"BPS * {this.paramTimeLen.value.ToString(tsTrunc)} hz";
                    else
                        this.descriptionText.text = $"BPS * {this.paramTimeLen.value.ToString(tsTrunc)} hz";
                }
                else
                {
                    if(this.paramTimeLen.mul == true)
                        this.descriptionText.text = $"1.0 / (BPS * {this.paramTimeLen.value.ToString(tsTrunc)}) seconds";
                    else
                        this.descriptionText.text = $"1.0 / (BPS / {this.paramTimeLen.value.ToString(tsTrunc)}) seconds";
                }

                this.contextText.text = $"BPS = BPM / 60.0 = {this.dlgRoot.application.BeatsPerMinute().ToString(tsTrunc)} / 60.0 = {(this.dlgRoot.application.BeatsPerMinute() / 60.0f).ToString(tsTrunc)}";
                break;
        }
    }

    public override bool Initialize(EditParamDlgRoot root, LLDNBase owner, ParamThumbBase baseThumb)
    { 
        bool ret = base.Initialize(root, owner, baseThumb);

        this.slider.onValueChanged.AddListener(
            (x)=>{ this.OnSlider_Changed(x); });

        this.UpdateSlider();
        return ret;
    }

    public void OnSlider_Changed(float x)
    { 
        if(this.paramFloat != null)
        {
            float val = 
                Mathf.Lerp(
                    this.paramFloat.min, 
                    this.paramFloat.max, 
                    x);

            this.paramFloat.value = val;
        }
        else if(this.paramInt != null)
        { 
            float f = Mathf.Lerp(
                this.paramInt.min,
                this.paramInt.max,
                x);

            int i = (int)Mathf.Round(f);
            this.paramInt.value = i;
            this.UpdateSlider();
        }
        else if(this.paramTimeLen != null)
        { 
            float val = x;

            switch(this.paramTimeLen.freqTy)
            {
                case ParamTimeLen.TimeLenType.Hertz:
                    val = Mathf.Lerp(ParamTimeLen.minFr, ParamTimeLen.maxFr, x);
                    break;

                case ParamTimeLen.TimeLenType.Seconds:
                    val = Mathf.Lerp(ParamTimeLen.minTime, ParamTimeLen.maxTime, x);
                    break;

                case ParamTimeLen.TimeLenType.FrequencyOctave:
                    val = Mathf.Lerp(ParamTimeLen.minFreqOct, ParamTimeLen.maxFreqOct, x);
                    break;

                case ParamTimeLen.TimeLenType.FrequencyMul:
                    val = Mathf.Lerp(ParamTimeLen.minFreqMul, ParamTimeLen.maxFreqMul, x);
                    break;

                case ParamTimeLen.TimeLenType.BeatOctave:
                    val = Mathf.Lerp(ParamTimeLen.minBeatOct, ParamTimeLen.maxBeatOct, x);
                    break;

                case ParamTimeLen.TimeLenType.BeatMul:
                    val = Mathf.Lerp(ParamTimeLen.minBeatMul, ParamTimeLen.maxBeatMul, x);
                    break;

                default:
                    // TODO: Error
                    break;
            }
            ParamTimeLen.Value v = this.paramTimeLen.GetValue();
            v.value = val;

            this.paramTimeLen.SetValue(v);

            this.UpdateDescription();
        }

        this.baseThumb.UpdateDisplayValue();

        if(this.sliderLock == false)
            this.UpdateInput();
    }

    public void UpdateSlider()
    {
        if(this.paramFloat != null)
        {
            float val = 
                Mathf.InverseLerp(
                    this.paramFloat.min, 
                    this.paramFloat.max, 
                    this.paramFloat.value);

            this.slider.value = val;
        }
        else if(this.paramInt != null)
        { 
            float val = 
                Mathf.InverseLerp(
                    (float)this.paramInt.min,
                    (float)this.paramInt.max,
                    (float)this.paramInt.value);

            this.slider.value = val;
        }
        else if(this.paramTimeLen != null)
        {
            float val = this.paramTimeLen.value;
            switch(this.paramTimeLen.freqTy)
            { 
                case ParamTimeLen.TimeLenType.Hertz:
                    val = Mathf.InverseLerp(ParamTimeLen.minFr, ParamTimeLen.maxFr, this.paramTimeLen.value);
                    break;

                case ParamTimeLen.TimeLenType.Seconds:
                    val = Mathf.InverseLerp(ParamTimeLen.minTime, ParamTimeLen.maxTime, this.paramTimeLen.value);
                    break;

                case ParamTimeLen.TimeLenType.FrequencyOctave:
                    val = Mathf.InverseLerp(ParamTimeLen.minFreqOct, ParamTimeLen.maxFreqOct, this.paramTimeLen.value);
                    break;

                case ParamTimeLen.TimeLenType.FrequencyMul:
                    val = Mathf.InverseLerp(ParamTimeLen.minFreqMul, ParamTimeLen.maxFreqMul, this.paramTimeLen.value);
                    break;

                case ParamTimeLen.TimeLenType.BeatOctave:
                    val = Mathf.InverseLerp(ParamTimeLen.minBeatOct, ParamTimeLen.maxBeatOct, this.paramTimeLen.value);
                    break;

                case ParamTimeLen.TimeLenType.BeatMul:
                    val = Mathf.InverseLerp(ParamTimeLen.minBeatMul, ParamTimeLen.maxBeatMul, this.paramTimeLen.value);
                    break;
            }

            this.slider.value = val;
            this.UpdateDescription();
        }
    }

    bool ignoreInputSem = false;
    public void UpdateInput()
    {
        if (this.paramFloat != null)
            input.text = this.paramFloat.value.ToString("0.000");
        else if(this.paramInt != null)
            input.text = this.paramInt.value.ToString();
        else if(this.paramTimeLen != null)
        {
            this.UpdateDescription();

            this.ignoreInputSem = true;
            input.text = this.paramTimeLen.value.ToString();
            this.ignoreInputSem = false;
        }

    }

    public void OnInput_ValueChanged()
    {
        if(this.ignoreInputSem == true)
            return;

        bool update = false;
        if (this.paramFloat != null)
        {
            float f;
            update = float.TryParse(this.input.text, out f);
            if ( update == true)
            {
                this.paramFloat.value =
                    Mathf.Clamp(
                        f,
                        this.paramFloat.min,
                        this.paramFloat.max);
            }
        }
        else if(this.paramInt != null)
        { 
            int n;
            update = int.TryParse(this.input.text, out n);
            if (update == true)
            {
                this.paramInt.value =
                    Mathf.Clamp(
                        n,
                        this.paramInt.min,
                        this.paramInt.max);
            }
        }
        else if(this.paramTimeLen != null)
        {
            float f;
            update = float.TryParse(this.input.text, out f);
            if (update == true)
            {
                ParamTimeLen.Value v = this.paramTimeLen.GetValue();
                v.value = f;

                this.paramTimeLen.SetValue(v);
                this.UpdateDescription();
            }
        }

        if(update == true)
        {
            this.sliderLock = true;
            this.UpdateSlider();
            this.baseThumb.UpdateDisplayValue();
            this.sliderLock = false;
        }
    }

    public void OnInput_EndEdit()
    {
        bool processed = false;
        if (this.paramFloat != null)
        {
            float f;
            processed = float.TryParse(this.input.text, out f);
            if (processed == true)
            {
                this.paramFloat.value =
                    Mathf.Clamp(
                        f,
                        this.paramFloat.min,
                        this.paramFloat.max);
            }
        }
        else if (this.paramInt != null)
        {
            int n;
            processed = int.TryParse(this.input.text, out n);
            if (processed == true)
            {
                this.paramInt.value =
                    Mathf.Clamp(
                        n,
                        this.paramInt.min,
                        this.paramInt.max);
            }
        }

        if (processed == true)
        {
            if (Input.GetKeyDown(KeyCode.Return) == true)
                this.dlgRoot.CloseDialog();

            this.UpdateSlider();
            this.baseThumb.UpdateDisplayValue();
            this.UpdateInput();
        }
        else
            this.UpdateInput();
    }

    void SetTimeLenType(ParamTimeLen.TimeLenType ft)
    {
        if(this.paramTimeLen.freqTy == ft)
            return;

        // Store the value for the current type in case they switch back
        switch(this.paramTimeLen.freqTy)
        { 
            case ParamTimeLen.TimeLenType.Hertz:
                this.origHertz = this.paramTimeLen.GetValue();
                break;

            case ParamTimeLen.TimeLenType.Seconds:
                this.origSeconds = this.paramTimeLen.GetValue();
                break;

            case ParamTimeLen.TimeLenType.FrequencyOctave:
                this.origFreqOct = this.paramTimeLen.GetValue();
                break;

            case ParamTimeLen.TimeLenType.FrequencyMul:
                this.origFreqMul = this.paramTimeLen.GetValue();
                break;

            case ParamTimeLen.TimeLenType.BeatOctave:
                this.origBeatOct = this.paramTimeLen.GetValue();
                break;

            case ParamTimeLen.TimeLenType.BeatMul:
                this.origBeatMul = this.paramTimeLen.GetValue();
                break;
        }

        // Switch type
        this.paramTimeLen.freqTy = ft;

        // Switch to the type's matching cached value.
        switch (this.paramTimeLen.freqTy)
        {
            case ParamTimeLen.TimeLenType.Hertz:
                this.paramTimeLen.SetValue(this.origHertz);
                break;

            case ParamTimeLen.TimeLenType.Seconds:
                this.paramTimeLen.SetValue(this.origSeconds);
                break;

            case ParamTimeLen.TimeLenType.FrequencyOctave:
                this.paramTimeLen.SetValue(this.origFreqOct);
                break;

            case ParamTimeLen.TimeLenType.FrequencyMul:
                this.paramTimeLen.SetValue(this.origFreqMul);
                break;

            case ParamTimeLen.TimeLenType.BeatOctave:
                this.paramTimeLen.SetValue(this.origBeatOct);
                break;

            case ParamTimeLen.TimeLenType.BeatMul:
                this.paramTimeLen.SetValue(this.origBeatMul);
                break;
        }

        this.UpdateSlider();
        this.UpdateInput();
    }

    public void OnButton_FreqOct()
    { 
        if(this.paramTimeLen != null)
        { 
            this.SetTimeLenType(ParamTimeLen.TimeLenType.FrequencyOctave);
            this.UpdateUnits(false);

            this.Vibrate();
        }
    }

    public void OnButton_FreqMul()
    {
        if (this.paramTimeLen != null)
        {
            this.SetTimeLenType(ParamTimeLen.TimeLenType.FrequencyMul);
            this.UpdateUnits(false);

            this.Vibrate();
        }
    }

    public void OnButton_BeatOct()
    {
        if (this.paramTimeLen != null)
        {
            this.SetTimeLenType(ParamTimeLen.TimeLenType.BeatOctave);
            this.UpdateUnits(false);

            this.Vibrate();
        }
    }

    public void OnButton_BeatMul()
    {
        if (this.paramTimeLen != null)
        {
            this.SetTimeLenType(ParamTimeLen.TimeLenType.BeatMul);
            this.UpdateUnits(false);

            this.Vibrate();
        }
    }

    public void OnButton_Seconds()
    {
        if (this.paramTimeLen != null)
        {
            this.SetTimeLenType(ParamTimeLen.TimeLenType.Seconds);
            this.UpdateUnits(false);

            this.Vibrate();
        }
    }

    public void OnButton_Freq()
    {
        if (this.paramTimeLen != null)
        {
            this.SetTimeLenType(ParamTimeLen.TimeLenType.Hertz);
            this.UpdateUnits(false);

            this.Vibrate();
        }
    }

    protected void Vibrate()
    { 
        if (this.doVibrate == 0)
            this.dlgRoot.application.DoVibrateButton();
    }

    public override void OnConfirm()
    {
        if (this.paramFloat != null)
        {
            if (this.paramFloat.value != this.origFloat)
            {
                this.dlgRoot.application.SetLLDAWParamValue(
                    this.owner,
                    this.paramFloat,
                    this.origStringValue,
                    this.paramFloat.GetStringValue());

                return;
            }
        }

        if(this.paramInt != null)
        {
            if (this.paramInt.value != this.origInt)
            {
                this.dlgRoot.application.SetLLDAWParamValue(
                    this.owner,
                    this.paramInt,
                    this.origStringValue,
                    this.paramInt.GetStringValue());

                return;
            }
        }

        if(this.paramTimeLen != null)
        { 
            if(this.paramTimeLen.GetValue() != this.origTime)
            { 
                this.dlgRoot.application.SetLLDAWParamValue(
                    this.owner,
                    this.paramTimeLen,
                    this.origStringValue,
                    this.paramTimeLen.GetStringValue());

                return;
            }
        }
    }
}
