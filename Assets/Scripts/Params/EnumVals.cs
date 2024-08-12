// <copyright file="EnumVals.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>Holds the enum datatype.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class EnumVals
{
    public struct Entry
    { 
        public readonly string id;
        public readonly int val;
        public readonly string label;

        public Entry(string id, int val, string label)
        { 
            this.id = id;
            this.val = val;
            this.label = label;
        }
    }

    public List<Entry> entries = new List<Entry>();

    public EnumVals(params Entry [] entries)
    { 
        this.entries = new List<Entry>(entries);
    }

    public bool HasValue(string val)
    { 
        foreach(Entry e in this.entries)
        { 
            if(e.id == val)
                return true;
        }
        return false;
    }

    public bool HasValue(int val)
    { 
        foreach(Entry e in this.entries)
        { 
            if(e.val == val)
                return true;
        }
        return false;
    }

    public int GetValue(string val)
    {
        foreach (Entry e in this.entries)
        {
            if (e.id == val)
                return e.val;
        }
        return -1;
    }

    public string GetValue(int val)
    {
        foreach (Entry e in this.entries)
        {
            if (e.val == val)
                return e.id;
        }
        return string.Empty;
    }

    public string GetLabel(int val)
    {
        foreach (Entry e in this.entries)
        {
            if (e.val == val)
                return e.label;
        }
        return string.Empty;
    }

    public string GetLabel(string id)
    {
        foreach (Entry e in this.entries)
        {
            if (e.id == id)
                return e.label;
        }
        return string.Empty;
    }

    public int DefaultValue()
    { 
        if(this.entries.Count == 0)
            return -1;

        return this.entries[0].val;
    }
}

