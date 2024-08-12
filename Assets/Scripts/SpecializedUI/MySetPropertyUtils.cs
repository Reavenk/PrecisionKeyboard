// Decompiled with JetBrains decompiler
// Type: UnityEngine.UI.SetPropertyUtility
// Assembly: UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2216A18B-AF52-44A5-85A0-A1CAA19C1090
// Assembly location: C:\Users\Blake\sandbox\unity\test-project\Library\UnityAssemblies\UnityEngine.UI.dll

using System;

namespace UnityEngine.UI
{
  internal static class MySetPropertyUtility
  {
    public static bool SetColor(ref Color currentValue, Color newValue)
    {
      if ((double) currentValue.r == (double) newValue.r && (double) currentValue.g == (double) newValue.g && ((double) currentValue.b == (double) newValue.b && (double) currentValue.a == (double) newValue.a))
        return false;
      currentValue = newValue;
      return true;
    }

    public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T>
    {
      if (currentValue.Equals(newValue))
        return false;
      currentValue = newValue;
      return true;
    }

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
      if (currentValue.Equals((object) newValue))
        return false;
      currentValue = newValue;
      return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
      if ((object) currentValue == null && (object) newValue == null || (object) currentValue != null && currentValue.Equals((object) newValue))
        return false;
      currentValue = newValue;
      return true;
    }
  }
}