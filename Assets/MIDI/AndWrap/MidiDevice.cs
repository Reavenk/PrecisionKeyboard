// <copyright file="MidiDevices.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's MidiDevices Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public class MidiDevice : JavaObj
        {
            public MidiDevice(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public void close()
            { 
                this._ajo.Call("close");
            }

            public MidiConnection connectPorts(MidiInputPort inputPort, int outputPortNumber)
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("connectionPorts", inputPort._ajo, outputPortNumber);
                return new MidiConnection(ajo, this._r);
            }

            public MidiDeviceInfo getInfo()
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("getInfo");
                return new MidiDeviceInfo(ajo, this._r);
            }

            public MidiInputPort openInputPort(int portNumber)
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("openInputPort", portNumber);
                return new MidiInputPort(ajo, this._r);
            }

            public MidiOutputPort openOutputPort(int portNumber)
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("openOutputPort", portNumber);
                return new MidiOutputPort(ajo, this._r);
            }

        }
    }
}
#endif