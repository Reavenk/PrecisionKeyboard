// <copyright file="MIDIInputWin.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Implementation of MIDIInput for Windows.</summary>

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
    public class MIDIInputWin : MIDIInput
{ 
    MIDIMgr.MIDIDevice device;

    int deviceID = -1;

    public int DeviceID 
    {get{return this.deviceID; } }

    internal MIDIInputWin(MIDIMgr.MIDIDevice device)
    { 
        this.device = device;
    }

    public bool Equivalent(MIDIInput input)
    { 
        MIDIInputWin inptWin = input as MIDIInputWin;
        if(inptWin == null)
            return false;

        if(this == inptWin)
            return true;

        if(
            this.device.productId == inptWin.device.productId &&
            this.device.port == inptWin.device.port &&
            MIDIMgr.StrCompShort(this.device.name, inptWin.device.name) == true)
        { 
            return true;
        }

        return false;
    }

    public bool Activate()
    {
        if(this.deviceID != -1)
            return true;

        this.deviceID = MIDIMgr.MIDIMgr_OpenMIDIDevice((int)this.device.port);

        if(this.deviceID != -1)
            return true;

        return false;
    }

    public bool Deactivate()
    { 
        if(this.deviceID == -1)
            return false;

        bool ret = MIDIMgr.MIDIMgr_CloseMIDIDevice(this.deviceID);
        this.deviceID = -1;

        return ret;
    }

    public string DeviceName()
    { 
        return this.device.GetName();
    }

    public string Description()
    { 
        return "MIDI input for " + this.DeviceName();
    }
}
#endif
