// <copyright file="MidiConnection.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's MidiConnection Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class MidiConnection : JavaObj
        { 
            public MidiConnection(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public void close()
            { 
                this._ajo.Call("close");
            }
        }
    }
}

#endif