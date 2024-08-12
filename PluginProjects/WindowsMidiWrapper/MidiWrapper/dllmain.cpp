// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"


#include "pch.h"
#include "MIDIManager.h"

MIDIManager midiMgr;

BOOL APIENTRY DllMain( 
    HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C"
{
    __declspec(dllexport) void __cdecl MIDIMgr_Clear(void)
    {
        midiMgr.Clear();
    }

    __declspec(dllexport) int __cdecl MIDIMgr_GetMIDIDeviceCount()
    {
        UINT nMidiDeviceNum = midiInGetNumDevs();
        return nMidiDeviceNum;
    }

    __declspec(dllexport) MIDIDevice __cdecl MIDIMgr_GetMIDIDevice(int deviceNum)
    {
        MIDIDevice dev = MIDIDevice_FromPort(deviceNum);
        return dev;
    }

    __declspec(dllexport) MidiMgrHandle __cdecl MIDIMgr_OpenMIDIDevice(int deviceNum)
    {
        MidiMgrHandle ret = midiMgr.OpenPort(deviceNum);
        return ret;
    }

    __declspec(dllexport) bool __cdecl MIDIMgr_CloseMIDIDevice(MidiMgrHandle handle)
    {
        return midiMgr.CloseHandleMgr(handle);
    }

    __declspec(dllexport) bool __cdecl MIDIMgr_MIDIHasQueuedMessages(MidiMgrHandle handle)
    {
        return midiMgr.HasMessagesMgr(handle);
    }

    __declspec(dllexport) size_t __cdecl MIDIMgr_GetMIDIQueuedMessageCount(MidiMgrHandle handle)
    {
        return midiMgr.MessageQueueCountMgr(handle);
    }

    __declspec(dllexport) MIDIQueuedMessage __cdecl MIDIMgr_PopMIDIQueuedMessage(MidiMgrHandle handle)
    {
        return midiMgr.PopMessageMgr(handle);
    }

    __declspec(dllexport) bool __cdecl MIDIMgr_IsHandleValid(MidiMgrHandle handle)
    {
        return midiMgr.IsHandleOpenMgr(handle);
    }
}