// <copyright file="VibrationEffect.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's VibrationEffect Java class.</summary>

#if UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class VibrationEffect : JavaObj
        {
            // https://developer.android.com/reference/android/os/VibrationEffect

            public const int DEFAULT_AMPLITUDE = -1;
            public const int EFFECT_CLICK = 0x00000000;
            public const int EFFECT_DOUBLE_CLICK = 0x00000001;
            public const int EFFECT_HEAVY_CLICK = 0x00000005;
            public const int EFFECT_TICK = 0x00000002;

            public VibrationEffect(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            //static VibrationEffect createOneShot(long milliseconds, int amplitude)
            //{ 
            //}
            //
            //static VibrationEffect createPredefined(int effectId)
            //{ 
            //}
            //
            //static VibrationEffect createWaveform(long[] timings, int[] amplitudes, int repeat)
            //{ 
            //}
            //
            //static VibrationEffect createWaveform(long[] timings, int repeat)
            //{ 
            //}
            //
            //public int describeContents()
            //{ 
            //}
            //
            //static VibrationEffect.Composition	startComposition()
            //{ 
            //}
        }
    }
}
#endif