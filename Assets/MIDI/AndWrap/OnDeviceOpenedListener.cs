// <copyright file="OnDeviceOpenedListener.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's OnDeviceOpenedListener Java interface.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public abstract class OnDeviceOpenedListener : AndroidJavaProxy
        {
            public readonly IReporter reporter;

            public OnDeviceOpenedListener()
                : base("android.media.midi.MidiManager$OnDeviceOpenedListener")
            { }

            public void onDeviceOpened(AndroidJavaObject ajo)
            { 
                this._impl_OnDeviceOpened(new MidiDevice(ajo, null));
            }

            public abstract void _impl_OnDeviceOpened(MidiDevice device);

            public override bool equals(AndroidJavaObject obj)
            { 
                return base.equals(obj);
            }
        
            public override int hashCode()
            { 
                return base.hashCode();
            }
        
            public override AndroidJavaObject Invoke(string methodName, object[] args)
            { 
                return base.Invoke(methodName, args);
            }
        
            public override AndroidJavaObject Invoke(string methodName, AndroidJavaObject[] javaArgs)
            { 
                return base.Invoke(methodName, javaArgs);
            }
        
            public override string toString()
            { 
                return base.toString();
            }
        }
    }
}

#endif
