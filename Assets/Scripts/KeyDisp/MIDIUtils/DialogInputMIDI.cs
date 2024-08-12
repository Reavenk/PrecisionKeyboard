using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogInputMIDI : 
    MonoBehaviour,
    IConnectionButtonBridge
{
    public class ConnectionButton : ConnectionButtonBase<MIDIInput>
    { }

    public Application app;
    public MIDIMgr midiMgr;
    public PxPre.UIL.Dialog dlg;

    PushdownButton refreshButton;

    ConnectionButton disconnectButton = new ConnectionButton();
    //
     List<ConnectionButton> inputButtons = 
        new List<ConnectionButton>();

    // The last knowledge we have of what's connected. It's used to track
    // what's highlighted green so we know what to "de-green-ify" when
    // a disconnection happens.
    MIDIInput lastSelectedInput = null;

    PxPre.UIL.EleBoxSizer entriesSizer = null;
    PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> entriesScroll = null;

    UnityEngine.UI.Image powerIcon = null;

    public const int buttonTitleFontSize = 20;
    public const int buttonInfoFontSize = 16;

    Coroutine dirtyCoroutine = null;

    public static Color GetSelectedColor() => new Color(0.5f, 1.0f, 0.5f);
    public static Color GetUnselectedColor() => Color.white;

    // We don't actually do anything functional with it, but we use the reference
    // to pull certain icon assets from it. We may want to encapsulate it better
    // to ensure that we limit the variable to that.
    public KeyDispMIDI midiTab;

    public const int tipFontSize = 10;

    public MIDIInputPollScope pollScope = null;

    public static DialogInputMIDI Create(Application app, MIDIMgr midiMgr, RectTransform invoking, KeyDispMIDI midiTab)
    { 
        PxPre.UIL.Dialog dlg = 
            app.dlgSpawner.CreateDialogTemplate( 
                new Vector2(650.0f, 0.0f), 
                PxPre.UIL.LFlag.GrowVertOnCollapse|PxPre.UIL.LFlag.AlignCenter, 0.0f);

        DialogInputMIDI dimidi = 
            dlg.host.RT.gameObject.AddComponent<DialogInputMIDI>();

        dimidi.app          = app;
        dimidi.midiMgr      = midiMgr;
        dimidi.dlg          = dlg;
        dimidi.midiTab      = midiTab;
        dimidi.pollScope    = new MIDIInputPollScope(midiMgr);

        dimidi.InitDialog();

        dlg.host.LayoutInRTSmartFit();
        app.SetupDialogForTransition(dlg, invoking, true, true);

        return dimidi;
    }

    private void OnDestroy()
    {
        this.midiMgr.onMIDIRefreshedInputs -= this.OnMIDIRefreshed;
        this.midiMgr.onMIDIInputConnected -= this.OnMIDIInputConnected;
        this.midiMgr.onMIDIInputDisconnected -= this.OnMIDIInputDisconnected;
        this.midiMgr.onMIDIPollInputUpdated -= this.OnMIDIInputPollingChange;

        if(this.pollScope != null)
            this.pollScope = null;
    }

    public void InitDialog()
    {
        this.dlg.AddDialogTemplateTitle("MIDI Input");

        this.dlg.AddDialogTemplateSeparator();
        this.dlg.AddDialogTemplateButtons(
            new PxPre.UIL.DlgButtonPair(
                "Close", 
                (x)=>
                {  
                    GameObject.Destroy(this.gameObject);
                    return true;
                }));

        PxPre.UIL.UILStack uiStack = 
            new PxPre.UIL.UILStack(
                this.app.uiFactory, 
                this.dlg.rootParent, 
                this.dlg.contentSizer);

        uiStack.AddText(
            "<i>A list of identified MIDI devices. An input device can send messages to control the application.</i>",
            14,
            true,
            0.0f, PxPre.UIL.LFlag.GrowHoriz);

        uiStack.PushImage(this.app.exerciseAssets.scrollRectBorder, 1.0f, PxPre.UIL.LFlag.Grow).Chn_SetImgSliced();
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);

            this.entriesScroll = uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>( 1.0f, PxPre.UIL.LFlag.GrowVertOnCollapse|PxPre.UIL.LFlag.GrowHoriz);
            this.entriesSizer = uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
        
                this.disconnectButton.button = uiStack.PushButton<PushdownButton>("", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                this.disconnectButton.button.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
                this.disconnectButton.button.border = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);
                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow);
                    this.powerIcon = uiStack.AddImage(this.midiTab.iconPower, 0.0f, 0).Img;
                    uiStack.AddHorizSpace(10.0f, 0.0f, 0);
                    uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                        this.disconnectButton.title = uiStack.AddText("Disconnected", buttonTitleFontSize, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                        this.disconnectButton.title.text.fontStyle = FontStyle.Bold;
                        uiStack.AddVertSpace(10.0f, 0.0f, 0);
                        this.disconnectButton.descr = uiStack.AddText("The device is currently not connected to any MIDI input.", buttonInfoFontSize, true, 1.0f, PxPre.UIL.LFlag.Grow|PxPre.UIL.LFlag.AlignVertCenter);
                        this.disconnectButton.button.Button.onClick.AddListener( this.OnButton_Disconnect);
                    uiStack.Pop();
                uiStack.Pop();
                uiStack.Pop();

            uiStack.Pop();
            uiStack.Pop();

        uiStack.Pop();
        uiStack.Pop();

        this.refreshButton = uiStack.AddButton<PushdownButton>("Refresh", 0.0f, PxPre.UIL.LFlag.GrowHoriz).Button;
        this.refreshButton.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
        this.refreshButton.onClick.AddListener(this.OnButton_Refresh);

        UpdateDisconnectButton(this.midiMgr.GetCurrentInput() != null);            

        foreach(MIDIInput mi in this.midiMgr.GetKnownInputs())
            AddInputButton(mi);

        MIDIInput cur = this.midiMgr.GetCurrentInput();
         if(cur != null)
        {
            foreach(ConnectionButton cb in this.inputButtons)
            { 
                if(cb.payload.Equivalent(cur) == true)
                { 
                    cb.SetSelected(true, this);
                    break;
                }
            }
        }

        this.midiMgr.onMIDIRefreshedInputs += this.OnMIDIRefreshed;
        this.midiMgr.onMIDIInputConnected += this.OnMIDIInputConnected;
        this.midiMgr.onMIDIInputDisconnected += this.OnMIDIInputDisconnected;
        this.midiMgr.onMIDIPollInputUpdated += this.OnMIDIInputPollingChange;

        this.StartCoroutine(
            PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(
                this.entriesScroll.ScrollRect, 1.0f));
    }

    ConnectionButton AddInputButton(MIDIInput midiInput)
    { 
        ConnectionButton ret = new ConnectionButton();
        ret.payload = midiInput;

        PxPre.UIL.UILStack uiStack = 
            new PxPre.UIL.UILStack(
                this.app.uiFactory,
                this.entriesScroll,
                this.entriesSizer);
               
        PxPre.UIL.EleGenButton<PushdownButton> button = uiStack.PushButton<PushdownButton>("", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        ret.button = button;
        button.border = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);
        uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.AddImage(this.midiTab.iconInput, 0.0f, 0);
            uiStack.AddHorizSpace(10.0f, 0.0f, 0);
            ret.infoSizer = uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                ret.title = uiStack.AddText(midiInput.DeviceName(), buttonTitleFontSize, false, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                ret.title.text.fontStyle = FontStyle.Bold;
                uiStack.AddSpace(10.0f, 0.0f, 0);
                ret.descr = uiStack.AddText(midiInput.Description(), buttonInfoFontSize, true, 1.0f, PxPre.UIL.LFlag.Grow|PxPre.UIL.LFlag.AlignVertCenter);
                ret.instr = uiStack.AddText("<i>Select to connect to this device for MIDI input.</i>", tipFontSize, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

        button.Button.onClick.AddListener(
            ()=>
            { 
                this.midiMgr.SetCurrentInput(midiInput); 
            });

        this.inputButtons.Add(ret);

        this.SetLayoutDirty();
        return ret;
    }

    void RemakeInputButtons()
    { 
        bool layout = false;
        if(this.inputButtons.Count > 0)
        {
            this.ClearInputButtons();
        }

        if(layout == true)
            this.SetLayoutDirty();
    }

    void ClearInputButtons()
    {
        this.lastSelectedInput = null;

        foreach(ConnectionButton cb in this.inputButtons)
        {
            this.entriesSizer.Remove(cb.button);
            GameObject.Destroy(cb.button.Button.gameObject);
        }

        this.inputButtons.Clear();

        this.SetLayoutDirty();
    }

    void DeleteInputButton(MIDIInput midiInput)
    { 
        for(int i = 0; i < this.inputButtons.Count; ++i)
        {
            if(this.inputButtons[i].payload.Equivalent(midiInput) == false)
                continue;

            this.DeleteInputButton(i);
            break;
        }
    }

    void DeleteInputButton(int idx)
    { 
        ConnectionButton cb = this.inputButtons[idx];
        this.entriesSizer.Remove(cb.button);
        GameObject.Destroy(cb.button.Button.gameObject);
        this.inputButtons.RemoveAt(idx);
        this.SetLayoutDirty();
    }

    void UpdateDisconnectButton(bool isConnected)
    { 
        if(isConnected == true)
        { 
            this.disconnectButton.button.Button.interactable = true;
            this.disconnectButton.title.text.text = "Disconnect";
            this.disconnectButton.descr.text.text = "<i>Select to disconnect the input MIDI.</i>";
            this.powerIcon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            this.disconnectButton.button.Button.interactable = false;
            this.disconnectButton.title.text.text = "Disconnected";
            this.disconnectButton.descr.text.text = "The device is currently not connected to any MIDI input.";
            this.powerIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        }
    }

    private void OnButton_Disconnect()
    {
        this.midiMgr.SetCurrentInput(null);
    }

    private void OnButton_Refresh()
    { 
        this.midiMgr.RefreshKnownInputs();
    }

    void OnMIDIRefreshed(MIDIMgr mgr, List<MIDIInput> newInputs)
    { 
        this.ClearInputButtons();

        MIDIInput currentIn = this.midiMgr.GetCurrentInput();
        foreach(MIDIInput mi in newInputs)
        { 
            ConnectionButton btn = this.AddInputButton(mi);

            if(currentIn != null)
            { 
                if(mi.Equivalent(currentIn) == true)
                    btn.SetSelected(true, this);
            }
        }
        this.lastSelectedInput = currentIn;

        this.UpdateDisconnectButton( currentIn != null);
        this.SetLayoutDirty();
    }

    void OnMIDIInputConnected(MIDIMgr mgr, MIDIInput newInput)
    { 
        this.UpdateDisconnectButton(true);

        if(this.lastSelectedInput != null)
        { 
            foreach(ConnectionButton cb in this.inputButtons)
            { 
                if(this.lastSelectedInput.Equivalent(cb.payload) == true)
                { 
                    cb.SetSelected(false, this);
                    break;
                }
            }
        }

        if(newInput != null)
        {
            foreach(ConnectionButton cb in this.inputButtons)
            { 
                if(newInput.Equivalent(cb.payload) == true)
                { 
                    cb.SetSelected(true, this);
                    break;
                }
            }
        }

        this.lastSelectedInput = newInput;
    }

    void OnMIDIInputDisconnected(MIDIMgr mgr)
    { 
        this.UpdateDisconnectButton(false);

        if(this.lastSelectedInput != null)
        { 
            foreach(ConnectionButton cb in this.inputButtons)
            { 
                if(this.lastSelectedInput.Equivalent(cb.payload) == false)
                    continue;

                cb.SetSelected(false, this);
                break;
            }   
        }
        this.lastSelectedInput = null;
    }

    void OnMIDIInputPollingChange(MIDIMgr mgr, List<MIDIInput> deleted, List<MIDIInput> added)
    {
        foreach(MIDIInput miDel in deleted)
        {
            for(int i = 0; i < this.inputButtons.Count; ++i)
            { 
                if(miDel.Equivalent(this.inputButtons[i].payload) == true)
                { 
                    this.DeleteInputButton(i);
                    break;
                }
            }
        }

        foreach(MIDIInput mi in added)
            this.AddInputButton(mi);
    }

    void SetLayoutDirty()
    { 
        if(this.dirtyCoroutine == null)
            this.dirtyCoroutine = this.StartCoroutine(this.DirtyLayout());
    }

    IEnumerator DirtyLayout()
    { 
        yield return new WaitForFixedUpdate();

        this.dlg.host.LayoutInRTSmartFit();
        this.dirtyCoroutine = null;
    }

    Color IConnectionButtonBridge.SelectedButtonColor 
    { get { return GetSelectedColor(); } }

    Color IConnectionButtonBridge.UnselectedButtonColor
    { get { return GetUnselectedColor(); } }

    void IConnectionButtonBridge.SetLayoutDirty()
    { 
        this.SetLayoutDirty();
    }
}
