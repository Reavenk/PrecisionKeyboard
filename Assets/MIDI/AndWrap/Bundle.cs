// <copyright file="Bundle.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Wrapper for Android's Bundle Java class.</summary>

#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace AndWrap
    {
        public class Bundle : JavaObj
        {
            public Bundle(AndroidJavaObject ajo, IReporter reporter)
                : base(ajo, reporter)
            { }

            public void clear()
            { 
                this._ajo.Call("clear");
            }

            public JavaObj	clone()
            {
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("clone");
                return new JavaObj(ajo, this._r);
            }

            public Bundle deepCopy()
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("deepCopy");
                return new Bundle(ajo, this._r);
            }

            public int describeContents()
            { 
                return this._ajo.Call<int>("describeContents");
            }

            //public IBinder getBinder(string key)
            //{ }

            public Bundle getBundle(string key)
            { 
                AndroidJavaObject ajo = this._ajo.Call<AndroidJavaObject>("getBundle");
                return new Bundle(ajo, this._r);
            }

            public byte getByte(string key)
            { 
                return this._ajo.Call<byte>("getByte");
            }

            public byte getByte(string key, byte defaultValue)
            { 
                return this._ajo.Call<byte>("getByte", key, defaultValue);
            }

            //byte[] getByteArray(string key)

            public char getChar(string key)
            { 
                return this._ajo.Call<char>("getChar", key);
            }

            public char getChar(string key, char defaultValue)
            { 
                return this._ajo.Call<char>("getChar", key, defaultValue);
            }

            //char[] getCharArray(string key)
            //{ 
            //}

            //public CharSequence	getCharSequence(string key, CharSequence defaultValue)

            // public CharSequence	getCharSequence(string key)

            // public CharSequence[]	getCharSequenceArray(string key)

            //ArrayList<CharSequence>	getCharSequenceArrayList(string key)

            //public ClassLoader getClassLoader(){}

            // float getFloat(string key, float defaultValue)

            // float	getFloat(string key)

            // float[] getFloatArray(string key)

            // ArrayList<Integer> getIntegerArrayList(string key)

            //<T extends Parcelable> T getParcelable(string key)

            //public Parcelable[] getParcelableArray(string key)

            //public <T extends Parcelable> ArrayList<T> getParcelableArrayList(string key)

            //public Serializable getSerializable(string key)

            public short getShort(string key)
            { 
                return this._ajo.Call<short>("getShort");
            }

            public short getShort(string key, short defaultValue)
            {
                return this._ajo.Call<short>("getShort", key, defaultValue);
            }

            //short[]	getShortArray(string key)

            //public Size getSize(String key){}

            //public SizeF getSizeF(String key){}

            //<T extends Parcelable> SparseArray<T>	getSparseParcelableArray(String key){}

            //ArrayList<String>	getStringArrayList(String key){}

            public bool hasFileDescriptors()
            { 
                return this._ajo.Call<bool>("hasFileDescriptors");
            }
                 
            public void putAll(Bundle bundle)
            {
                this._ajo.Call("putAll", bundle._ajo);
            }


            //public void putBinder(string key, IBinder value)
            //{ 
            //}
                 
            public void putBundle(string key, Bundle value)
            { 
            }
                 
            public void putByte(string key, byte value)
            { 
            }
                 
            public void putByteArray(string key, byte[] value)
            { 
            }
                 
            public void putChar(string key, char value)
            { 
            }
                 
            public void putCharArray(string key, char[] value)
            { 
                this._ajo.Call("putCharArray", key, value);
            }

            public void putCharArray(string key, List<char> value)
            { 
                this.putCharArray(key, value.ToArray());
            }
                 
            //public void putCharSequence(String key, CharSequence value)
                 
            //public void putCharSequenceArray(String key, CharSequence[] value)

            //public void putCharSequenceArrayList(String key, ArrayList<CharSequence> value){ }

            public void putFloat(string key, float value)
            { 
            }

            public void putFloatArray(string key, float[] value)
            { 
                this._ajo.Call("putFloatArray", key, value);
            }

            public void putFloatArray(string key, List<float> value)
            { 
                putFloatArray(key, value.ToArray());
            }

            //public void putIntegerArrayList(String key, ArrayList<Integer> value)
            //{ 
            //}

            //public void putParcelable(String key, Parcelable value)
            //{ 
            //}

            //public void putParcelableArray(String key, Parcelable[] value)
            //{ 
            //}

            //void	putParcelableArrayList(String key, ArrayList<? extends Parcelable> value)

            //public void putSerializable(String key, Serializable value)
            //{ 
            //}

            public void putShort(string key, short value)
            { 
                this._ajo.Call("putShort", key, value);
            }

            public void putShortArray(string key, short[] value)
            { 
            }

            //public void putSize(string key, Size value)
            //{ 
            //}

            //public void putSizeF(String key, SizeF value)
            //{ 
            //}

            //public void putSparseParcelableArray(String key, SparseArray<? extends Parcelable> value)
            //{ 
            //}

            //public void	putStringArrayList(String key, ArrayList<String> value)
            //{ 
            //}

            public string getString(string key)
            { 
                return this._ajo.Call<string>("getString", key);
            }

            public string getString(string key, string defaultValue)
            { 
                return this._ajo.Call<string>("getString", key, defaultValue);
            }

            public void readFromParcel(Parcel parcel)
            { 
                this._ajo.Call("readFromParcel", parcel._ajo);
            }

            public void remove(string key)
            {
                this._ajo.Call("remove", key);
            }

            //public void	setClassLoader(ClassLoader loader)
            //{ }

            public void writeToParcel(Parcel parcel, int flags)
            { 
                this._ajo.Call("writeToParcel", parcel._ajo, flags);
            }
        }
    }
}

#endif