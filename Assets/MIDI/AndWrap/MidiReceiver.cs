// <copyright file="MidiReceiver.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's MidiReceiver Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class MidiReceiver : JavaObj
        {
            public MidiReceiver(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public void flush()
            { 
                this._ajo.Call("flush");
            }

            public int getMaxMessageSize()
            { 
                return this._ajo.Call<int>("getMaxMessageSize");
            }

            public void onFlush()
            { 
                this._ajo.Call("onFlush");
            }

            // Abstract function
            //
            //public void onSend(sbyte[] msg, int offset, int count, long timestamp)
            //{
            //    this._ajo.Call("onSend", msg, offset, count, timestamp);
            //}

            public void send(sbyte[] msg, int offset, int count)
            { 
                this._ajo.Call("send", msg, offset, count);
            }

            public void send(sbyte[] msg, int offset, int count, long timestamp)
            { 
                this._ajo.Call("send", msg, offset, count, timestamp);
            }
        }
    }
}

#endif