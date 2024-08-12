// <copyright file="Handler.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's Handler Java class.</summary>


#if UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class Handler : JavaObj
        {
            public Handler(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            {}
        }
    }
}
#endif