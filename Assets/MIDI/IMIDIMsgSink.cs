using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MIDISystemMsg
{ 
    TimingClock,
    StartSequence,
    ContinueSequence,
    StopSequence,
    ActiveSensing,
    SystemReset,

    TimingCode,
    SongPosition,
    SongSelect,
    TuneRequest
}

public interface IMIDIMsgSink
{
    void MIDIMsg_OnSystemMsg(MIDIMgr mgr, MIDISystemMsg msg);

    void MIDIMsg_OnNoteOff(int channel, int keyNum, float velocity);
    void MIDIMsg_OnNoteOn(int channel, int keyNum, float velocity);
    void MIDIMsg_OnKeyPressure(int channel, int keyNum, float velocity);
    void MIDIMsg_OnControlChange(int channel, int controllerNum, int controllerValue);
    void MIDIMsg_OnProgramChange(int channel, int program);
    void MIDIMsg_OnChannelPressure(float pressure);
    void MIDIMsg_OnPitchBend(float msb, float lsb);
}
