// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParamBase
{
    public enum Type
    { 
        Bool,
        Nickname,
        Int,
        Float,
        Enum,
        PCMInput,
        TimeLen,
        WireReference,
        Text,
        Invalid
    }

    public static string ToSerializationType(Type ty)
    { 
        switch(ty)
        { 
            case Type.Bool:
                return "bool";

            case Type.Int:
                return "int";

            case Type.Float:
                return "float";

            case Type.Enum:
                return "enum";

            case Type.PCMInput:
                return "connection";

            case Type.TimeLen:
                return "timelen";

            case Type.Nickname:
                return "nickname";

            case Type.WireReference:
                return "wref";

            case Type.Text:
                return "text";
        }
        return "";
    }

    public static Type FromSerializationType(string str)
    { 
        switch(str)
        { 
            case "bool":
                return Type.Bool;

            case "int":
                return Type.Int;

            case "float":
                return Type.Float;

            case "enum":
                return Type.Enum;

            case "connection":
                return Type.PCMInput;

            case "timelen":
                return Type.TimeLen;

            case "nickname":
                return Type.Nickname;

            case "wref":
                return Type.WireReference;

            case "text":
                return Type.Text;

        }

        return Type.Invalid;
    }

    public readonly string name;
    public bool visible = false;
    public string widgetType;
    public readonly Type type;
    public bool editable = true;

    public string description = string.Empty;

    public ParamBase(string name, Type intrinsicType, string widgetType = "")
    { 
        this.name = name;
        this.widgetType = widgetType;
        this.type = intrinsicType;
    }

    public abstract string GetStringValue();

    public abstract bool SetValueFromString(string str);

    // Called internally during load after a WiringDocument has finished loading
    // all its nodes.
    public virtual bool SetValueFromString(
        string str, 
        Dictionary<string, LLDNBase> directory)
    { return false;}

    // Called after ALL wiring documents have been loaded and are available.
    public virtual bool PostLoad(
        Dictionary<string, LLDNBase> directory,
        Dictionary<string, WiringDocument> scopedDocs)
    { return false; }

    public abstract string unit {get; }

    public virtual bool ReferencesDocument(WiringDocument wd) => false;
}
