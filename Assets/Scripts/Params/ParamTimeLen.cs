// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamTimeLen : ParamBase
{
    public static string serializationTypeId = "freq";

    public enum TimeLenType
    { 
        FrequencyOctave,
        FrequencyMul,
        BeatOctave,
        BeatMul,
        Seconds,
        Hertz
    }

    public struct Value
    { 
        public float value;
        public bool mul;

        public Value(float value)
        { 
            this.value = value;
            this.mul = true;
        }

        public Value(float value, bool mul)
        { 
            this.value = value;
            this.mul = mul;
        }

        public override bool Equals(object obj)
        {
            if(Object.ReferenceEquals(obj, null))
                return false;

            if(obj is Value == false)
                return false;

            Value v = (Value)obj;

            return this == v;
        }

        public static bool operator ==(Value lhs, Value rhs)
        { 
            return 
                lhs.mul == rhs.mul && 
                lhs.value == rhs.value;
        }

        public static bool operator !=(Value lhs, Value rhs)
        {
            return
                lhs.mul != rhs.mul ||
                lhs.value != rhs.value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public bool mul = true;
    public float value;
    public TimeLenType freqTy;

    public const float minFreqOct = -5.0f;
    public const float maxFreqOct = 5.0f;
    //
    public const float minFreqMul = 1.0f;
    public const float maxFreqMul = 10.0f;
    //
    public const float minBeatOct = -5.0f;
    public const float maxBeatOct = 5.0f;
    //
    public const float minBeatMul = 1.0f;
    public const float maxBeatMul = 10.0f;
    //
    public const float minTime = 0.0f;
    public const float maxTime = 10.0f;
    //
    public const float minFr = 0.1f;
    public const float maxFr = 20000.0f;

    public const string WidgetFreq = "frequency";
    public const string WidgetTime = "duration";

    public ParamTimeLen(string name, float value, TimeLenType frty, string widget)
        : base(name, Type.TimeLen, widget)
    { 
        this.value = value;
        this.freqTy = frty;
    }

    public override string GetStringValue()
    {
        string str = "";

        switch(this.freqTy)
        {
            case TimeLenType.FrequencyOctave:
                str = "foct";
                break;

            case TimeLenType.FrequencyMul:
                if(this.mul == true)
                    str = "frmm";
                else
                    str = "frmd";
                break;

            case TimeLenType.BeatOctave:
                str = "boct";
                break;

            case TimeLenType.BeatMul:
                if(this.mul == true)
                    str = "bmm";
                else
                    str = "bmd";
                break;

            case TimeLenType.Seconds:
                str = "sec";
                break;

            default:
                str = "hz";
                break;
        }

        str += "| " + this.value.ToString();
        return str;
    }

    public override bool SetValueFromString(string str)
    {
        string ty = string.Empty;
        string val = string.Empty;

        if(str.Contains("|") == true)
        { 
            string [] rest = str.Split(new char [] {'|' }, System.StringSplitOptions.RemoveEmptyEntries);
            ty = rest[0].Trim();
            val = rest[1].Trim();
        }
        else
        {
            ty = "hz";
            val = str.Trim();
        }

        float.TryParse(val, out this.value);

        switch(ty)
        { 
            case "foct":
                this.freqTy = TimeLenType.FrequencyOctave;
                break;

            case "frmm":
                this.freqTy = TimeLenType.FrequencyMul;
                this.mul = true;
                break;

            case "frmd":
                this.freqTy = TimeLenType.FrequencyMul;
                this.mul = false;
                break;

            case "boct":
                this.freqTy = TimeLenType.BeatOctave;
                break;

            case "bmm":
                this.freqTy = TimeLenType.BeatMul;
                this.mul = true;
                break;

            case "bmd":
                this.freqTy = TimeLenType.BeatMul;
                this.mul = false;
                break;

            case "sec":
                this.freqTy = TimeLenType.Seconds;
                break;

            default:
                this.freqTy = TimeLenType.Hertz;
                break;
        }
        return true;
    }

    public float GetFrequency(float noteFr, float bps)
    {
        switch (this.freqTy)
        {
            case TimeLenType.FrequencyOctave:
                return noteFr * Mathf.Pow(2.0f, this.value);

            case TimeLenType.FrequencyMul:
                if(this.mul == true)
                    return noteFr * this.value;
                else
                    return noteFr / this.value;

            case TimeLenType.BeatOctave:
                return bps * Mathf.Pow(2.0f, this.value);

            case TimeLenType.BeatMul:
                if(this.mul == true)
                    return bps * this.value;
                else
                    return bps / this.value;

            case TimeLenType.Seconds:
                if(this.value == 0.0f)
                    return 1.0f;

                return 1.0f / this.value;

            case TimeLenType.Hertz:
                return this.value;

        }

        return noteFr;
    }

    public float GetWavelength(float noteFr, float bps)
    {
  
        switch (this.freqTy)
        {
            case TimeLenType.FrequencyOctave:
                return (1.0f / noteFr) * Mathf.Pow(2.0f, this.value);

            case TimeLenType.FrequencyMul:
                if(this.mul == true)
                    return (1.0f / noteFr) * this.value;
                else
                    return (1.0f / noteFr) / this.value;

            case TimeLenType.BeatOctave:
                return bps * Mathf.Pow(2.0f, this.value);

            case TimeLenType.BeatMul:
                if(this.mul == true)
                    return bps * this.value;
                else
                    return bps / this.value;

            case TimeLenType.Seconds:
                return this.value;

            case TimeLenType.Hertz:
                if(this.value == 0.0f)
                    return 10.0f;

                return 1.0f / this.value;
        }

        return 1.0f / this.GetFrequency(noteFr, bps);
    }

    public void SetFrequency(float fr, float noteFr, float bps)
    {
        Value newVal = CalcValFromFrequency(fr, noteFr, bps, this.freqTy);
        this.SetValue( newVal);
    }

    public static Value CalcValFromFrequency(float hz, float noteFr, float bps, TimeLenType ty)
    {
        const float minSafeHz = 0.1f; // Arbitrary
        float clampedHz = Mathf.Max(hz, minSafeHz);

        switch (ty)
        {
            case TimeLenType.FrequencyOctave:
                // From wolfram alpha,
                // y = n*2^x solve for x
                // where n is noteFr, and y = fr
                return new Value(Mathf.Log(clampedHz/noteFr) / Mathf.Log(2.0f));

            case TimeLenType.FrequencyMul:
                if(clampedHz > noteFr)
                    return new Value(noteFr/clampedHz, true);
                else
                    return new Value(clampedHz/noteFr, false);

            case TimeLenType.BeatOctave:
                return new Value(Mathf.Log(clampedHz/bps) / Mathf.Log(2.0f));

            case TimeLenType.BeatMul:
                if (clampedHz > bps)
                    return new Value(bps/clampedHz, true);
                else
                    return new Value(clampedHz/bps, false);

            case TimeLenType.Seconds:
                return new Value(1.0f/clampedHz);

            case TimeLenType.Hertz:
                return new Value(hz);
        }

        return new Value(1.0f);
    }

    public override string unit 
    {
        get {
            switch (this.freqTy)
            {
                case TimeLenType.BeatMul:
                    if(this.mul == true)
                        return "BPS Multiple";
                    else
                        return "BPS Fraction";

                case TimeLenType.BeatOctave:
                    return "BPS PowerOf2";

                case TimeLenType.FrequencyMul:
                    if(this.mul == true)
                        return "Frequency Multiple";
                    else
                        return "Frequency Fraction";

                case TimeLenType.FrequencyOctave:
                    return "Frequency PowerOf2";

                case TimeLenType.Seconds:
                    return "Second(s)";

                default:
                    return "Hertz";
            }
        }
    }

    public void SetValue(Value value)
    {

        switch(this.freqTy)
        { 
            case TimeLenType.FrequencyMul:
                this.value = Mathf.Clamp(value.value, minFreqMul, maxFreqMul);
                this.mul = value.mul;
                break;

            case TimeLenType.BeatMul:
                this.value = Mathf.Clamp(value.value, minBeatMul, maxBeatMul);
                this.mul = value.mul;
                break;

            default:
                this.SetValue(value.value);
                this.mul = value.mul;
                break;
        }

        
    }

    public void SetValue(float f)
    { 
        switch(this.freqTy)
        {
            case TimeLenType.Hertz:
                this.value = Mathf.Clamp(f, minFr, maxFr);
                break;

            case TimeLenType.FrequencyOctave:
                this.value = Mathf.Clamp(f, minFreqOct, maxFreqOct);
                break;

            case TimeLenType.FrequencyMul:
                if(f > 0.0f)
                { 
                    this.value = Mathf.Clamp(f, minFreqMul, maxFreqMul);
                    this.mul = true;
                }
                else
                {
                    this.value = Mathf.Clamp(-f, minFreqMul, maxFreqMul);
                    this.mul = false;
                }
                break;

            case TimeLenType.BeatOctave:
                this.value = Mathf.Clamp(f, minBeatOct, maxBeatOct);
                break;

            case TimeLenType.BeatMul:
                if(f > 0.0f)
                {
                    this.value = Mathf.Clamp(f, minBeatMul, maxBeatMul);
                    this.mul = true;
                }
                else
                {
                    this.value = Mathf.Clamp(-f, minBeatMul, maxBeatMul);
                    this.mul = false;
                }
                break;

            case TimeLenType.Seconds:
                this.value = Mathf.Clamp(f, minTime, maxTime);
                break;

            default:
                this.value = f;
                break;
        }
    }

    public Value GetValue()
    {
        return new Value(this.value, this.mul);
    }
}
