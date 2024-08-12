// <copyright file="MIDIInputAnd.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>Implementation of MIDIInput for Android</summary>

using UnityEngine;

#if UNITY_EDITOR || UNITY_ANDROID

public class MIDIInputAnd : MIDIInput
{
    protected class ConnectionListener : PxPre.AndWrap.OnDeviceOpenedListener
    {
        MIDIInputAnd and;
        PxPre.AndWrap.MidiDeviceInfo deviceInfo;
        public ConnectionListener(MIDIInputAnd and, PxPre.AndWrap.MidiDeviceInfo deviceInfo)
        { 
            this.and = and;
            this.deviceInfo = deviceInfo;
        }

        public override void _impl_OnDeviceOpened(PxPre.AndWrap.MidiDevice device)
        {
            if(this.and == null)
                return;

            this.and._OnConnect(this, device, this.deviceInfo);
        }
    }

    MIDIMgr mgr;
    PxPre.AndWrap.MidiDeviceInfo deviceInfo;
    PxPre.AndWrap.PortInfo portInfo;

    PxPre.AndWrap.MidiDevice deviceConnection;
    PxPre.AndWrap.MidiOutputPort outputConnection;
    OutputConnection outputBridge;

    string cachedDeviceName;
    string cachedManufacturer;
    string cachedProduct;

    public MIDIInputAnd(
        MIDIMgr mgr,
        PxPre.AndWrap.MidiDeviceInfo deviceInfo, 
        PxPre.AndWrap.PortInfo portInfo)
    { 
        this.mgr = mgr;
        this.deviceInfo = deviceInfo;
        this.portInfo = portInfo;

        PxPre.AndWrap.Bundle bundle = deviceInfo.getProperties();
        this.cachedDeviceName   = bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_NAME);
        this.cachedManufacturer = bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_MANUFACTURER);
        this.cachedProduct      = bundle.getString(PxPre.AndWrap.MidiDeviceInfo.PROPERTY_PRODUCT);
    }

    public bool Equivalent(MIDIInput input)
    { 
        if(input == null)
            return false;

        MIDIInputAnd outputAnd = input as MIDIInputAnd;
        if(outputAnd == null)
            return false;

        if(this.portInfo == outputAnd.portInfo)
            return true;

        if(this.portInfo.getPortNumber() != outputAnd.portInfo.getPortNumber())
            return false;

        if(this.portInfo.getType() != outputAnd.portInfo.getType())
            return false;

        return true;
    }

    public bool Activate()
    { 
        if(this.deviceConnection != null)
            return false;

        PxPre.AndWrap.MidiManager midiMgr = 
            new PxPre.AndWrap.MidiManager(null);

        ConnectionListener conListener = new ConnectionListener(this, this.deviceInfo);

        midiMgr.openDevice(
            this.deviceInfo, 
            conListener, 
            new PxPre.AndWrap.Handler(null, null));

        // Activate won't do anything, we'll have to wait for the asyncronous callback
        // to come back before we can acknowledge a change.
        return false;
    }

    public bool Deactivate()
    { 
        if(this.deviceConnection == null)
            return false;

        AndroidMIDIDeviceRecord.Inst().RemoveInput(this.deviceInfo);

        this.outputConnection.close();
        this.deviceConnection.close();
        this.outputBridge.seal();
        this.deviceConnection = null;
        this.outputConnection = null;
        this.outputBridge = null;

        return true;
    }

    public string DeviceName()
    { 
        return this.cachedDeviceName;
    }

    public string Description()
    { 
        return this.cachedProduct;
    }

    public int GetMessageCount()
    { 
        if(this.outputBridge == null)
            return 0;

        return this.outputBridge.getCount();
    }

    public byte [] PopMessage()
    { 
        if(this.outputBridge == null)
            return null;

        OCMidiMsg msg = this.outputBridge.popMessage();
        if(msg == null)
            return null;

        return msg.bytes;
    }

    void _OnConnect(ConnectionListener cl, PxPre.AndWrap.MidiDevice device, PxPre.AndWrap.MidiDeviceInfo deviceInfo)
    { 
        this.outputConnection = device.openOutputPort(this.portInfo.getPortNumber());

        if(this.outputConnection == null)
        { 
            device.close();
            AndroidMIDIDeviceRecord.Inst().RemoveInput(deviceInfo);

            return;
        }
        AndroidMIDIDeviceRecord.Inst().AddInput(deviceInfo);

        AndroidJavaObject ajcMidi = new AndroidJavaObject("tech.pixelprecision.midibridge.OutputConnection");
        this.outputBridge = new OutputConnection(ajcMidi, null);
        //
        this.outputConnection.connect(this.outputBridge);

        this.deviceConnection = device;
        this.mgr._OnMIDIInputConnected(this);
    }
}

#endif