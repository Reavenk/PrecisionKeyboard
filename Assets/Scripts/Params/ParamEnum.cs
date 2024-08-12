// <copyright file="ParamEnum.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Represents an instance of an enumerated value.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamEnum : ParamBase
{
    public static string serializationTypeId = "enum";

    public EnumVals enumVals;
    public int value = -1;

    string unitType;

    public ParamEnum(string name, string unitType, EnumVals enumVals, int value, string widgetType = "")
        : base(name, Type.Enum, widgetType)
    { 
        this.enumVals = enumVals;
        this.value = value;
        this.unitType = unitType;
    }

    public ParamEnum(string name, string unitType, EnumVals enumVals, string value, string widgetType = "")
        : base(name, Type.Enum, widgetType)
    { 
        this.enumVals = enumVals;
        this.value = enumVals.GetValue(value);
        this.unitType = unitType;
    }

    public override string GetStringValue()
    {
        if(value == -1)
            return string.Empty;

        return this.enumVals.GetValue(this.value);
    }

    public override bool SetValueFromString(string str)
    {
        this.value = this.enumVals.GetValue(str);

        if(this.value == -1)
            this.value = this.enumVals.DefaultValue();

        return true;
    }

    public string GetLabel()
    { 
        if(this.value == -1)
            return string.Empty;

        return this.enumVals.GetLabel(this.value);
    }

    public override string unit => this.unitType;
}
