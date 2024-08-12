using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExerciseEnumParam
{
    public struct Entry
    { 
        public int id;
        public Sprite icon;
        public string label;
        public string description;

        public Entry(int id, Sprite icon, string label, string description )
        { 
            this.id = id;
            this.icon = icon;
            this.label = label;
            this.description = description;

            
        }
    }

    public string paramName;
    public string description;
    public readonly bool titlebarIco;

    protected int value;

    List<Entry> exerciseValues = null;
    Entry curEntry;

    string serializationName;
    public string SerializationName { get { return this.serializationName; } }

    public ExerciseEnumParam(string name, string description, string serializationName, bool titlebarIco, params Entry [] entries)
    { 
        this.paramName = name;
        this.description = description;

        this.serializationName = serializationName;

        this.exerciseValues = new List<Entry>(entries);
        this.SetInt(entries[0].id);

        this.titlebarIco = titlebarIco;
    }

    public int GetInt()
    { 
        return (int)this.value;
    }

    public virtual bool SetInt(int val)
    { 
        foreach(Entry e in this.exerciseValues)
        {
            if(e.id == val)
            { 
                this.curEntry = e;
                this.value = val;
                return true;
            }
        }
        return false;
    }

    public IEnumerable<Entry> Entries()
    { 
        return this.exerciseValues;
    }

    public Sprite GetSprite()
    { 
        return this.curEntry.icon;
    }

    public string GetLabel()
    { 
        return this.curEntry.label;
    }

    public string GetDescription()
    { 
        return this.curEntry.description;
    }

    public void Save()
    {
        PlayerPrefs.SetInt(this.serializationName, this.GetInt());
    }

    public bool Load()
    {
        if(PlayerPrefs.HasKey(this.serializationName) == false)
            return false;

        return this.SetInt(PlayerPrefs.GetInt(this.serializationName, this.exerciseValues[0].id));
    }
}
