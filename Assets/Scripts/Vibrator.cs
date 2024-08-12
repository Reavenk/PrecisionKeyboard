using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.reddit.com/r/Unity3D/comments/4j5js7/unity_vibrate_android_device_for_custom_duration/

public static class Vibrator
{
    public static bool vibrateEnabled = true;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static readonly AndroidJavaObject androidVibrator =
    new AndroidJavaClass("com.unity3d.player.UnityPlayer")// Get the Unity Player.
    .GetStatic<AndroidJavaObject>("currentActivity")// Get the Current Activity from the Unity Player.
    .Call<AndroidJavaObject>("getSystemService", "vibrator");// Then get the Vibration Service from the Current Activity.
#endif

    static Vibrator()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Trick Unity into giving the App vibration permission when it builds.
        // This check will always be false, but the compiler doesn't know that.
        if (UnityEngine.Application.isEditor) Handheld.Vibrate();
#endif
    }

    public static void Vibrate(long milliseconds)
    {
        if(vibrateEnabled == false)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        androidVibrator.Call("vibrate", milliseconds); 
#endif
    }

    public static void Vibrate(long[] pattern, int repeat)
    {
        if(vibrateEnabled == false)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        androidVibrator.Call("vibrate", pattern, repeat);
#endif
    }

    public static bool HasVibrator()
    { 
#if UNITY_EDITOR
        return true;
#elif UNITY_ANDROID
        bool ret = androidVibrator.Call<bool>("hasVibrator"); 
        return ret;
#else
        return false;
#endif
    }
}
