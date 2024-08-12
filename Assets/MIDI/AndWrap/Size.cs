// <copyright file="Size.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's Size Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class Size : JavaObj
        {
            public Size(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public int getHeight()
            { 
                return this._ajo.Call<int>("getHeight");
            }

            public int getWidth()
            { 
                return this._ajo.Call<int>("getWidth");
            }

            
        }
    }
}
#endif