// <copyright file="AndroidMIDIDeviceRecord.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// A system for tracking what a MIDI device is reserved for. Designed to work with
// MIDIInputAnd, MIDIOutputAnd and MidiMgr.
// </summary>

#if UNITY_ANDROID

using System.Collections.Generic;

public class AndroidMIDIDeviceRecord
{
    static AndroidMIDIDeviceRecord inst;

    public static AndroidMIDIDeviceRecord Inst()
    { 
        if(inst == null)
            inst = new AndroidMIDIDeviceRecord();

        return inst;
    }

    List<PxPre.AndWrap.MidiDeviceInfo> inputConnections = new List<PxPre.AndWrap.MidiDeviceInfo>();
    List<PxPre.AndWrap.MidiDeviceInfo> outputConnections = new List<PxPre.AndWrap.MidiDeviceInfo>();

    public bool ContainsInput(PxPre.AndWrap.MidiDeviceInfo mdi)
    {
        foreach(PxPre.AndWrap.MidiDeviceInfo mi in this.inputConnections)
        {
            if(CompareDeviceInfo(mi, mdi) == true)
                return true;
        }
        return false;
    }

    public bool ContainsOutput(PxPre.AndWrap.MidiDeviceInfo mdi)
    { 
        foreach(PxPre.AndWrap.MidiDeviceInfo mo in this.outputConnections)
        {
            if(CompareDeviceInfo(mo, mdi) == true)
                return true;
        }
        return false;
    }

    public bool Contains(PxPre.AndWrap.MidiDeviceInfo mdi)
    { 
        foreach(PxPre.AndWrap.MidiDeviceInfo mi in this.inputConnections)
        {
            if(CompareDeviceInfo(mi, mdi) == true)
                return true;
        }

        foreach(PxPre.AndWrap.MidiDeviceInfo mo in this.outputConnections)
        {
            if(CompareDeviceInfo(mo, mdi) == true)
                return true;
        }

        return false;
    }

    public bool AddInput(PxPre.AndWrap.MidiDeviceInfo mdi)
    { 
        foreach(PxPre.AndWrap.MidiDeviceInfo mi in this.inputConnections)
        {
            if(CompareDeviceInfo(mi, mdi) == true)
                return false;
        }
        this.inputConnections.Add(mdi);
        return true;
    }

    public bool AddOutput(PxPre.AndWrap.MidiDeviceInfo mdi)
    { 
        foreach(PxPre.AndWrap.MidiDeviceInfo mo in this.outputConnections)
        {
            if(CompareDeviceInfo(mo, mdi) == true)
                return false;
        }
        this.outputConnections.Add(mdi);
        return true;
    }

    public bool RemoveInput(PxPre.AndWrap.MidiDeviceInfo mdi)
    { 
        for(int i = 0; i < this.inputConnections.Count; ++i)
        {
            if(CompareDeviceInfo(this.inputConnections[i], mdi) == true)
            {
                this.inputConnections.RemoveAt(i);
                return  true;
            }
        }
        return false;
    }

    public bool RemoveOutput(PxPre.AndWrap.MidiDeviceInfo mdi)
    { 
        for(int i = 0; i <  this.outputConnections.Count; ++i)
        {
            if(CompareDeviceInfo(this.outputConnections[i], mdi) == true)
            { 
                this.outputConnections.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public static bool CompareDeviceInfo(PxPre.AndWrap.MidiDeviceInfo mdia, PxPre.AndWrap.MidiDeviceInfo mdib)
    { 
        return mdia.getId() == mdib.getId();
    }
}
#endif