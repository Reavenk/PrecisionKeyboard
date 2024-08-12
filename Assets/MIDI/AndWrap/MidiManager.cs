// <copyright file="MidiManager.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's MidiManager Java class.</summary>

#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class MidiManager : JavaObj
        {
            
#if UNITY_ANDROID && !UNITY_EDITOR
            private static AndroidJavaObject _ajoClass = 
                new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity")
                    .Call<AndroidJavaObject>("getSystemService", "midi");
#else
            private static AndroidJavaObject _ajoClass = null;
#endif
        

            public MidiManager(IReporter reporter)
                : base(_ajoClass, reporter)
            { }

            public List<MidiDeviceInfo> GetDevices()
            { 
                List<MidiDeviceInfo> ret = new List<MidiDeviceInfo>();
                try
                {
                    AndroidJavaObject devices = _ajo.Call<AndroidJavaObject>("getDevices");
                    int arrayLen = AndroidJNI.GetArrayLength(devices.GetRawObject());

                    for(int i = 0; i < arrayLen; ++i)
                    { 
                        AndroidJavaObject ajGet = arrayClass.CallStatic<AndroidJavaObject>("get", devices, i);
                        MidiDeviceInfo amd = new MidiDeviceInfo(ajGet, this._r);
                        ret.Add(amd);
                    }
                }
                catch(System.Exception ex)
                { 
                    DiagnosticScene.InstLog(ex.Message);
                }

                return ret;
            }

            public void openBluetoothDevice(BluetoothDevice bluetoothDevice, OnDeviceOpenedListener listener, Handler handler)
            { 
                this._ajo.Call("openBluetoothDevice", bluetoothDevice._ajo, listener, handler._ajo);
            }

            public void openDevice(MidiDeviceInfo deviceInfo, OnDeviceOpenedListener listener, Handler handler)
            { 
                Debug.Log("Calling openDevice");
                this._ajo.Call("openDevice", deviceInfo._ajo, listener, handler._ajo);
                Debug.Log("Called openDevice");
            }

            //public void registerDeviceCallback(MidiManager.DeviceCallback callback, Handler handler)
            //{ }

            //public void unregisterDeviceCallback(MidiManager.DeviceCallback callback)
            //{ }
        }
    }
}
#endif