using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamText : ParamBase
{
    public string value;

    public ParamText(string name, string value, string widgetType = "")
        : base(name, Type.Text, widgetType)
    { 
        this.value = value;
    }

    public override bool SetValueFromString(string str)
    {
        this.value = str;
        return true;
    }

    public override string GetStringValue()
    {
        return this.value;
    }

    public override string unit => "Text";
}
