// <copyright file="Parcel.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's Parcel Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public class Parcel : JavaObj
        { 
            // https://developer.android.com/reference/android/os/Parcel

            // Not going to implement this ATM, it's just the bare bones to
            // complete the AndWrap interfaces of other classes.
            //
            // (wleu 07/02/2020)
            public Parcel(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }
        }
    }
}
#endif