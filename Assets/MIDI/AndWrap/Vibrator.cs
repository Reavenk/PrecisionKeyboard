// <copyright file="Vibrator.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's Vibrator Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class Vibrator : JavaObj
        { 
            // https://developer.android.com/reference/android/os/Vibrator

            public const int VIBRATION_EFFECT_SUPPORT_NO        = 0x00000002;
            public const int VIBRATION_EFFECT_SUPPORT_UNKNOWN   = 0x00000000;
            public const int VIBRATION_EFFECT_SUPPORT_YES       = 0x00000001;

            public Vibrator(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public int areAllEffectsSupported(params int [] effectIds)
            { 
                object arrayObj = effectIds;
                return this._ajo.Call<int>("areAllEffectsSupported", arrayObj);
            }

            public bool areAllPrimitivesSupported(params int [] primitiveIds)
            { 
                return this._ajo.Call<bool>("areAllPrimitivesSupported", primitiveIds);
            }

            public int[] areEffectsSupported(params int [] effectIds)
            { 
                return this._ajo.Call<int[]>("areEffectsSupported", effectIds);
            }

            public bool [] arePrimitivesSupported(params int [] primitiveIds)
            { 
                return this._ajo.Call<bool[]>("arePrimitivesSupported", primitiveIds);
            }

            public void cancel()
            { 
                this._ajo.Call("cancel");
            }

            public bool hasAmplitudeControl()
            { 
                return this._ajo.Call<bool>("hasAmplitudeControl");
            }

            public bool hasVibrator()
            {
                return this._ajo.Call<bool>("hasVibrator");
            }

            public void vibrate(long milliseconds)
            { 
                this._ajo.Call("vibrate", milliseconds);
            }

            public void vibrate(long[] pattern, int repeat)
            { 
                this._ajo.Call("vibrate", pattern, repeat);
            }

            public void vibrate(VibrationEffect vibe, AudioAttributes attributes)
            { 
                this._ajo.Call("vibrate", vibe._ajo, attributes._ajo);
            }

            public void vibrate(long milliseconds, AudioAttributes attributes)
            { }

            public void vibrate(VibrationEffect vibe)
            { 
                this._ajo.Call("vibrate", vibe._ajo);
            }

            public void vibrate(long[] pattern, int repeat, AudioAttributes attributes)
            { 
                this._ajo.Call("vibrate", pattern, repeat, attributes._ajo);
            }
        }
    }
}

#endif