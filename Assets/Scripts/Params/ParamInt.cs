// <copyright file="ParamInt.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Represent an introspective integer parameter.</summary>

public class ParamInt : ParamBase
{
    public static string serializationTypeId = "int";

    public int value;

    public int min;
    public int max;

    public string unitName;

    public ParamInt(string name, string unitName, int value, int min, int max, string widgetType = "")
        : base(name, Type.Int, widgetType)
    { 
        this.value = value;
        this.min = min;
        this.max = max;
        this.unitName = unitName;
    }

    public override string GetStringValue()
    {
        return this.value.ToString();
    }

    public override bool SetValueFromString(string str)
    {
        return int.TryParse(str, out this.value);
    }

    public override string unit => this.unitName;
}
