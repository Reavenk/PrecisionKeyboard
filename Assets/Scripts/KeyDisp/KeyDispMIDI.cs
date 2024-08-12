using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDispMIDI : KeyDispBase
{
    struct MIDIKeyPressRecord
    { 
        public int channel;
        public int handle;
        public int note;
    }

    const string prefMIDI_Channel = "prefMidiChannel";

    public const float meterLitTime = 0.5f;

    public UnityEngine.UI.Image inputMeter;
    public UnityEngine.UI.Text inputText;

    public UnityEngine.UI.Image outputMeter;
    public UnityEngine.UI.Text outputText;

    public PulldownInfo channelButton;
    public PulldownInfo inputButton;
    public PulldownInfo outputButton;

    public MIDIMgr midiMgr;

    TweenUtil tweenUtil = null; // TODO: In the end, this may not be needed.

    public float ? lastInputMsgTime;
    public float ? lastControllerMsgTime;

    public Color meterColorDisc = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    //
    public Color meterColorLow = new Color(0.15f, 0.3f, 0.15f, 1.0f);
    public Color meterColorHigh = new Color(0.5f, 1.0f, 0.5f, 1.0f);

    public Sprite channelIconMusic;
    public Sprite channelIconDrum;

    public Sprite iconPower;
    public Sprite iconInput;
    public Sprite iconOutput;

    bool initRefreshInputs = false;
    bool initRefreshOutputs = false;

    // The keys that are currently pressed down and the channel that 
    // was used to press the key.
    Dictionary<int, MIDIKeyPressRecord> pressedOutputs = 
        new Dictionary<int, MIDIKeyPressRecord>();

    public List<GameObject> hideForWindows;

    public override void Init(Application app, PaneKeyboard keyboard)
    { 
        base.Init(app, keyboard);

        // We set the Application as the host because we'll need animations
        // that started to finish, even if the MIDI pane get switched away before
        // that happens.
        this.tweenUtil = new TweenUtil(app);

        int startingChannel = this.midiMgr.GetChannel();

        int prefChannel = PlayerPrefs.GetInt(prefMIDI_Channel, startingChannel);
        this.midiMgr.SetChannel(prefChannel);

        Sprite midiIcon;
        string midiLabel;
        GetChannelInfo(startingChannel, out midiIcon, out midiLabel);
        this.channelButton.icon.sprite = midiIcon;
        this.channelButton.text.text = midiLabel;

        this.midiMgr.onMIDIChannelChanged           += this.MIDICallback_OnChannelChanged;
        this.midiMgr.onMIDIControllerConnected      += this.MIDICallback_onControllerConnected;
        this.midiMgr.onMIDIControllerDisconnected   += this.MIDICallback_onControllerDisconnected;
        this.midiMgr.onMIDIControllerMsg            += this.MIDICallback_onControllerMsg;
        this.midiMgr.onMIDIInputConnected           += this.MIDICallback_OnInputConnected;
        this.midiMgr.onMIDIInputDisconnected        += this.MIDICallback_OnInputDisconnected;
        this.midiMgr.onMIDIInputMsg                 += this.MIDICallback_OnInputMsg;
        this.midiMgr.onMIDIRefreshedInputs          += this.MIDICallback_OnInputRefreshed;
        this.midiMgr.onMIDIRefreshedOutputs         += this.MIDICallback_OnOutputRefreshed;

        this.inputButton.text.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        this.inputButton.text.text = "Unconnected";
        this.outputButton.text.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        this.outputButton.text.text = "Unconnected";

#if DEPLOYMENT && !UNITY_ANDROID
        if(this.hideForWindows != null)
        { 
            foreach(GameObject go in this.hideForWindows)
                go.SetActive(false);
        }
#endif
    }


    public override bool AvailableDuringExercise(PaneKeyboard.ExerciseDisplayMode exDisp)
    {
        return true;
    }

    public override void OnShow(bool exercise)
    { 
        base.OnShow(exercise);

        if(this.initRefreshInputs == false)
        { 
            this.initRefreshInputs = true;
            this.midiMgr.RefreshKnownInputs();
        }

        if(this.initRefreshOutputs == false)
        { 
            this.initRefreshOutputs = true;
            this.midiMgr.RefreshKnownOutputs();
        }
    }

    private void Update()
    {
        //      INPUT METER
        //////////////////////////////////////////////////
        if(this.lastInputMsgTime.HasValue == false)
            this.inputMeter.color = this.meterColorDisc;
        else
        { 
            float lambda = 
                Time.time - this.lastInputMsgTime.Value;

            lambda = Mathf.Clamp01(lambda);
            this.inputMeter.color = 
                Color.Lerp(
                    this.meterColorHigh,
                    this.meterColorLow,
                    lambda);
        }

        //      CONTROLLER METER
        //////////////////////////////////////////////////
        if(this.lastControllerMsgTime.HasValue == false)
            this.outputMeter.color = this.meterColorDisc;
        else
        { 
            float lambda = 
                Time.time - this.lastControllerMsgTime.Value;

            lambda = Mathf.Clamp01(lambda);
            this.outputMeter.color = 
                Color.Lerp(
                    this.meterColorHigh, 
                    this.meterColorLow, 
                    lambda);
        }
    }

    public void OnButton_Channel()
    {
        this.app.DoVibrateButton();

        PxPre.DropMenu.StackUtil dropStack = new PxPre.DropMenu.StackUtil();

        int curChan = this.midiMgr.GetChannel();
        dropStack.AddAction(curChan == 1,   this.channelIconMusic,  "Channel 01", ()=>{ this.OnMenu_SetChannel(1); });
        dropStack.AddAction(curChan == 2,   this.channelIconMusic,  "Channel 02", ()=>{ this.OnMenu_SetChannel(2); });
        dropStack.AddAction(curChan == 3,   this.channelIconMusic,  "Channel 03", ()=>{ this.OnMenu_SetChannel(3); });
        dropStack.AddAction(curChan == 4,   this.channelIconMusic,  "Channel 04", ()=>{ this.OnMenu_SetChannel(4); });
        dropStack.AddAction(curChan == 5,   this.channelIconMusic,  "Channel 05", ()=>{ this.OnMenu_SetChannel(5); });
        dropStack.AddAction(curChan == 6,   this.channelIconMusic,  "Channel 06", ()=>{ this.OnMenu_SetChannel(6); });
        dropStack.AddAction(curChan == 7,   this.channelIconMusic,  "Channel 07", ()=>{ this.OnMenu_SetChannel(7); });
        dropStack.AddAction(curChan == 8,   this.channelIconDrum,   "Channel 08", ()=>{ this.OnMenu_SetChannel(8); });
        dropStack.AddAction(curChan == 9,   this.channelIconMusic,  "Channel 09", ()=>{ this.OnMenu_SetChannel(9); });
        dropStack.AddAction(curChan == 10,  this.channelIconMusic,  "Channel 10", ()=>{ this.OnMenu_SetChannel(10); });
        dropStack.AddAction(curChan == 11,  this.channelIconMusic,  "Channel 11", ()=>{ this.OnMenu_SetChannel(11); });
        dropStack.AddAction(curChan == 12,  this.channelIconMusic,  "Channel 12", ()=>{ this.OnMenu_SetChannel(12); });
        dropStack.AddAction(curChan == 13,  this.channelIconMusic,  "Channel 13", ()=>{ this.OnMenu_SetChannel(13); });
        dropStack.AddAction(curChan == 14,  this.channelIconMusic,  "Channel 14", ()=>{ this.OnMenu_SetChannel(14); });
        dropStack.AddAction(curChan == 15,  this.channelIconMusic,  "Channel 15", ()=>{ this.OnMenu_SetChannel(15); });
        dropStack.AddAction(curChan == 16,  this.channelIconMusic,  "Channel 16", ()=>{ this.OnMenu_SetChannel(16); });

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            dropStack.Root,
            this.channelButton.rootPlate);
    }

    public void OnButton_Input()
    { 
        this.app.DoVibrateButton();

        DialogInputMIDI.Create(
            this.app, 
            this.midiMgr, 
            this.inputButton.rootPlate,
            this);

        this.midiMgr.RefreshKnownInputs();
    }

    public void OnButton_Output()
    { 
        this.app.DoVibrateButton();

        DialogControllerMIDI.Create(
            this.app,
            this.midiMgr,
            this.outputButton.rootPlate,
            this);

        this.midiMgr.RefreshKnownOutputs();
    }

    void OnMenu_SetChannel(int newChannel)
    { 
        this.midiMgr.SetChannel(newChannel);
    }

    void MIDICallback_onControllerConnected(MIDIMgr midimgr, MIDIOutput newOutput)
    { 
        if(newOutput == null)
        { 
            this.lastControllerMsgTime = null;
            this.outputText.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            this.outputButton.SetInfo(this.app, "Unconnected", false);
        }
        else
        { 
            this.lastControllerMsgTime = -99.0f;
            this.outputText.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            this.outputButton.SetInfo(this.app, newOutput.DeviceName(), false);
        }
    }

    void MIDICallback_onControllerDisconnected(MIDIMgr midimgr)
    { 
        this.lastControllerMsgTime = null;
        this.outputText.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        this.outputButton.SetInfo(this.app, "Unconnected", false);
    }

    void MIDICallback_onControllerMsg(MIDIMgr midimgr)
    { 
        this.lastControllerMsgTime = Time.time;
    }

    void MIDICallback_OnInputMsg(MIDIMgr midimgr)
    { 
        //DialogInputMIDI.Create(this.app, this.midiMgr, this.inputButton.rootPlate);
        this.lastInputMsgTime = Time.time;
    }

    void MIDICallback_OnInputConnected(MIDIMgr midimgr, MIDIInput newInput)
    { 
        if(newInput == null)
        { 
            this.lastInputMsgTime = null;
            this.inputText.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            this.inputButton.SetInfo(this.app, "Unconnected", false);
        }
        else
        { 
            this.lastInputMsgTime = -99.0f;
            this.inputText.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            this.inputButton.SetInfo(this.app, newInput.DeviceName(), false);
        }
    }

    void MIDICallback_OnInputDisconnected(MIDIMgr midimgr)
    { 
        this.lastInputMsgTime = null;
        this.inputText.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        this.inputButton.SetInfo(this.app, "Unconnected", false);
    }

    void MIDICallback_OnInputRefreshed(MIDIMgr midiMgr, List<MIDIInput> newInputs)
    {
        this.initRefreshInputs = true;
    }

    void MIDICallback_OnOutputRefreshed(MIDIMgr midiMgr, List<MIDIOutput> newOutputs)
    { 
        this.initRefreshOutputs = true;
    }

    void MIDICallback_OnChannelChanged(MIDIMgr midimgr, int newChannel)
    { 
        string label;
        Sprite icon;

        this.GetChannelInfo(newChannel, out icon, out label);

        channelButton.SetInfo(this.app, icon, label, false);
    }

    void GetChannelInfo(int channel, out Sprite sp, out string label)
    { 
        sp = this.channelIconMusic;
        if(channel == 8)
            sp = this.channelIconDrum;

        label = channel.ToString("00");
    }

    public override void OnNoteStart(Application.NoteStartEvent eventType, int noteID, float velocity, int handle)
    {
        if(eventType != Application.NoteStartEvent.Pressed)
            return;

        // C4 is 60... So lets convert out note id to MIDI
        int c4ID = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 4);

        noteID += (60 - c4ID);

        //velocity *= this.app.MasterVolume();

        int channel = this.midiMgr.GetChannel();
        if(this.midiMgr.SendKeyPress(noteID, channel, velocity) == false)
            return;

        MIDIKeyPressRecord pressRecord = new MIDIKeyPressRecord();
        pressRecord.channel = channel;
        pressRecord.handle = handle;
        pressRecord.note = noteID;

        this.pressedOutputs[handle] = pressRecord;
    }

    public override void OnNoteEnd(int handle)
    {
        MIDIKeyPressRecord pressRecord;
        if(this.pressedOutputs.TryGetValue(handle, out pressRecord) == false)
            return;

        this.midiMgr.SendKeyRelease(pressRecord.note, pressRecord.channel);
    }

    public override void OnEStop()
    {
        MIDIKeyPressRecord [] pressesToClear = (new List<MIDIKeyPressRecord>(this.pressedOutputs.Values)).ToArray();
        this.pressedOutputs.Clear();

        foreach(MIDIKeyPressRecord mkr in pressesToClear)
            this.midiMgr.SendKeyRelease(mkr.note, mkr.channel);
    }
}
