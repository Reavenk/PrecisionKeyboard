// <copyright file="ParamFloat.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Rerpresent an introspective float parameter.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamFloat : ParamBase
{
    public static string serializationTypeId = "float";

    public float value;
    public float min;
    public float max;
    public string unitName;

    public ParamFloat(string name, string unit, float value, float min, float max, string widgetType = "")
        : base(name, Type.Float, widgetType)
    { 
        this.min = min;
        this.max = max;
        this.value = Mathf.Clamp(value, this.min, this.max);
        this.unitName = unit;
    }

    public override string GetStringValue()
    { 
        return this.value.ToString();
    }

    public override bool SetValueFromString(string str)
    {
        float val;
        if(float.TryParse(str, out val) == false)
            return false;

        this.value = Mathf.Clamp(val, min, max);
        return true;
    }

    public override string unit => this.unitName;
}
