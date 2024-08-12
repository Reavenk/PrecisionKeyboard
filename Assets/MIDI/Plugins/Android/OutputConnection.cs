// <copyright file="OutputConnection.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// Wrapper for an implementation of MIDIReceiver class in the
// plugin.
// </summary>

#if UNITY_ANDROID

using UnityEngine;

public class OutputConnection : PxPre.AndWrap.MidiReceiver
{
    public OutputConnection(AndroidJavaObject ajo, PxPre.AndWrap.IReporter reporter)
        : base(ajo, reporter)
    { }

    public void seal()
    { 
        this._ajo.Call("seal");
    }
    

    public int getCount()
    { 
        return this._ajo.Call<int>("getCount");
    }
    

    public bool hasMessages()
    { 
        return this._ajo.Call<bool>("hasMessages");
    }
    

    public OCMidiMsg popMessage()
    {
        AndroidJavaObject ajo = 
            this._ajo.Call<AndroidJavaObject>("popMessage");

        if(ajo == null)
            return null;

        return new OCMidiMsg(ajo, this._r);
    }
}
#endif