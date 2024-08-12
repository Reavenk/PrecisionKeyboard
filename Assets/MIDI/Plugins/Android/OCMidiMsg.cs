// <copyright file="OCMidiMsg.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// Wrapper for the android class representing a MIDI message from the android Plugin.
// </summary>

#if UNITY_ANDROID

using UnityEngine;

public class OCMidiMsg : PxPre.AndWrap.JavaObj
{
    public OCMidiMsg(AndroidJavaObject ajo, PxPre.AndWrap.IReporter reporter)
        : base(ajo, reporter)
    { }

    public byte[] bytes 
    {
        get
        { 
            return this._ajo.Get<byte[]>("bytes"); 
        } 
    }

    public long timestamp 
    {
        get
        {
            return this._ajo.Get<long>("timestamp"); 
        } 
    }
}
#endif