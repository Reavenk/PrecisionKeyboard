// <copyright file="JavaObj.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Java's object class using Unity's AndroidJavaClass system.</summary>


#if UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    { 
        public class JavaObj
        { 
            protected static AndroidJavaClass arrayClass = 
                        new AndroidJavaClass("java.lang.reflect.Array");

            public readonly AndroidJavaObject _ajo;
            public readonly IReporter _r;

            public static JavaObj Null()
            {  return new JavaObj(null, null); }

            public JavaObj(AndroidJavaObject ajo, IReporter reporter)
            { 
                this._ajo = ajo;
                this._r = reporter;
            }

            public bool	equals(AndroidJavaObject o)
            {
                return this._ajo.Call<bool>("equals", o);
            }

            public int hashCode()
            { 
                return this._ajo.Call<int>("hashCode");
            }

            public string toString()
            { 
                return this._ajo.Call<string>("toString");
            }
        }
    }
}
#endif