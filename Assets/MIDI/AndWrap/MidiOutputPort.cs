// <copyright file="MidiOutputPort.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's MidiOutputPort Java class.</summary>

#if UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class MidiOutputPort : JavaObj
        { 
            public MidiOutputPort(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public void close()
            { 
                this._ajo.Call("close");
            }

            public int getPortNumber()
            { 
                return this._ajo.Call<int>("getPortNumber");
            }

            // Abstract callback
            //public void onConnect(MidiReceiver receiver)
            //{ 
            //}
            
            // Abstract callback
            //public void onDisconnect(MidiReceiver receiver)
            //{ 
            //}

            public void connect(MidiReceiver receiver)
            { 
                this._ajo.Call("connect", receiver._ajo);
            }

            public void disconnect(MidiReceiver receiver)
            { 
                this._ajo.Call("disconnect", receiver._ajo);
            }

        }
    }
}
#endif