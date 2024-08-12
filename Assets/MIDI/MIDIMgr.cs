// <copyright file="MIDIMgr.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>07/07/2020</date>
// <summary>
// Multiplatform manager for input and output MIDI connections in Unity.
// </summary>

using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


/// <summary>
/// 
/// </summary>
public class MIDIMgr : MonoBehaviour, IMIDIPollCountering
{
    /// <summary>
    /// The current connected MIDI output used to send MIDI messages to another
    /// connected device.
    /// </summary>
    MIDIOutput currentOutput = null;

    /// <summary>
    /// The current connected MIDI input used to receive MIDI messages from another
    /// connected device.
    /// </summary>
    MIDIInput currentInput = null;

    /// <summary>
    /// The list of known possible outputs. This is only updated when told to update
    /// or if changes are found while polling.
    /// </summary>
    List<MIDIOutput> knownOutputs = new List<MIDIOutput>();

    /// <summary>
    /// The list of known possibled inputs. This is only updated when told to update 
    /// or if changes are found while polling.
    /// </summary>
    List<MIDIInput> knownInputs = new List<MIDIInput>();

    // How often to do the polling
    const float pollRate = 2.0f;

    float nextPoll = 0.0f;

    int pollingInputCounter = 0;
    int pollingOutputCounter = 0;

    HashSet<IMIDIMsgSink> midiDispatch = 
        new HashSet<IMIDIMsgSink>();

#if UNITY_EDITOR || UNITY_ANDROID
    enum AndroidSyncEvent
    {
        InputConnected,
        InputDisconnected,
        OutputConnected,
        OutputDisconnected
    }

    struct AndroidMIDIEvent
    { 
        public AndroidSyncEvent eventType;
        public MIDIInput input;
        public MIDIOutput output;
    }

    List<AndroidMIDIEvent> androidSyncs = 
        new List<AndroidMIDIEvent>();
#endif

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
    internal struct MIDIDevice
    {
        const int MaxPNameLen = 32;

        public uint port;
        public uint error;
        ushort manufactureId;
        public ushort productId;
        public uint driverVersion;

        /// int[15]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = MaxPNameLen)]
        public short[] name;

        public uint support;

        public string GetName()
        { 
            // We need to check the range because the Windows SDK will fill 
            // extra character with garbage fence values.
            byte[] bytes = new byte[this.name.Length];
            for(int i = 0; i < name.Length; ++i)
            { 
                if(this.name[i] < 0 || this.name[i] > 255)
                    bytes[i] = 0;
                else
                    bytes[i] = (byte)this.name[i];
            }

            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }

    public struct MIDIQueuedMessage
    {
        public enum MessageType
        { 
            Invalid,
            Message,
            Open,
            Close,
            LongData,
            Error,
            LongError,
            MoreData,
            Unknown
        }

        public MessageType messageType;
        public ulong message;
        public ulong timestamp;
    }

    [DllImport("MidiWrapper.dll")]
    internal extern static void MIDIMgr_Clear();

    [DllImport("MidiWrapper.dll")]
    internal extern static int MIDIMgr_GetMIDIDeviceCount();

    [DllImport("MidiWrapper.dll")]
    internal extern static MIDIDevice MIDIMgr_GetMIDIDevice(int deviceNum);

    [DllImport("MidiWrapper.dll")]
    internal extern static int MIDIMgr_OpenMIDIDevice(int deviceNum);

    [DllImport("MidiWrapper.dll")]
    internal extern static bool MIDIMgr_CloseMIDIDevice(int handle);
    
    [DllImport("MidiWrapper.dll")]
    internal extern static bool MIDIMgr_MIDIHasQueuedMessages(int handle);
    
    [DllImport("MidiWrapper.dll")]
    internal extern static uint MIDIMgr_GetMIDIQueuedMessageCount(int handle);
    
    [DllImport("MidiWrapper.dll")]
    internal extern static MIDIQueuedMessage MIDIMgr_PopMIDIQueuedMessage(int handle);

    [DllImport("MidiWrapper.dll")]
    internal extern static bool MIDIMgr_IsHandleValid(int handle);
#endif

    static MIDIMgr instance;
    public static MIDIMgr Instance {get{return instance; } }

    int controllerChannel = 1;

    public Application app;




    /// <summary>
    /// Fired when a MIDI message is sent out when using the application as a MIDI controller
    /// </summary>
    public System.Action<MIDIMgr> onMIDIControllerMsg;

    public System.Action<MIDIMgr, MIDIOutput> onMIDIControllerConnected;

    public System.Action<MIDIMgr> onMIDIControllerDisconnected;

    public System.Action<MIDIMgr, MIDIInput> onMIDIInputConnected;

    public System.Action<MIDIMgr> onMIDIInputDisconnected;

    public System.Action<MIDIMgr, int> onMIDIChannelChanged;

    public System.Action<MIDIMgr> onMIDIInputMsg;

    public System.Action<MIDIMgr, List<MIDIInput>> onMIDIRefreshedInputs;

    public System.Action<MIDIMgr, List<MIDIOutput>> onMIDIRefreshedOutputs;

    // First list is removed, second list is added
    public System.Action<MIDIMgr, List<MIDIInput>, List<MIDIInput>> onMIDIPollInputUpdated;

    // First list is removed, second list is added
    public System.Action<MIDIMgr, List<MIDIOutput>, List<MIDIOutput>> onMIDIPollOutputUpdated;

    void Awake()
    {
        instance = this;
    }

    public List<MIDIInput> QueryInputs()
    { 
        List<MIDIInput> ret = new List<MIDIInput>();

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
        int ct = MIDIMgr_GetMIDIDeviceCount();
        for(int i = 0; i < ct; ++i)
        { 
            MIDIDevice md = MIDIMgr_GetMIDIDevice(i);
            
            if(md.error != 0)
                continue;

            ret.Add(new MIDIInputWin(md));
        }
#elif UNITY_ANDROID
        PxPre.AndWrap.MidiManager midiMgr = new PxPre.AndWrap.MidiManager(null);
        List<PxPre.AndWrap.MidiDeviceInfo> lstDevices = midiMgr.GetDevices();

        foreach(PxPre.AndWrap.MidiDeviceInfo mdi in lstDevices)
        { 
            if(mdi.getOutputPortCount() == 0)
                continue;

            // If a device is taken up for output, we don't want to nuke it by
            // claiming it for input
            if(AndroidMIDIDeviceRecord.Inst().ContainsOutput(mdi) == true)
                continue;

            List<PxPre.AndWrap.PortInfo> lstPort = mdi.getPorts();
            foreach(PxPre.AndWrap.PortInfo pi in lstPort)
            { 
                int type = pi.getType();

                if(type == PxPre.AndWrap.PortInfo.TYPE_INPUT)
                    ret.Add( new MIDIInputAnd(this, mdi, pi));
            }
        }
#endif

        return ret;
    }

    public List<MIDIOutput> QueryOutputs()
    { 
        List<MIDIOutput> ret = new List<MIDIOutput>();

#if UNITY_ANDROID
        // There's a terminology switch here, where what they called inputs are 
        // what we're calling 
        // 
        PxPre.AndWrap.MidiManager midiMgr = new PxPre.AndWrap.MidiManager(null);
        List<PxPre.AndWrap.MidiDeviceInfo> lstDevices = midiMgr.GetDevices();

        foreach(PxPre.AndWrap.MidiDeviceInfo mdi in lstDevices)
        { 
            if(mdi.getInputPortCount() == 0)
                continue;

            // If a device is taken up for input, we can't use it as output
            if(AndroidMIDIDeviceRecord.Inst().ContainsInput(mdi) == true)
                continue;

            List<PxPre.AndWrap.PortInfo> lstPort = mdi.getPorts();
            foreach(PxPre.AndWrap.PortInfo pi in lstPort)
            { 
                int type = pi.getType();

                if(type == PxPre.AndWrap.PortInfo.TYPE_INPUT)
                    ret.Add( new MIDIOutputAnd(this, mdi, pi));
            }
        }
#endif

        return ret;
    }

    private void Start()
    { 
    }



    public void Update()
    {

        List<AndroidMIDIEvent> syncEvts = null;
        lock(this.androidSyncs)
        { 
            syncEvts = this.androidSyncs;
            this.androidSyncs = new List<AndroidMIDIEvent>();
        }
        if(syncEvts != null)
        { 
            foreach(AndroidMIDIEvent ame in syncEvts)
            { 
                switch(ame.eventType)
                { 
                    case AndroidSyncEvent.InputConnected:
                        if(this.currentInput != ame.input)
                        { 
                            if(this.currentInput != null)
                            { 
                                this.currentInput.Deactivate();
                                this.currentInput = null;
                                this.onMIDIInputDisconnected?.Invoke(this);
                            }

                            this.currentInput = ame.input;
                            this.onMIDIInputConnected?.Invoke(this, this.currentInput);
                        }
                        break;

                    //case AndroidSyncEvent.InputDisconnected:
                    //    break;

                    case AndroidSyncEvent.OutputConnected:
                        if(this.currentOutput != ame.output)
                        {
                            // This actually should have been cleared a while ago
                            // but just in case some weird ordering or synch issue
                            // occured.
                            if(this.currentOutput != null)
                            { 
                                this.currentOutput.Deactivate();
                                this.currentOutput = null;
                                this.onMIDIControllerDisconnected?.Invoke(this);
                            }

                            this.currentOutput = ame.output;
                            this.onMIDIControllerConnected?.Invoke(this, this.currentOutput);
                        }

                        break;

                    //case AndroidSyncEvent.OutputDisconnected:
                    //    break;
                }
            }
        }

        // Do regular polling to find out if something that's connected is actually
        // now invalid.
        if(
            (
                this.currentInput != null || 
                this.currentOutput != null ||
                this.pollingInputCounter > 0 ||
                this.pollingOutputCounter > 0
            )
            && 
            Time.time > this.nextPoll)
        { 
            this.nextPoll = Time.time + pollRate;

            if(this.currentInput != null || this.pollingInputCounter > 0)
            { 
                List<MIDIInput> mis = this.QueryInputs();
                if(this.currentInput != null)
                {
                    bool found = false;
                    foreach(MIDIInput mi in mis)
                    { 
                        if(mi.Equivalent(this.currentInput) == true)
                        { 
                            found = true;
                            break;
                        }
                    }

                    if(found == false)
                    { 
                        this.currentInput.Deactivate();
                        this.currentInput = null;
                        this.onMIDIInputDisconnected?.Invoke(this);

                        this.RefreshKnownInputs();
                    }
                }

                // Start with everything assuming to be deleted - and remove
                // it from the deleted list if we find it still exists.
                List<MIDIInput> deleted = new List<MIDIInput>(this.knownInputs);
                // Resuse the mis as items added. As we find stuff in the added
                // list that still exist, we remove it, because it doesn't exist
                // in the list because it was added, but because it already existed.
                List<MIDIInput> added = mis;

                for(int i = 0; i < mis.Count; )
                { 
                    bool foundMatch = false;
                    for(int j = 0; j < deleted.Count; ++j)
                    { 
                        if(mis[i].Equivalent(deleted[j]) == true)
                        { 
                            foundMatch = true;
                            mis.RemoveAt(i);
                            deleted.RemoveAt(j);
                        }
                    }

                    if(foundMatch == false)
                        ++i;
                }

                if(deleted.Count > 0 || added.Count > 0)
                {
                    foreach(MIDIInput mi in deleted)
                        this.knownInputs.Remove(mi);

                    this.knownInputs.AddRange(added);
                    this.onMIDIPollInputUpdated?.Invoke(this, deleted, added);
                }
            }

            if(this.currentOutput != null || this.pollingOutputCounter > 0)
            { 
                List<MIDIOutput> mos = this.QueryOutputs();
                if(this.currentOutput != null)
                {
                    bool found = false;
                    foreach(MIDIOutput mo in mos)
                    { 
                        if(mo.Equivalent(this.currentOutput) == true)
                        { 
                            found = true;
                            break;
                        }
                    }
            
                    if(found == false)
                    { 
                        this.currentOutput.Deactivate();
                        this.currentOutput = null;
                        this.onMIDIControllerDisconnected?.Invoke(this);

                        this.RefreshKnownOutputs();
                    }
                }

                List<MIDIOutput> deleted = new List<MIDIOutput>(this.knownOutputs);
                List<MIDIOutput> added = mos;

                for(int i = 0; i < mos.Count; )
                { 
                    bool foundMatch = false;
                    for(int j = 0; j < deleted.Count; ++j)
                    { 
                        if(mos[i].Equivalent(deleted[j]) == true)
                        {
                            foundMatch = true;
                            mos.RemoveAt(i);
                            deleted.RemoveAt(j);
                        }
                    }
                    if(foundMatch == false)
                        ++i;
                }

                if(deleted.Count > 0 || added.Count > 0)
                { 
                    foreach(MIDIOutput mo in deleted)
                        this.knownOutputs.Remove(mo);

                    this.knownOutputs.AddRange(added);
                    this.onMIDIPollOutputUpdated?.Invoke(this, deleted, added);
                }
            }
        }

        if(this.currentInput != null)
        { 
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
            MIDIInputWin midiInWin = this.currentInput as MIDIInputWin;
            if(midiInWin != null)
            {

                if(MIDIMgr_IsHandleValid(midiInWin.DeviceID) == false)
                { 
                    Debug.Log("MIDI handle became invalid");
                    this.SetCurrentInput(null);
                    return;
                }

                uint msgCt = MIDIMgr_GetMIDIQueuedMessageCount(midiInWin.DeviceID);

                if(msgCt == 0)
                    return;

                // https://users.cs.cf.ac.uk/Dave.Marshall/Multimedia/node158.html
                for(uint i = 0; i < msgCt; ++i)
                { 
                    MIDIQueuedMessage mqm = MIDIMgr_PopMIDIQueuedMessage(midiInWin.DeviceID);
                    //Debug.Log( $"Got message {mqm.messageType} with payload {mqm.message}");

                    if(mqm.messageType == MIDIQueuedMessage.MessageType.Invalid)
                        continue;

                    // https://users.cs.cf.ac.uk/Dave.Marshall/Multimedia/node158.html

                    
                    if(mqm.messageType == MIDIQueuedMessage.MessageType.Message)
                    {
                        byte [] rb = System.BitConverter.GetBytes(mqm.message);
                        this.ProcessMIDIInputBytes(rb);
                    }
                }
            }
            
#elif UNITY_ANDROID
            MIDIInputAnd miiand = this.currentInput as MIDIInputAnd;
            int msgCt = miiand.GetMessageCount();
            for(int i = 0; i < msgCt; ++i)
            { 
                byte [] rb = miiand.PopMessage();
                this.ProcessMIDIInputBytes(rb);
            }
#endif
        }
    }

    void ProcessMIDIInputBytes(byte[] rb)
    { 
        // https://users.cs.cf.ac.uk/Dave.Marshall/Multimedia/node158.html
        // Not all decoding is supported.
        //
        // The plan is to add stuff as needed.
        //
        byte status = rb[0];
        int msg = ( status & 0xF0) >> 4;
        int channel = (status & 0x0F);

        this.onMIDIInputMsg?.Invoke(this);

        if(msg == 0x8) // Note off
        { 
            int key = rb[1];
            float vel = rb[2]/127.0f;

            foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnNoteOff(channel, key, vel);

        }
        else if(msg == 0x9) // Note on
        { 
            int key = rb[1];
            float vel = rb[2]/127.0f;

            foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnNoteOn(channel, key, vel);
        }
        else if(msg == 0xA) // Polyphonic Key Pressure
        { 
            int key = rb[1];
            float vel = rb[2]/127.0f;

            foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnKeyPressure(channel, key, vel);
        }
        else if(msg == 0xB) // Control Change
        { 
            int controllerNum = rb[1];
            int controllerVal = rb[2];

            if(controllerNum == 121)
            { }     // Reset all controllers
            else if(controllerNum == 122)
            { }     // Local control
            else if(controllerNum == 123)
            { }     // All notes off
            else if(controllerNum == 124)
            { }     // Omni mode off
            else if(controllerNum == 125)
            { }     // Moni mode on
            else if(controllerNum == 126)
            { }     // Mono mode on
            else if(controllerNum == 127)
            { }     // Poly mode off
            else
            {
                foreach(IMIDIMsgSink sink in this.midiDispatch)
                    sink.MIDIMsg_OnControlChange(channel, controllerNum, controllerVal);
            }
        }
        else if(msg == 0xC) //Program Change
        { 
            int programNum = rb[1];

            foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnProgramChange(channel, programNum);
        }
        else if(msg == 0xD) // Channel pressure
        { 
            float presVal = rb[1]/127.0f;

            foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnChannelPressure(presVal);
        }
        else if(msg == 0xE) // Pitch Bend
        { 
            float msb = rb[1] / 127.0f;
            float lsb = rb[2] / 127.0f;

            foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnPitchBend(msb, lsb);
        }
        else if(msg == 0xF) // System message
        { 
            // The channel value now represents the message type
            if(channel == 0x1) // MIDI Timing code
            { 
                foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnSystemMsg(this, MIDISystemMsg.TimingCode);
            }
            else if(channel == 0x2) // Song Position pointer
            { }
            else if(channel == 0x3) // Song Select
            { }
            else if(channel == 0x4)
            { }
            else if(channel == 0x5)
            { }
            else if(channel == 0x6) // Tune Request
            { }
            else if(channel == 0x7)
            { }
            else if(channel == 0x8) // Timing Clock
            { }
            else if(channel == 0x9)
            { }
            else if(channel == 0xA) // Start Sequence
            { 
                foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnSystemMsg(this, MIDISystemMsg.StartSequence);
            }
            else if(channel == 0xB) // Continue Sequence
            { }
            else if(channel == 0xC) // Stop Sequence
            { }
            else if(channel == 0xD)
            { }
            else if(channel == 0xE) // Active Sensing
            { }
            else if(channel == 0xF) // System Reset
            { 
                foreach(IMIDIMsgSink sink in this.midiDispatch)
                sink.MIDIMsg_OnSystemMsg(this, MIDISystemMsg.SystemReset);
            }

        }
    }

    private void OnDisable()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
        MIDIMgr_Clear();
#endif
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
        MIDIMgr_Clear();
#endif
    }

    public int GetChannel()
    { 
        return this.controllerChannel;
    }

    public void SetChannel(int channel)
    { 
        if(channel < 1)
            channel = 1;
        else if(channel > 16)
            channel = 16;

        if(this.controllerChannel != channel)
        {
            this.controllerChannel = channel;
            onMIDIChannelChanged?.Invoke(this, channel);
        }
    }

    public MIDIOutput GetCurrentOutput()
    { 
        return this.currentOutput;
    }

    public MIDIInput GetCurrentInput()
    { 
        return this.currentInput;
    }

    public bool SendKeyPress(int midiNote, int channel, float velocity)
    { 
        if(this.currentOutput == null)
            return false;

        sbyte vel = (sbyte)(Mathf.Clamp01(velocity) * 127.0f);

        if(this.currentOutput.SendKeyPress(this, midiNote, channel, vel) == true)
        {
            this.onMIDIControllerMsg?.Invoke(this);
            return true;
        }
        return false;
    }

    public bool SendKeyPress(int midiNote, float velocity)
    { 
        return this.SendKeyPress(midiNote, this.controllerChannel, velocity);
    }

    public bool SendKeyRelease(int midiNote, int channel)
    { 
        if(this.currentOutput == null)
            return false;

        if(this.currentOutput.SendKeyRelease(this, midiNote, channel) == true)
        { 
            this.onMIDIControllerMsg?.Invoke(this);
            return true;
        }
        return false;
    }

    public bool SendKeyRelease(int midiNote)
    { 
        return this.SendKeyRelease(midiNote, this.controllerChannel);
    }

    public bool SetCurrentOutput(MIDIOutput output)
    { 

        if(this.currentOutput == null && output == null)
            return false;

        if(this.currentOutput != null && output != null && this.currentOutput.Equivalent(output) == true)
            return false;

        if(this.currentOutput != null)
        {
            this.currentOutput.Deactivate();
            this.currentOutput = null;

            this.onMIDIControllerDisconnected?.Invoke(this);
        }

        MIDIOutput oldOutput = this.currentOutput;
        this.currentOutput = output;

        bool ret = true;
        if(this.currentOutput != null)
        { 
            // If the new output doesn't activate, we still need to return if 
            // anything changed or not - since that's what the return value is,
            // and not if the output actually activated.
            //
            // So if it wasn't null before and null when we exit (because of failed
            // activation) - even if that wasn't the input parameter, true should
            // be returned.
            if(this.currentOutput.Activate() == false)
            {
                this.currentOutput = null;
                ret = (oldOutput != null);
            }
            else
                this.onMIDIControllerConnected?.Invoke(this, this.currentOutput);
        }
        return ret;
    }

    public bool SetCurrentInput(MIDIInput input)
    { 
        if(this.currentInput == null && input == null)
            return false;

        if(this.currentInput != null && input != null && this.currentInput.Equivalent(input) == true)
            return false;

        if(this.currentInput != null)
        {
            this.currentInput.Deactivate();
            this.currentInput = null;

            this.onMIDIInputDisconnected?.Invoke(this);
        }
        
        MIDIInput oldInput = this.currentInput;
        this.currentInput = input;

        bool ret = true;
        if(this.currentInput != null)
        { 
            // If the new input doesn't activate, we still need to return if 
            // anything changed or not - since that's what the return value is,
            // and not if the input actually activated.
            //
            // So if it wasn't null before and null when we exit (because of failed
            // activation) - even if that wasn't the input parameter, true should
            // be returned.
            if(this.currentInput.Activate() == false)
            {
                this.currentInput = null;
                ret = (oldInput != null);
            }
            else
                this.onMIDIInputConnected?.Invoke(this, this.currentInput);
        }
        return ret;
    }

    public List<MIDIOutput> GetKnownOutputs()
    { 
        return this.knownOutputs;
    }

    public List<MIDIInput> GetKnownInputs()
    { 
        return this.knownInputs;
    }

    public void RefreshKnownInputs()
    {
        this.knownInputs = this.QueryInputs();
        this.onMIDIRefreshedInputs?.Invoke(this, this.knownInputs);
    }

    public void RefreshKnownOutputs()
    { 
        this.knownOutputs = this.QueryOutputs();
        this.onMIDIRefreshedOutputs?.Invoke(this, this.knownOutputs);
    }

    public static bool StrCompShort(short [] a, short [] b)
    { 
        if(a.Length != b.Length)
            return false;

        for(int i = 0; i < a.Length; ++i)
        { 
            if(a[i] != b[i])
                return false;

            if(a[i] == 0)
                return true;
        }

        return true;
    }

    #if UNITY_EDITOR || UNITY_ANDROID
    internal void _OnMIDIOutputConnected(MIDIOutputAnd output)
    { 
        lock(this.androidSyncs)
        { 
            AndroidMIDIEvent evt = new AndroidMIDIEvent();
            evt.eventType = AndroidSyncEvent.OutputConnected;
            evt.output = output;

            this.androidSyncs.Add(evt);
        }
    }
    #endif

    #if UNITY_EDITOR || UNITY_ANDROID
    internal void _OnMIDIInputConnected(MIDIInputAnd input)
    { 
        lock(this.androidSyncs)
        { 
            AndroidMIDIEvent evt = new AndroidMIDIEvent();
            evt.eventType = AndroidSyncEvent.InputConnected;
            evt.input = input;

            this.androidSyncs.Add(evt);
        }
    }
    #endif

    bool IMIDIPollCountering.IncrementInputPollCounter()
    { 
        ++this.pollingInputCounter;
        return true;
    }

    bool IMIDIPollCountering.DecrementInputPollCounter()
    { 
        --this.pollingInputCounter;

        if(this.pollingInputCounter < 0)
        { 
            this.pollingInputCounter = 0;
            return false;
        }

        return true;
    }

    bool IMIDIPollCountering.IncrementOutputPollCounter()
    { 
        ++this.pollingOutputCounter;
        return true;
    }

    bool IMIDIPollCountering.DecrementOutputPollCounter()
    { 
        --this.pollingOutputCounter;

        if(this.pollingOutputCounter < 0)
        { 
            this.pollingOutputCounter = 0;
            return false;
        }
        return true;
    }

    public void ClearDispatch()
    { 
        this.midiDispatch.Clear();
    }

    public int DispatchCount()
    { 
        return this.midiDispatch.Count;
    }

    public bool HasDispatch(IMIDIMsgSink sink)
    { 
        return this.midiDispatch.Contains(sink);
    }

    public bool AddDispatch(IMIDIMsgSink sink)
    { 
        return this.midiDispatch.Add(sink);
    }

    public bool RemoveDispatch(IMIDIMsgSink sink)
    { 
        return this.midiDispatch.Remove(sink);
    }
}
