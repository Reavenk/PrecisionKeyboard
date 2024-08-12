using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogControllerMIDI : 
    MonoBehaviour,
    IConnectionButtonBridge
{
    public class ConnectionButton : ConnectionButtonBase<MIDIOutput>
    { }

    public Application app;
    public MIDIMgr midiMgr;
    public PxPre.UIL.Dialog dlg;

    PushdownButton refreshButton;

    ConnectionButton disconnectButton = new ConnectionButton();
    //
    List<ConnectionButton> outputButtons = 
        new List<ConnectionButton>();

    // The last knowledge we have of what's connected. It's used to track
    // what's highlighted green so we know what to "de-green-ify" when
    // a disconnection happens.
    MIDIOutput lastSelectedOutput = null;

    PxPre.UIL.EleBoxSizer entriesSizer = null;
    PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> entriesScroll = null;

    UnityEngine.UI.Image powerIcon = null;


    const int buttonTitleFontSize = DialogInputMIDI.buttonTitleFontSize;
    const int buttonInfoFontSize = DialogInputMIDI.buttonInfoFontSize;

    Coroutine dirtyCoroutine = null;

    public readonly Color selectedButtonColor = DialogInputMIDI.GetSelectedColor();

    // We don't actually do anything functional with it, but we use the reference
    // to pull certain icon assets from it. We may want to encapsulate it better
    // to ensure that we limit the variable to that.
    public KeyDispMIDI midiTab;

    public const int tipFontSize = DialogInputMIDI.tipFontSize;

    public MIDIOutputPollScope pollScope = null;

    public static DialogControllerMIDI Create(Application app, MIDIMgr midiMgr, RectTransform invoking, KeyDispMIDI midiTab)
    { 
        PxPre.UIL.Dialog dlg = 
            app.dlgSpawner.CreateDialogTemplate( 
                new Vector2(650.0f, 0.0f), 
                PxPre.UIL.LFlag.GrowVertOnCollapse|PxPre.UIL.LFlag.AlignCenter, 0.0f);

        DialogControllerMIDI dcmidi = 
            dlg.host.RT.gameObject.AddComponent<DialogControllerMIDI>();

        dcmidi.app          = app;
        dcmidi.midiMgr      = midiMgr;
        dcmidi.dlg          = dlg;
        dcmidi.midiTab      = midiTab;
        dcmidi.pollScope    = new MIDIOutputPollScope(midiMgr);

        dcmidi.InitDialog();

        dlg.host.LayoutInRTSmartFit();
        app.SetupDialogForTransition(dlg, invoking, true, true);

        return dcmidi;
    }

    private void OnDestroy()
    {
        this.midiMgr.onMIDIRefreshedOutputs -= this.OnMIDIRefreshed;
        this.midiMgr.onMIDIControllerConnected -= this.OnMIDIControllerConnected;
        this.midiMgr.onMIDIControllerDisconnected -= this.OnMIDIControllerDisconnected;
        this.midiMgr.onMIDIPollOutputUpdated -= this.OnMIDIOutputPollingChange;

        if(this.pollScope != null)
            this.pollScope = null;
    }

    public void InitDialog()
    {
        this.dlg.AddDialogTemplateTitle("MIDI Output");

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
            "<i>A list of identified MIDI ports that can be used as inputs to other applications.</i>",
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

        UpdateDisconnectButton(this.midiMgr.GetCurrentOutput() != null);            

        foreach(MIDIOutput mo in this.midiMgr.GetKnownOutputs())
            this.AddOutputButton(mo);

        MIDIOutput cur = this.midiMgr.GetCurrentOutput();
        if(cur != null)
        {
            foreach(ConnectionButton cb in this.outputButtons)
            { 
                if(cb.payload.Equivalent(cur) == true)
                { 
                    cb.SetSelected(true, this);
                    break;
                }
            }
        }
        this.lastSelectedOutput = cur;

        this.midiMgr.onMIDIRefreshedOutputs         += this.OnMIDIRefreshed;
        this.midiMgr.onMIDIControllerConnected      += this.OnMIDIControllerConnected;
        this.midiMgr.onMIDIControllerDisconnected   += this.OnMIDIControllerDisconnected;
        this.midiMgr.onMIDIPollOutputUpdated        += this.OnMIDIOutputPollingChange;

        this.StartCoroutine(
            PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(
                this.entriesScroll.ScrollRect, 1.0f));
    }

    ConnectionButton AddOutputButton(MIDIOutput midiOutput)
    { 
        ConnectionButton ret = new ConnectionButton();
        ret.payload = midiOutput;

        PxPre.UIL.UILStack uiStack = 
            new PxPre.UIL.UILStack(
                this.app.uiFactory,
                this.entriesScroll,
                this.entriesSizer);
               
        PxPre.UIL.EleGenButton<PushdownButton> button = uiStack.PushButton<PushdownButton>("", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        ret.button = button;
        button.border = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);
        uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow);
            uiStack.AddImage(this.midiTab.iconOutput, 0.0f, 0);
            uiStack.AddHorizSpace(10.0f, 0.0f, 0);
            ret.infoSizer = uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                ret.title = uiStack.AddText(midiOutput.DeviceName(), buttonTitleFontSize, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                ret.title.text.fontStyle = FontStyle.Bold;
                uiStack.AddSpace(10.0f, 0.0f, 0);
                ret.descr = uiStack.AddText(midiOutput.Product(), buttonInfoFontSize, true, 1.0f, PxPre.UIL.LFlag.Grow|PxPre.UIL.LFlag.AlignVertCenter);
                ret.instr = uiStack.AddText("<i>Select to connect send MIDI keyboard messages to the target device.</i>", tipFontSize, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

        button.Button.onClick.AddListener(
            ()=>
            { 
                this.midiMgr.SetCurrentOutput(midiOutput); 
            });

        this.outputButtons.Add(ret);

        this.SetLayoutDirty();
        return ret;
    }

    void RemakeOutputButtons()
    { 
        bool layout = false;
        if(this.outputButtons.Count > 0)
        {
            this.ClearOutputButtons();
        }

        if(layout == true)
            this.SetLayoutDirty();
    }

    void ClearOutputButtons()
    {
        this.lastSelectedOutput = null;

        foreach(ConnectionButton cb in this.outputButtons)
        {
            this.entriesSizer.Remove(cb.button);
            GameObject.Destroy(cb.button.Button.gameObject);
        }

        this.outputButtons.Clear();

        this.SetLayoutDirty();
    }

    void DeleteOutputButton(MIDIOutput midiOutput)
    { 
        for(int i = 0; i < this.outputButtons.Count; ++i)
        {
            if(this.outputButtons[i].payload.Equivalent(midiOutput) == false)
                continue;

            this.DeleteOutputButton(i);
            break;
        }
    }

    void DeleteOutputButton(int idx)
    { 
        ConnectionButton cb = this.outputButtons[idx];
        this.entriesSizer.Remove(cb.button);
        GameObject.Destroy(cb.button.Button.gameObject);
        this.outputButtons.RemoveAt(idx);
        this.SetLayoutDirty();
    }

    void UpdateDisconnectButton(bool isConnected)
    { 
        if(isConnected == true)
        { 
            this.disconnectButton.button.Button.interactable = true;
            this.disconnectButton.title.text.text = "Disconnect";
            this.disconnectButton.descr.text.text = "<i>Select to disconnect the controller MIDI.</i>";
            this.powerIcon.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            this.disconnectButton.button.Button.interactable = false;
            this.disconnectButton.title.text.text = "Disconnected";
            this.disconnectButton.descr.text.text = "The device is currently not connected to any MIDI destination.";
            this.powerIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        }
    }

    private void OnButton_Disconnect()
    {
        this.midiMgr.SetCurrentOutput(null);
    }

    private void OnButton_Refresh()
    {
        this.midiMgr.RefreshKnownOutputs();
    }

    void OnMIDIRefreshed(MIDIMgr mgr, List<MIDIOutput> newInputs)
    { 
        this.ClearOutputButtons();

        MIDIOutput currentOut = this.midiMgr.GetCurrentOutput();
        foreach(MIDIOutput mo in newInputs)
        { 
            ConnectionButton btn = this.AddOutputButton(mo);

            if(currentOut != null)
            { 
                if(mo.Equivalent(currentOut) == true)
                    btn.SetSelected(true, this);
            }
        }
        this.lastSelectedOutput = currentOut;

        this.UpdateDisconnectButton(currentOut != null);
        this.SetLayoutDirty();
    }

    void OnMIDIControllerConnected(MIDIMgr mgr, MIDIOutput newOutput)
    { 
        this.UpdateDisconnectButton(true);

        if(this.lastSelectedOutput != null)
        { 
            foreach(ConnectionButton cb in this.outputButtons)
            { 
                cb.SetSelected(false, this);
                break;
            }
        }

        if(newOutput != null)
        { 
            foreach(ConnectionButton cb in this.outputButtons)
            { 
                cb.SetSelected(true, this);
                break;
            }
        }
        
        Debug.Log("DELEME OnMIDIControllerConnected set new output");
        this.lastSelectedOutput = newOutput;
    }

    void OnMIDIControllerDisconnected(MIDIMgr mgr)
    { 
        this.UpdateDisconnectButton(false);

        Debug.Log("DELEME UpdateDisconnectButton right before null check");
        if(this.lastSelectedOutput != null)
        { 
            Debug.Log("DELEME UpdateDisconnectButton AFTER null check");
            foreach(ConnectionButton cb in this.outputButtons)
            { 
                if(this.lastSelectedOutput.Equivalent(cb.payload) == false)
                    continue;

                Debug.Log("DELEME Set selected false");
                cb.SetSelected(false, this);
                break;
            }   
            
        }
        Debug.Log("DELEME UpdateDisconnectButton set lastSelectedOutput to NULL");
        this.lastSelectedOutput = null;
    }

    void OnMIDIOutputPollingChange(MIDIMgr mgr, List<MIDIOutput> deleted, List<MIDIOutput> added)
    { 
        foreach(MIDIOutput moDel in deleted)
        {
            for(int i = 0; i < this.outputButtons.Count; ++i)
            { 
                if(moDel.Equivalent(this.outputButtons[i].payload) == true)
                { 
                    this.DeleteOutputButton(i);
                    break;
                }
            }
        }

        foreach(MIDIOutput mo in added)
            this.AddOutputButton(mo);
    }

    void SetLayoutDirty()
    { 
        if(this.dirtyCoroutine == null)
            this.dirtyCoroutine = this.StartCoroutine(this.DirtyLayout());
    }

    IEnumerator DirtyLayout()
    { 
        yield return new WaitForEndOfFrame();

        this.dlg.host.LayoutInRTSmartFit();
        this.dirtyCoroutine = null;
    }

    Color IConnectionButtonBridge.SelectedButtonColor
    { get { return DialogInputMIDI.GetSelectedColor(); } }

    Color IConnectionButtonBridge.UnselectedButtonColor
    { get { return DialogInputMIDI.GetUnselectedColor(); } }

    void IConnectionButtonBridge.SetLayoutDirty()
    { 
        this.SetLayoutDirty();
    }
}
