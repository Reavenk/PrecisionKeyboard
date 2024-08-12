// <copyright file="ParamBool.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Represent an introspective bool parameter.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamBool : ParamBase
{
    public bool value;

    public bool Value {get {return this.value; } }

    public string trueString = "On";
    public string falseString = "Off";

    public string unitName;

    public ParamBool(string name, string unitName, bool value, string widgetType = "")
        : base(name, Type.Bool, widgetType)
    { 
        this.value = value;
        this.unitName = unitName;
    }

    public void SetLabels(string trueStr, string falseStr)
    { 
        this.trueString = trueStr;
        this.falseString = falseStr;
    }

    public override string GetStringValue()
    { 
        return ConvertToString(this.value);
    }

    public static bool ConvertFromString(string str)
    { 
        return str.ToLower() == "true";
    }

    public static string ConvertToString(bool b)
    { 
        return b ? "true" : "false";
    }

    public override bool SetValueFromString(string str)
    { 
        this.value = ParamBool.ConvertFromString(str);
        return true;
    }

    public override string unit => this.unitName;

}
