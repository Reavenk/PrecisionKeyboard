// <copyright file="ParamConnection.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Represents a connection to a PCM stream.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamConnection : ParamBase
{
    public static string serializationTypeId = "connection";

    LLDNBase reference;

    public ParamConnection(string name, LLDNBase reference, string widgetType = "")
        : base(name, Type.PCMInput, widgetType)
    { 
        this.reference = reference;
    }

    public bool IsConnected()
    { 
        return this.reference != null;
    }

    public bool IsConnected(LLDNBase test)
    { 
        return this.reference == test;
    }

    public void SetReference(LLDNBase newRef)
    { 
        this.reference = newRef;
    }

    public void ClearReference()
    { 
        this.reference = null;
    }

    public LLDNBase Reference 
    {
        get
        {
            return this.reference; 
        } 
    }

    public override string GetStringValue()
    { 
        if(this.reference == null)
            return string.Empty;

        return this.reference.GUID;
    }

    public override bool SetValueFromString(string str)
    { 
        return false;
    }

    public override bool SetValueFromString(string str, Dictionary<string, LLDNBase> directory)
    { 
        return directory.TryGetValue(str, out this.reference);
    }

    public override string unit => "Audio Stream";
}