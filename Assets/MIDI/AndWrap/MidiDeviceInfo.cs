// <copyright file="MidiDeviceInfo.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's MidiDeviceInfo Java class.</summary>

#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public class MidiDeviceInfo : JavaObj
        { 
            public const string PROPERTY_BLUETOOTH_DEVICE   = "bluetooth_device";
            public const string PROPERTY_MANUFACTURER       = "manufacturer";
            public const string PROPERTY_NAME               = "name";
            public const string PROPERTY_PRODUCT            = "product";
            public const string PROPERTY_SERIAL_NUMBER      = "serial_number";
            public const string PROPERTY_USB_DEVICE         = "usb_device";
            public const string PROPERTY_VERSION            = "version";

            public const int TYPE_BLUETOOTH                 = 0x00000003;
            public const int TYPE_USB                       = 0x00000001;
            public const int TYPE_VIRTUAL                   = 0x00000002;

            public MidiDeviceInfo(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public int	describeContents()
            {
                return this._ajo.Call<int>("describeContents");
            }

            public int getId()
            { 
                return this._ajo.Call<int>("getId");
            }

            public int getInputPortCount()
            { 
                return this._ajo.Call<int>("getInputPortCount");
            }

            public int getOutputPortCount()
            {
                return this._ajo.Call<int>("getOutputPortCount");
            }

            public List<PortInfo> getPorts()
            { 
                List<PortInfo> ret = new List<PortInfo>();

                AndroidJavaObject ports = _ajo.Call<AndroidJavaObject>("getPorts");
                int arrayLen = AndroidJNI.GetArrayLength(ports.GetRawObject());

                for(int i = 0; i < arrayLen; ++i)
                { 
                    AndroidJavaObject ajGet = arrayClass.CallStatic<AndroidJavaObject>("get", ports, i);
                    PortInfo pi = new PortInfo(ajGet, this._r);
                    ret.Add(pi);
                }

                return ret;
            }

            public Bundle getProperties()
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("getProperties");
                return new Bundle(ajo, this._r);
            }

            public int getType()
            { 
                return this._ajo.Call<int>("getType");
            }


            public bool isPrivate()
            { 
                return this._ajo.Call<bool>("isPrivate");
            }

            public void writeToParcel(Parcel parcel, int flags)
            {
                this._ajo.Call("writeToParcel", parcel._ajo, flags);
            }
        }
    }
}

#endif