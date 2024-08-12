// <copyright file="PortInfo.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's PortInfo Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public class PortInfo : JavaObj
        {
            public const int TYPE_INPUT     = 0x00000001;
            public const int TYPE_OUTPUT    = 0x00000002;

            public PortInfo(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public string	getName()
            { 
                return this._ajo.Call<string>("getName");
            }

            public int getPortNumber()
            { 
                return this._ajo.Call<int>("getPortNumber");
            }

            public int getType()
            { 
                return this._ajo.Call<int>("getType");
            }
        }
    }
}
#endif