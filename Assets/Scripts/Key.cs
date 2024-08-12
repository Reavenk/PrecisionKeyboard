// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Phonics;

public class Key : 
    KeyDragGroup,
    UnityEngine.EventSystems.IPointerDownHandler,
    UnityEngine.EventSystems.IPointerUpHandler,
    UnityEngine.EventSystems.IPointerEnterHandler,
    UnityEngine.EventSystems.IPointerExitHandler,
    UnityEngine.EventSystems.IBeginDragHandler,
    UnityEngine.EventSystems.IDragHandler,
    UnityEngine.EventSystems.IEndDragHandler
{
    public struct TouchID
    {
        public UnityEngine.EventSystems.PointerEventData.InputButton ? button;
        public int touch;

        public TouchID(int touch)
        { 
            this.touch = touch;
            this.button = null;
        }

        public TouchID(UnityEngine.EventSystems.PointerEventData.InputButton button)
        { 
            this.touch = -1;
            this.button = button;
        }

        public TouchID(UnityEngine.EventSystems.PointerEventData data)
        { 
            if(data.pointerId == -1)
            {
                this.touch = -1;
                this.button = data.button;
            }
            else
            { 
                this.touch = data.pointerId;
                this.button = null;
            }
        }

    }
    static Dictionary<TouchID, int> multitouchHandles = new Dictionary<TouchID, int>();
    static Dictionary<int, TouchID> multitouchInvHandles = new Dictionary<int, TouchID>();
    static Dictionary<int, Key> keyToPress = new Dictionary<int, Key>();

    public Application app;
    public PaneKeyboard parent;

    public WesternFreqUtils.Key key;
    public int octave;

    private float freq;

    public int idDown = -1;
    public int cachedKeyID;

    public UnityEngine.UI.Image plate = null;
    public UnityEngine.UI.Text label = null;

    private int keyplayHandle = -1;

    public UnityEngine.UI.Image keyShadow;


    public RectTransform rectTransform 
    {
        get
        {
            return this.plate.rectTransform; 
        } 
    }

    public new void Awake()
    {
        base.Awake();

        // This isn't actually accurate since the Key isn't set up yet.
        //
        // These are reset later when this.key and this.octave are actually
        // hooked up properly.
        this.cachedKeyID = WesternFreqUtils.GetNote(this.key, this.octave);
        this.freq = WesternFreqUtils.GetFrequency(this.key, this.octave);
    }

    public static void EndAllNotes(Application a)
    {
        // This function has some encapsulation issues by talking in the application
        // as a parameter - but Application is a singleton anyways.
        foreach (KeyValuePair<TouchID, int> kvp in multitouchHandles)
            a.EndNote(kvp.Value);

        foreach(KeyValuePair<int, Key> kvp in keyToPress)
        { 
            if(kvp.Value == null || kvp.Value.gameObject == null)
                continue;

            kvp.Value.ResetTransition();
        }

        multitouchHandles.Clear();
        multitouchInvHandles.Clear();
        keyToPress.Clear();
    }

    public static void EndAllNotes()
    { 
        EndAllNotes(Application.Instance);
    }

    public void InitializeKey(PaneKeyboard parent, WesternFreqUtils.Key key, int octave, Sprite up, Sprite down, Sprite disabled, Color cup, Color cdown)
    { 
        this.app    = parent.App;
        this.parent = parent;
        this.key = key;
        this.octave = octave;
        this.freq           = WesternFreqUtils.GetFrequency(this.key, this.octave);
        this.cachedKeyID    = WesternFreqUtils.GetNote(this.key, this.octave);

        this.plate = this.gameObject.AddComponent<UnityEngine.UI.Image>();
        this.plate.color = cup;
        this.plate.sprite = up;
        this.plate.type = UnityEngine.UI.Image.Type.Sliced;
        this.targetGraphic = this.plate;

        this.navigation = new UnityEngine.UI.Navigation();

        UnityEngine.UI.SpriteState ss = new UnityEngine.UI.SpriteState();
        ss.disabledSprite = disabled;
        ss.highlightedSprite = null;
        ss.pressedSprite = down;
        this.spriteState = ss;
        this.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;

        UnityEngine.UI.ColorBlock cb = this.colors;
        cb.pressedColor = cdown;
        cb.disabledColor = new Color(cup.r, cup.g, cup.b, 0.5f);
        cb.normalColor = cup;
        cb.highlightedColor = cup;
        this.colors = cb;

        this.CreateLabel(parent.keyAssets, parent.keyDims);

        this.SetLabel(true, true, parent.GetAccidental());
    }

    public void ShowLabel(bool show = true)
    { 
        if(this.label == null)
            return;

        this.label.gameObject.SetActive(show);
    }

    public void HideLabel()
    { 
        this.ShowLabel(false);
    }

    public void DestroyLabel()
    { 
        if(this.label == null)
            return;

        GameObject.Destroy(this.label.gameObject);
        this.label = null;
    }

    public void CreateLabel(KeyAssets ka, KeyDimParams kdim)
    {
        GameObject goLabel = new GameObject($"Label_{this.key.ToString()}{this.octave.ToString()}");
        goLabel.transform.SetParent(this.transform);
        UnityEngine.UI.Text label = goLabel.AddComponent<UnityEngine.UI.Text>();

        label.font                  = ka.keyLabelFont;
        label.color                 = ka.keyLabelColor;
        label.fontSize              = ka.labelFontSize;
        label.horizontalOverflow    = HorizontalWrapMode.Overflow;
        label.verticalOverflow      = VerticalWrapMode.Overflow;
        label.alignment             = TextAnchor.MiddleCenter;
        RectTransform rtLabel       = label.rectTransform;
        rtLabel.localRotation       = Quaternion.identity;
        rtLabel.localScale          = Vector3.one;
        rtLabel.pivot               = new Vector2(0.5f, 0.0f);
        rtLabel.anchorMin           = new Vector2(0.5f, 0.0f);
        rtLabel.anchorMax           = new Vector2(0.5f, 0.0f);
        rtLabel.anchoredPosition    = new Vector2(0.05f, kdim.labelUpOffset);
        rtLabel.sizeDelta           = Vector2.zero;

        this.label = label;
    }

    void StartNote(TouchID tid, float velocity)
    {
        // Just in case
        this.EndNote();

        // Not quite the same as end-note. End-note will end the handle of the last
        // play handle it has, where the code below will end the note of anything
        // using the same input.
        int curPlay;
        if(multitouchHandles.TryGetValue(tid, out curPlay) == true)
            this.EndNote(curPlay);

        GenBase gb = this.app.wiringPane.GenerateForFrequency(this.freq);
        int noteHandle = this.app.StartNote(gb, this.cachedKeyID, velocity, Application.NoteStartEvent.Pressed);

        if(noteHandle == -1)
            return;

        this.keyplayHandle = noteHandle;
        
        multitouchInvHandles.Add(this.keyplayHandle, tid);
        multitouchHandles.Add(tid, this.keyplayHandle);
        keyToPress.Add(this.keyplayHandle, this);
    }

    public void SetLabel(bool showKey, bool showOctave, KeyCollection.Accidental acc)
    {
        string str = "";

        if(showKey == true)
            str = KeyCollection.GetKeyCreationsInfo(acc)[this.key].noteName;

        if(showOctave == true)
            str += this.octave.ToString();

        if(string.IsNullOrEmpty(str) == true)
        {
            this.label.gameObject.SetActive(false);
            return;
        }


        this.label.text = str;
        this.label.gameObject.SetActive(true);
    }

    void EndNote()
    {
        if(this.keyplayHandle == -1)
            return;

        this.EndNote(this.keyplayHandle);
        this.keyplayHandle = -1;

    }

    void EndNote(int id)
    {
        TouchID match;
        
        if(multitouchInvHandles.TryGetValue(id, out match) == false)
            return;

        this.app.EndNote(id);

        multitouchHandles.Remove(match);
        multitouchInvHandles.Remove(id);

        // We need to remember when looking into keyToPress that
        // that's what we currently have recorded as pressing - 
        // so we need to reset that.
        Key k;
        if(keyToPress.TryGetValue(id, out k) == true)
        {
            if(k != null && k.gameObject != null)
                k.ResetTransition();

            keyToPress.Remove(id);
        }
    }

    public void ResetTransition()
    { 
        if(this.enabled == false || this.interactable == false)
            this.DoStateTransition(SelectionState.Disabled, true);
        else
            this.DoStateTransition(SelectionState.Normal, true);
    }

    void EndNote(TouchID tid)
    { 
        int id;
        if(multitouchHandles.TryGetValue(tid, out id) == false)
            return;

        this.EndNote(id);
    }

    void UnityEngine.EventSystems.IPointerDownHandler.OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if(this.interactable == false)
            return;

        base.OnPointerDown(eventData);

        TouchID tid = new TouchID(eventData);
        this.StartNote(tid, this.app.defaultVelocity);
    }


    void UnityEngine.EventSystems.IPointerUpHandler.OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        base.OnPointerUp(eventData);

        if(this.interactable == false)
            return;

        this.ResetColor();
        this.EndNote();

        TouchID tid = new TouchID(eventData);
        this.EndNote(tid);

        if(this.enabled == false || this.interactable == false)
            this.DoStateTransition(SelectionState.Disabled, true);
        else
            this.DoStateTransition(SelectionState.Normal, true);
    }

    public void ResetColor()
    {
        this.targetGraphic.color = this.colors.normalColor;
    }

    void UnityEngine.EventSystems.IPointerEnterHandler.OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (this.interactable == false)
            return;

        // The KeyDragGroup will most often be another key, but can also be a backplate
        // where a touch can split between the cracks of keys.
        if(
            eventData.pointerDrag != null && 
            eventData.pointerDrag.GetComponent<KeyDragGroup>() != null)
        { 
            // If we're dragging, pass the torch of who gets the dragging messages
            eventData.pointerDrag = this.gameObject;
            // If we're pressing the mouse and dragging from a KeyDragGroup instead of
            // a Key, we'll need to also set the pointerPress.
            eventData.pointerPress = this.gameObject;

            TouchID tid = new TouchID(eventData);
            this.StartNote(tid, this.app.defaultVelocity);

            this.DoStateTransition(SelectionState.Pressed, true);
            this.targetGraphic.color = this.colors.pressedColor;
        }
    }

    void UnityEngine.EventSystems.IPointerExitHandler.OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        base.OnPointerExit(eventData);

        TouchID tid = new TouchID(eventData);
        EndNote(tid);

        this.InstantClearState();

        //if(this.enabled == false || this.interactable == false)
        //    this.DoStateTransition(SelectionState.Disabled, true);
        //else
        //    this.DoStateTransition(SelectionState.Normal, true);
    }

    void UnityEngine.EventSystems.IBeginDragHandler.OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {}

    void UnityEngine.EventSystems.IDragHandler.OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    { }

    void UnityEngine.EventSystems.IEndDragHandler.OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.EndNote();

        if(this.interactable == false)
            return;

        this.DoStateTransition(SelectionState.Normal, true);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        // This shouldn't be set to null, but may end up being null when first created
        // because a callback could call this before setup code and assign the targetGraphic.
        if(this.targetGraphic != null)
        {
            switch ( state)
            { 
                case SelectionState.Normal:
                    this.targetGraphic.color = this.colors.normalColor;
                    break;

                case SelectionState.Highlighted:
                    this.targetGraphic.color = this.colors.highlightedColor;
                    break;

                case SelectionState.Pressed:
                    this.targetGraphic.color = this.colors.pressedColor;
                    break;

                case SelectionState.Disabled:
                    this.targetGraphic.color = this.colors.disabledColor;

                    this.EndNote();
                    break;
            }
        }
    }

    protected override void OnDestroy()
    { 
        this.EndNote();
        base.OnDestroy();
    }
}
