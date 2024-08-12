#include "pch.h"
#include "MIDIManager.h"

extern "C"
{
    int MIDIManager::conHandleCtr = 0;

    MIDIDevice MIDIDevice_FromPort(int port)
    {
        MIDIINCAPS caps;
        MMRESULT mres = midiInGetDevCaps(port, &caps, sizeof(MIDIINCAPS));

        MIDIDevice ret;
        ret.port = port;
        ret.error = mres;

        memset(ret.name, 0, sizeof(ret.name));

        if (mres == MMSYSERR_NOERROR)
        {
            ret.manufactureId = caps.wMid;
            ret.productId = caps.wPid;
            ret.driverVersion = caps.vDriverVersion;
            ret.support = caps.dwSupport;

            wcsncpy_s(ret.name, caps.szPname, MIDIDevice::MaxPNameLen);
        }
        else
        {
            ret.manufactureId = 0;
            ret.productId = 0;
            ret.driverVersion = 0;
            ret.support = 0;

            memset(ret.name, 0, MIDIDevice::MaxPNameLen);
        }

        return ret;
    }

    MIDIDevice MIDIDevice_Copy(const MIDIDevice& md)
    {
        MIDIDevice ret;

        ret.port = md.port;
        ret.error = md.error;
        ret.manufactureId = md.manufactureId;
        ret.productId = md.productId;
        ret.driverVersion = md.driverVersion;
        ret.support = md.support;
        wcsncpy_s(ret.name, md.name, MIDIDevice::MaxPNameLen);

        return ret;
    }

    //std::vector<MIDIDevice> MIDIManager::QueryDevices()
    //{
    //    std::vector<MIDIDevice> ret;
    //
    //    UINT nMidiDeviceNum = midiInGetNumDevs();
    //
    //    for (unsigned int i = 0; i < nMidiDeviceNum; ++i)
    //    {
    //        MIDIDevice midiDev(i);
    //        ret.push_back(midiDev);
    //    }
    //
    //    return ret;
    //}
}

MidiMgrHandle MIDIManager::OpenPort(int port)
{

    MIDIConnectionLog* pcl = new MIDIConnectionLog();

    HMIDIIN hMidiDevice = nullptr;
    MMRESULT rv =
        midiInOpen(
            &hMidiDevice,
            port,
            (DWORD_PTR)MidiInProc,
            (DWORD_PTR)this,
            CALLBACK_FUNCTION);

    if (rv != MMSYSERR_NOERROR)
    {
        delete pcl;
        return errHandle;
    }


    // We can't lock until the MIDI channel is open because that will try to add an
    // Open message which would then block and lock.
    //const std::lock_guard<std::mutex> lock(this->mutexMaps);

    pcl->managerHandle = AllocateHandle();
    pcl->midiHandle = hMidiDevice;
    pcl->port = port;
    //
    this->connectionByMIDI[pcl->midiHandle] = pcl;
    this->connectionsByMgrHandle[pcl->managerHandle] = pcl;

    midiInStart(pcl->midiHandle);

    return pcl->managerHandle;
}

bool MIDIManager::IsPortOpen(int port)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    for (
        auto it = connectionsByMgrHandle.begin();
        it != this->connectionsByMgrHandle.end();
        ++it)
    {
        if (it->second->port == port)
            return true;
    }
    return false;
}

bool MIDIManager::IsHandleOpenMgr(MidiMgrHandle handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);
    return this->connectionsByMgrHandle.find(handle) != this->connectionsByMgrHandle.end();
}

bool MIDIManager::IsHandleOpenMIDI(HMIDIIN handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);
    return this->connectionByMIDI.find(handle) != this->connectionByMIDI.end();
}

bool MIDIManager::ClosePort(int port)
{
    this->mutexMaps.lock();
    for (
        auto it = connectionsByMgrHandle.begin();
        it != this->connectionsByMgrHandle.end();
        ++it)
    {
        if (it->second->port != port)
            continue;

        MidiMgrHandle handle = it->first;
        this->mutexMaps.unlock();

        this->CloseHandleMgr(handle);
    }
    this->mutexMaps.unlock();
    return false;
}

bool MIDIManager::CloseHandleMgr(MidiMgrHandle handle)
{
    this->mutexMaps.lock();

    auto it = this->connectionsByMgrHandle.find(handle);

    if (it == this->connectionsByMgrHandle.end())
    {
        this->mutexMaps.unlock();
        return false;
    }

    MIDIConnectionLog* pcl = it->second;
    bool rm1 = this->connectionsByMgrHandle.erase(pcl->managerHandle);
    bool rm2 = this->connectionByMIDI.erase(pcl->midiHandle);

    this->mutexMaps.unlock();

    midiInStop(pcl->midiHandle);
    MMRESULT mr = midiInClose(pcl->midiHandle);

    delete pcl;

    return
        rm1 == true &&
        rm2 == true &&
        mr == MMSYSERR_NOERROR;
}

bool MIDIManager::CloseHandleMIDI(HMIDIIN handle)
{
    this->mutexMaps.lock();

    auto it = this->connectionByMIDI.find(handle);

    if (it == this->connectionByMIDI.end())
    {
        this->mutexMaps.unlock();
        return false;
    }

    MIDIConnectionLog* pcl = it->second;
    bool rm1 = this->connectionsByMgrHandle.erase(pcl->managerHandle);
    bool rm2 = this->connectionByMIDI.erase(pcl->midiHandle);

    this->mutexMaps.unlock();

    midiInStop(pcl->midiHandle);
    MMRESULT mr = midiInClose(pcl->midiHandle);

    delete pcl;

    return
        rm1 == true &&
        rm2 == true &&
        mr == MMSYSERR_NOERROR;
}

bool MIDIManager::HasMessagesMgr(MidiMgrHandle handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    auto it = this->connectionsByMgrHandle.find(handle);

    if (it == this->connectionsByMgrHandle.end())
        return false;

    return it->second->queuedMessages.empty() == false;
}

bool MIDIManager::HasMessagesMIDI(HMIDIIN handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    auto it = this->connectionByMIDI.find(handle);

    if (it == this->connectionByMIDI.end())
        return false;

    return it->second->queuedMessages.empty() == false;
}

size_t MIDIManager::MessageQueueCountMgr(MidiMgrHandle handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    auto it = this->connectionsByMgrHandle.find(handle);

    if (it == this->connectionsByMgrHandle.end())
        return 0;

    return it->second->queuedMessages.size();
}

size_t MIDIManager::MessageQueueCountMIDI(HMIDIIN handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    auto it = this->connectionByMIDI.find(handle);

    if (it == this->connectionByMIDI.end())
        return 0;

    return it->second->queuedMessages.size();
}

bool MIDIManager::AddMessageMgr(MidiMgrHandle handle, const MIDIQueuedMessage& msg)
{
    bool rm = (msg.messageType == MessageType::Close);

    this->mutexMaps.lock();

    auto it = this->connectionsByMgrHandle.find(handle);

    if (it == this->connectionsByMgrHandle.end())
    {
        this->mutexMaps.unlock();
        return false;
    }

    it->second->queuedMessages.push(msg);

    this->mutexMaps.unlock();

    if (rm == true)
        this->CloseHandleMgr(handle);

    return true;
}

bool MIDIManager::AddMessageMIDI(HMIDIIN handle, const MIDIQueuedMessage& msg)
{
    bool rm = (msg.messageType == MessageType::Close);

    this->mutexMaps.lock();

    auto it = this->connectionByMIDI.find(handle);

    if (it == this->connectionByMIDI.end())
    {
        this->mutexMaps.unlock();
        return false;
    }

    it->second->queuedMessages.push(msg);

    this->mutexMaps.unlock();

    if (rm == true)
        this->CloseHandleMIDI(handle);

    return true;
}

MIDIQueuedMessage MIDIManager::PopMessageMgr(MidiMgrHandle handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    auto it = this->connectionsByMgrHandle.find(handle);

    if (it == this->connectionsByMgrHandle.end())
        return MIDIQueuedMessage::Empty();

    return this->_PopMessage(it->second);
}

MIDIQueuedMessage MIDIManager::PopMessageMIDI(HMIDIIN handle)
{
    const std::lock_guard<std::mutex> lock(this->mutexMaps);

    auto it = this->connectionByMIDI.find(handle);

    if (it == this->connectionByMIDI.end())
        return MIDIQueuedMessage::Empty();

    return this->PopMessageMgr(it->second->managerHandle);
}

MIDIQueuedMessage MIDIManager::_PopMessage(MIDIConnectionLog * pcl)
{
    if (pcl->queuedMessages.size() == 0)
        return MIDIQueuedMessage::Empty();
    {
        MIDIQueuedMessage ret = pcl->queuedMessages.front();
        pcl->queuedMessages.pop();
        return ret;
    }
}

void MIDIManager::Clear()
{
    this->mutexMaps.lock();

    auto oldInfo = this->connectionsByMgrHandle;

    this->connectionsByMgrHandle.clear();
    this->connectionByMIDI.clear();

    this->mutexMaps.unlock();

    for (auto it = oldInfo.begin(); it != oldInfo.end(); ++it)
    {
        MIDIConnectionLog * s = it->second;
        midiInStop(s->midiHandle);
        midiInClose(s->midiHandle);
        delete s;
    }
}

MIDIManager::~MIDIManager()
{
    //const std::lock_guard<std::mutex> lock(this->mutexDestruction);
    this->destroyed = true;

    this->Clear();
}

void CALLBACK MidiInProc(HMIDIIN hMidiIn, UINT wMsg, DWORD_PTR dwInstance, DWORD_PTR dwParam1, DWORD_PTR dwParam2)
{
    MIDIManager* pmgr = (MIDIManager*)dwInstance;
    
    if (pmgr->Destroyed() == true)
        return;

    const std::lock_guard<std::mutex> lock(pmgr->DestructionMutex());
    
    if (pmgr->Destroyed() == true)
        return;

    switch (wMsg)
    {
    case MIM_OPEN:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Opened(dwParam2));
        break;

    case MIM_CLOSE:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Closed(dwParam2));
        break;
    case MIM_DATA:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Message(dwParam1, dwParam2));
        break;
    case MIM_LONGDATA:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Bank(MessageType::LongData, dwParam1, dwParam2));
        break;
    case MIM_ERROR:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Bank(MessageType::Error, dwParam1, dwParam2));
        break;
    case MIM_LONGERROR:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Bank(MessageType::LongError, dwParam1, dwParam2));
        break;
    case MIM_MOREDATA:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Bank(MessageType::MoreData, dwParam1, dwParam2));
        break;
    default:
        pmgr->AddMessageMIDI(hMidiIn, MIDIQueuedMessage::Bank(MessageType::Unknown, dwParam1, dwParam2));
        break;
    }
    return;
}