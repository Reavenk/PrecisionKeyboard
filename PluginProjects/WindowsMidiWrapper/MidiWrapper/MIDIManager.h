#pragma once

#include <vector>
#include <map>
#include <queue>

#include <memory>
#include <mutex>

#include <Mmsystem.h>

typedef int MidiMgrHandle;
static const MidiMgrHandle errHandle = -1;

extern "C"
{
    enum class MessageType
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
    };

    struct MIDIQueuedMessage
    {
        MessageType messageType;
        DWORD_PTR dw;
        DWORD_PTR timestamp;

        static MIDIQueuedMessage Empty()
        {
            MIDIQueuedMessage msg;
            msg.messageType = MessageType::Invalid;
            msg.timestamp = -1;
            return msg;
        }

        static MIDIQueuedMessage Opened(DWORD_PTR timestamp)
        {
            MIDIQueuedMessage msg;
            msg.messageType = MessageType::Open;
            msg.timestamp = timestamp;
            return msg;
        }

        static MIDIQueuedMessage Closed(DWORD_PTR timestamp)
        {
            MIDIQueuedMessage msg;
            msg.messageType = MessageType::Open;
            msg.timestamp = timestamp;
            return msg;
        }

        static MIDIQueuedMessage Message(DWORD_PTR dw, DWORD_PTR timestamp)
        {
            MIDIQueuedMessage msg;
            msg.messageType = MessageType::Message;
            msg.dw = dw;
            msg.timestamp = timestamp;

            return msg;
        }

        static MIDIQueuedMessage Bank(MessageType mt, DWORD_PTR dw, DWORD_PTR timestamp)
        {
            MIDIQueuedMessage msg;
            msg.messageType     = mt;
            msg.dw              = dw;
            msg.timestamp       = timestamp;

            return msg;

        }
    };

    struct MIDIDevice
    {

        static const int MaxPNameLen = 32;
        static const int MaxErrorLen = 256;

        unsigned int port;
        unsigned int error;

        unsigned short manufactureId;
        unsigned short productId;
        unsigned int driverVersion;
        wchar_t name[MaxPNameLen];
        unsigned int support;
    };

    MIDIDevice MIDIDevice_FromPort(int port);
    MIDIDevice MIDIDevice_Copy(const MIDIDevice& md);
}


struct MIDIConnectionLog
{
    HMIDIIN midiHandle;
    MidiMgrHandle managerHandle;

    int port;
    std::queue<MIDIQueuedMessage> queuedMessages;
};

class MIDIManager
{
public:
    static MidiMgrHandle conHandleCtr;

private:
    MidiMgrHandle AllocateHandle()
    {
        MidiMgrHandle ret = conHandleCtr;
        ++conHandleCtr;
        return ret;
    }

private:
    std::map< HMIDIIN, MIDIConnectionLog*> connectionByMIDI;
    std::map< MidiMgrHandle, MIDIConnectionLog*> connectionsByMgrHandle;

    std::mutex mutexMaps;
    std::mutex mutexDestruction;
    bool destroyed = false;
public:

    std::mutex& DestructionMutex()
    {
        return this->mutexDestruction;
    }

    bool Destroyed()
    {
        return this->destroyed;
    }

    //static std::vector<MIDIDevice> QueryDevices();

    MidiMgrHandle OpenPort(int port);

    bool IsPortOpen(int port);
    bool IsHandleOpenMgr(MidiMgrHandle handle);
    bool IsHandleOpenMIDI(HMIDIIN handle);

    bool ClosePort(int port);
    bool CloseHandleMgr(MidiMgrHandle handle);
    bool CloseHandleMIDI(HMIDIIN handle);

    bool HasMessagesMgr(MidiMgrHandle handle);
    bool HasMessagesMIDI(HMIDIIN handle);
    size_t MessageQueueCountMgr(MidiMgrHandle handle);
    size_t MessageQueueCountMIDI(HMIDIIN handle);

    bool AddMessageMgr(MidiMgrHandle handle, const MIDIQueuedMessage& msg);
    bool AddMessageMIDI(HMIDIIN handle, const MIDIQueuedMessage& msg);
    MIDIQueuedMessage PopMessageMgr(MidiMgrHandle handle);
    MIDIQueuedMessage PopMessageMIDI(HMIDIIN handle);

    void Clear();
    ~MIDIManager();

private:
    MIDIQueuedMessage _PopMessage(MIDIConnectionLog * pcl);

};

void CALLBACK MidiInProc(HMIDIIN hMidiIn, UINT wMsg, DWORD_PTR dwInstance, DWORD_PTR dwParam1, DWORD_PTR dwParam2);
