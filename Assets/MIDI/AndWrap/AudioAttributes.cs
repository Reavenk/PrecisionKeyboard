// <copyright file="AudioAttributes.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's AudioAttributes Java class.</summary>

#if UNITY_ANDROID

using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        // https://developer.android.com/reference/android/media/AudioAttributes
        public class AudioAttributes : JavaObj
        { 

            public const int ALLOW_CAPTURE_BY_ALL                       = 0x00000001;
            public const int ALLOW_CAPTURE_BY_NONE                      = 0x00000003;
            public const int ALLOW_CAPTURE_BY_SYSTEM                    = 0x00000002;
            public const int CONTENT_TYPE_MOVIE                         = 0x00000003;
            public const int CONTENT_TYPE_MUSIC                         = 0x00000002;
            public const int CONTENT_TYPE_SONIFICATION                  = 0x00000004;
            public const int CONTENT_TYPE_SPEECH                        = 0x00000001;
            public const int CONTENT_TYPE_UNKNOWN                       = 0x00000000;
            public const int FLAG_AUDIBILITY_ENFORCED                   = 0x00000001;
            public const int FLAG_HW_AV_SYNC                            = 0x00000010;
            public const int FLAG_LOW_LATENCY                           = 0x00000100;
            public const int USAGE_ALARM                                = 0x00000004;
            public const int USAGE_ASSISTANCE_ACCESSIBILITY             = 0x0000000b;
            public const int USAGE_ASSISTANCE_NAVIGATION_GUIDANCE       = 0x0000000c;
            public const int USAGE_ASSISTANCE_SONIFICATION              = 0x0000000d;
            public const int USAGE_ASSISTANT                            = 0x00000010;
            public const int USAGE_GAME                                 = 0x0000000e;
            public const int USAGE_MEDIA                                = 0x00000001;
            public const int USAGE_NOTIFICATION                         = 0x00000005;
            public const int USAGE_NOTIFICATION_COMMUNICATION_DELAYED   = 0x00000009;
            public const int USAGE_NOTIFICATION_COMMUNICATION_INSTANT   = 0x00000008;
            public const int USAGE_NOTIFICATION_COMMUNICATION_REQUEST   = 0x00000007;
            public const int USAGE_NOTIFICATION_EVENT                   = 0x0000000a;
            public const int USAGE_NOTIFICATION_RINGTONE                = 0x00000006;
            public const int USAGE_UNKNOWN                              = 0x00000000;
            public const int USAGE_VOICE_COMMUNICATION                  = 0x00000002;
            public const int USAGE_VOICE_COMMUNICATION_SIGNALLING       = 0x00000003;

            public AudioAttributes(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public bool areHapticChannelsMuted()
            { 
                return this._ajo.Call<bool>("areHapticChannelsMuted");
            }

            public int describeContents()
            { 
                return this._ajo.Call<int>("describeContents");
            }

            public int getAllowedCapturePolicy()
            { 
                return this._ajo.Call<int>("getAllowedCapturePolicy");
            }

            public int getContentType()
            { 
                return this._ajo.Call<int>("getContentType");
            }

            public int getFlags()
            { 
                return this._ajo.Call<int>("getFlags");
            }

            public int getUsage()
            { 
                return this._ajo.Call<int>("getUsage");
            }

            public int getVolumeControlStream()
            { 
                return this._ajo.Call<int>("getVolumeControlStream");
            }

            public void writeToParcel(Parcel dest, int flags)
            { 
                this._ajo.Call("writeToParcel", dest._ajo, flags);
            }
        }
    }
}
#endif