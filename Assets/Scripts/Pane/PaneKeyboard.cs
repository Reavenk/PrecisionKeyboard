// <copyright file="PaneKeyboard.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The keyboard pane.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Phonics;

public class PaneKeyboard : PaneBase
{
    // 
    //      PLAYER PREFERENCE VARIABLES
    //////////////////////////////////////////////////
    const string prefKey_HighlightKeys      = "hkeys";      // PlayerPreferences key for the label keys settings
    const string prefKey_HighlightOcts      = "hocts";      // PlayerPreferences key for the label octave settings
    const string prefKey_HighlightWhites    = "hwhites";    // PlayerPreferences key for the white key settings
    const string prefKey_HighlightBlacks    = "hblacks";    // PlayerPreferences key for the black key settings

    const string prefKey_DoubleHeight       = "keydheight";
    const string prefKey_ScrollPlace        = "scrollplace";

    const string prefKey_KeyWidth           = "keywidths";
    const string prefKey_BlackRatio         = "blackratio";

    const string prefMetro_Volume           = "metrovol";

    const string prefKey_QualPrior          = "qualPrior";
    const string prefKey_Accidental         = "acci";

    const string prefKey_BackColor          = "backCol";    // ROYGBIVM Background setting

    //
    //      MIN AND MAX PIANO KEY VALUES
    //////////////////////////////////////////////////
    const float minWhiteKeyWidth = 50.0f;       // The minimum pixel width of white keys
    const float maxWhiteKeyWidth = 180.0f;      // The maximum pixel width of white keys
    const float minBlackKeyWidth = 40.0f;       // The minimum pixel width of black keys
    const float maxBlackKeyWidth = 150.0f;      // The maximum pixel width of black keys

    /// <summary>
    /// The height of the injected exercise region
    /// </summary>
    public const float exerciseRegionHeight = 150.0f;

    public enum HighlightKeyMode
    {
        Ignore,
        Settings,
        Highlight,
        Unhighlight
    }

    public enum ScrollbarPlace
    { 
        Top,
        Bottom
    }

    public enum ExerciseDisplayMode
    {
        KeyboardExercise,
        PlainExercise
    }

    public enum QualityPriority
    { 
        ExperimentalPlus,
        Experimental,
        Responsiveness,
        Balance,
        Quality
    }

    /// <summary>
    /// Variables related to a spinner widget that controls the range 
    /// of piano octaves being shown
    /// </summary>
    [System.Serializable]
    public struct OctaveSpinner
    { 
        public UnityEngine.UI.Button downButton;    // Lower octave button
        public UnityEngine.UI.Button upButton;      // Raise octave button
        public UnityEngine.UI.InputField input;     // Input edit field
    }

    public struct KeyboardSetting
    {
        public int min;
        public int max;

        public PxPre.Phonics.WesternFreqUtils.Key highlightRoot;
        public int [] highlightIntervals;

        public float scroll;
        public bool roygbivm;

        public KeyboardSetting(int min, int max, PxPre.Phonics.WesternFreqUtils.Key highlightRoot, int [] highlightIntervals, float scroll, bool roygbivm)
        {
            this.min = min;
            this.max = max;

            this.scroll = scroll;

            this.highlightRoot = highlightRoot;
            this.highlightIntervals = highlightIntervals;

            this.roygbivm = roygbivm;
        }
    }

    Dictionary<int, MIDIPrevGlyph> midiPrevGlyphs = new Dictionary<int, MIDIPrevGlyph>();

    /// <summary>
    /// Variable used to track if code is being run before-or-after InitPane().
    /// </summary>
    bool initialized = false;

    ExerciseDisplayMode ? exerciseDisplayMode = null;

    Stack<KeyboardSetting> keyboardSettingStack = 
        new Stack<KeyboardSetting>();

    public RectTransform exerciseRectLabel;
    public RectTransform exerciseRectPulldown;

    float exerciseVolume = 0.75f;
    public float GetExerciseVolume() => this.exerciseVolume;

    /// <summary>
    /// The keyboard scroll viewport
    /// </summary>
    public UnityEngine.UI.Image keyboardViewport;

    /// <summary>
    /// The keyboard scrollbar
    /// </summary>
    public MultitouchScrollbar pianoScroll;

    public BEVScrollRender bevScroll;

    /// <summary>
    /// The keyboard transform where keys are directly parented to
    /// </summary>
    public RectTransform pianoRoot;
    public UnityEngine.UI.RawImage pianoROYGBIVMColor;

    /// <summary>
    /// 
    /// </summary>
    KeyDispBase curKeyDisp = null;

    /// <summary>
    /// The collection of keys on the keyboard
    /// </summary>
    KeyCollection keyCollection = null;

    /// <summary>
    /// 
    /// </summary>
    public KeyAssets keyAssets;

    /// <summary>
    /// 
    /// </summary>
    public KeyDimParams keyDims;

    /// <summary>
    /// The main eStop button
    /// </summary>
    [UnityEngine.SerializeField]
    private UnityEngine.UI.Image eStop;

    public QuizProgressRecord quizRecordCorrect;
    public QuizProgressRecord quizRecordIncorrect;

    //
    //      KEY DISPLAY OPTION PANELS
    //////////////////////////////////////////////////

    /// <summary>
    /// A collection of all keyboard display options.
    /// </summary>
    List<KeyDispBase> lstKeyDisp = new List<KeyDispBase>();

    public KeyDispDefault       keyDispDefault;         // The default options panel
    public KeyDispSamples       keyDispData;            // The data samples options panel
    public KeyDispHighlights    keyDispHighlight;       // The key highlighting options panel
    public KeyDispDims          keyDispDims;            // The key display and dimensions options panel
    public KeyDispMIDI          keyDispMIDI;            // The key display for MIDI input and output

    /// <summary>
    /// The image to change for the display options.
    /// </summary>
    public UnityEngine.UI.Image selKeyView;

    /// <summary>
    /// The pulldown region to spawn the dropdown to change 
    /// the options display options
    /// </summary>
    public RectTransform selKeyViewContainer;
    
    const float StdScrollbarHeight = 50.0f;


    //
    //      PUSHDOWN TOGGLES FOR LABELING KEYS
    //////////////////////////////////////////////////
    [Header("PUSHDOWN TOGGLES FOR LABELING KEYS")]
    public PushdownToggle modeToggleKeys;       // Toggle button for labeling music keys on the piano keys
    public PushdownToggle modeToggleOctaves;    // Toggle button for labeling octaves on the piano keys
    public PushdownToggle modeToggleWhites;     // Toggle button for labeling the white piano keys
    public PushdownToggle modeToggleBlacks;     // Toggle button for labeling the black piano keys

    /// <summary>
    /// The pulldown for the highlight scale
    /// </summary>
    public PulldownInfo pulldownHighlightScale;

    /// <summary>
    /// The pulldown for the highlight scale's key
    /// </summary>
    public PulldownInfo pulldownHighlightRoot;

    public PulldownInfo pulldownActiveInstrument;

    public Sprite icoAcciSharps;
    public Sprite icoAcciFlats;
    public PulldownInfo pulldownAccidental;

    public QualityPriority qualityPriority = QualityPriority.Responsiveness;

    /// <summary>
    /// A record of the current root key for keyboard highlighting. 
    /// Cached so we can restore it back after an exercise.
    /// </summary>
    PxPre.Phonics.WesternFreqUtils.Key highlightKey = WesternFreqUtils.Key.C;

    /// <summary>
    /// A record of the current intervals of the scale for keyboard
    /// highlighting. Cached so we can restore it back after 
    /// an exercise.
    /// </summary>
    int [] highlightIntervals;

    //
    //      DEFAULT OPTIONS PANEL UI VARIABLES
    //////////////////////////////////////////////////

    public UnityEngine.UI.Slider masterSlider; // The master volume slider

    public UnityEngine.UI.Text bpmText;         // The text showing the BPM on the default screen
    public UnityEngine.UI.Image metronomeButton;// The button to bring up the BPM dialog
    public GameObject bpmDlg;                   // Prefab for the BPM dialog

    /// <summary>
    /// A list of BPM UI items that are disabled during exercises and re-enabled all other times.
    /// </summary>
    public List<GameObject> bpmUIItems = new List<GameObject>();

    //
    //      EXERCISE RELATED UI VARIABLES
    //////////////////////////////////////////////////
    public RectTransform optionsContainer;
    public UnityEngine.UI.Image controlPanelPlate;
    public RectTransform tutorialContainer;

    float startingOptionsHeight; // The cached height of the options window when the app is initialized
    float keyboardTopOffset;

    public RectTransform mainTutorialBar;

    [System.NonSerialized]
    public AudioSource timedAudioSlow;
    [System.NonSerialized]
    public AudioSource timedAudioMed;
    [System.NonSerialized]
    public AudioSource timedAudioFast;

    public AudioClip clipTimeout;
    public AudioClip clipCorrect;
    public AudioClip clipIncorrect;
    public AudioClip clipLeaveExercise;
    public AudioClip clipPerfectApplause;
    public AudioClip clipEndBell;

    AudioSource audioTimeout;
    AudioSource audioCorrect;
    AudioSource audioIncorrect;
    AudioSource audioLeaveExercise;
    AudioSource audioPerfectApplause;
    AudioSource audioEndBell;

    //
    //      KEY DIMENSIONS OPTIONS PANEL UI VARIABLES
    //////////////////////////////////////////////////

    public ScrollbarPlace scrollbarPlace = ScrollbarPlace.Bottom;

    float scrollbarHeightScale = 1.0f;

    /// </summary>
    public UnityEngine.UI.Slider sliderBlackHeight; // The slider to adjust the height of black keys
    public UnityEngine.UI.Slider sliderKeyWidth;    // The slider to adjust the width (horizontal scale) of all piano keys

    public OctaveSpinner botOctaveSpin;
    public OctaveSpinner topOctaveSpin;

    public int lowerOctave = 1;                     // The lower value octave range.
    public int upperOctave = 7;                     // The higher value octave range.

    public Sprite icoScrollTop;
    public Sprite icoScrollBot;
    public PulldownInfo pulldownPlace;


    public Sprite icoScrollHeightNorm;
    public Sprite icoScrollHeightDouble;
    public PulldownInfo pulldownScrollHeight;

    //
    //      DATA OPTIONS PANEL UI VARIABLES
    //////////////////////////////////////////////////
    public UnityEngine.UI.InputField inputSamplesPerSec;
    public UnityEngine.UI.InputField inputBufferSize;

    public Sprite icoQualExperimentalPlus;
    public Sprite icoQualExperimental;
    public Sprite icoQualSpeed;
    public Sprite icoQualBalance;
    public Sprite icoQualQuality;
    public PulldownInfo pulldownQuality;

    public Sprite midiNotifier;
    public Sprite midiNotifierSmall;

    public override Application.TabTypes TabType()
    {
        return Application.TabTypes.Keyboard;
    }

    public void SetKeyWidthPercent(float per, bool setSlider = true, bool force = false)
    {
        per = Mathf.Clamp01(per);

        if (this.sliderKeyWidth.value == per && force == false)
            return;

        if(setSlider == true)
            this.sliderKeyWidth.value = per;

        this.keyDims.whiteKeyWidth = Mathf.Lerp(minWhiteKeyWidth, maxWhiteKeyWidth, per);
        this.keyDims.blackKeyWidth = Mathf.Lerp(minBlackKeyWidth, maxBlackKeyWidth, per);

        // This shouldn't normally happen
        if (this.keyCollection == null)
            return;

        this.AlignKeys(true, true, true);

        PlayerPrefs.SetFloat(prefKey_KeyWidth, per);
    }

    public float AlignKeys(bool updateScrollDims, bool updateScroll, bool updateBEV)
    { 
        float ret = this.keyCollection.AlignKeys();
        this.keyCollection.keyboardWidth = ret;
        this.pianoRoot.sizeDelta = 
            new Vector2(ret, this.pianoRoot.sizeDelta.y);

        if (updateScrollDims == true)
            this.UpdateScrollDims();

        if(updateScroll == true)
            this.UpdateScroll();

        if(updateBEV == true)
        {
            if (this.keyCollection.octaveBackground == KeyCollection.OctaveHighlighting.ROYGBIVM)
            {
                float halfPx = 0.0f;
                if(ret != 0.0f)
                    halfPx = 0.5f / ret;

                float uS = (this.lowerOctave - 1.0f + halfPx) / 8.0f;
                float uE = (this.upperOctave + halfPx) / 8.0f;

                this.pianoROYGBIVMColor.enabled = true;
                this.pianoROYGBIVMColor.uvRect = new Rect(uS, 0.0f, uE - uS, 1.0f);
            }
            else
                this.pianoROYGBIVMColor.enabled = false;

            this.bevScroll.SetDirty(true, true);
        }

        return ret;
    }

    public Key GetKey(int idx)
    { 
        int octave;
        PxPre.Phonics.WesternFreqUtils.Key k;

        PxPre.Phonics.WesternFreqUtils.GetNoteInfo(idx, out k, out octave);
        KeyPair kp = new KeyPair(k, octave);

        return this.GetKey(kp);
    }

    public Key GetKey(PxPre.Phonics.WesternFreqUtils.Key k, int octave)
    { 
        return this.GetKey(new KeyPair(k, octave));
    }

    public Key GetKey(KeyPair kp)
    {
        Key k;
        this.keyCollection.keyLookup.TryGetValue(kp, out k);
        return k;
    }

    public float GetKeyWidthPercent()
    {
        return this.sliderKeyWidth.value;
    }

    public float GetKeyWidth()
    { 
        return Mathf.Lerp(this.sliderKeyWidth.value, maxWhiteKeyWidth, this.keyDims.whiteKeyWidth);
    }

    public override void InitPane(Application app)
    { 
        base.InitPane(app);

        bool highlightKeys = PlayerPrefs.GetInt(prefKey_HighlightKeys, 1) != 0;
        bool highlightOcts = PlayerPrefs.GetInt(prefKey_HighlightOcts, 1) != 0;
        bool highlightWhites = PlayerPrefs.GetInt(prefKey_HighlightWhites, 1) != 0;
        bool highlightBlacks = PlayerPrefs.GetInt(prefKey_HighlightBlacks, 1) != 0;
        //
        this.modeToggleKeys.Initialize();
        this.modeToggleKeys.isOn = highlightKeys;
        this.modeToggleKeys.SetSpriteDown(highlightKeys);
        //
        this.modeToggleOctaves.Initialize();
        this.modeToggleOctaves.isOn = highlightOcts;
        this.modeToggleOctaves.SetSpriteDown(highlightOcts);
        //
        this.modeToggleWhites.Initialize();
        this.modeToggleWhites.isOn = highlightWhites;
        this.modeToggleWhites.SetSpriteDown(highlightWhites);
        //
        this.modeToggleBlacks.Initialize();
        this.modeToggleBlacks.isOn = highlightBlacks;
        this.modeToggleBlacks.SetSpriteDown(highlightBlacks);

        this.startingOptionsHeight = optionsContainer.rect.height;
        this.keyboardTopOffset = this.startingOptionsHeight;
        this.tutorialContainer.gameObject.SetActive(false);

        float bkRatio = PlayerPrefs.GetFloat(prefKey_BlackRatio, this.keyDims.blackPaddPercent);
        this.keyDims.blackPaddPercent = bkRatio;
        this.sliderBlackHeight.value = bkRatio;

        // Get the current percentage of white key width - used as default
        float whiteKeyLam = Mathf.InverseLerp(minWhiteKeyWidth, maxWhiteKeyWidth, this.keyDims.whiteKeyWidth);
        // override default is there's already a saved value.
        whiteKeyLam = PlayerPrefs.GetFloat(prefKey_KeyWidth, whiteKeyLam);
        // Convert back into black and white key widths
        this.sliderKeyWidth.value = whiteKeyLam;
        this.keyDims.whiteKeyWidth = Mathf.Lerp(minWhiteKeyWidth, maxWhiteKeyWidth, whiteKeyLam);
        this.keyDims.blackKeyWidth = Mathf.Lerp(minBlackKeyWidth, maxBlackKeyWidth, whiteKeyLam);
        
        this.keyCollection = 
            new KeyCollection(
                this.keyAssets, 
                this.keyDims, 
                this.pianoRoot, 
                this.keyboardViewport.rectTransform,
                this.keyAssets.whiteKeyColors,
                this.keyAssets.blackKeyColors);

        // We just directly set the accidentals mode. Something else will refresh the labels - probably
        // first creating the keyboard. If we did it afterwards, we'd be redoing the labels multiple times
        // needlessly.
        const int nAccSharp = (int)KeyCollection.Accidental.Sharp;
        if (PlayerPrefs.GetInt(prefKey_Accidental, nAccSharp) == nAccSharp)
        {
            this.keyCollection.accidental = KeyCollection.Accidental.Sharp;
            this.pulldownAccidental.icon.sprite = this.icoAcciSharps;
        }
        else
        {
            this.keyCollection.accidental = KeyCollection.Accidental.Flat;
            this.pulldownAccidental.icon.sprite = this.icoAcciFlats;
        }

        const int nColored = (int)KeyCollection.OctaveHighlighting.Black;
        if (PlayerPrefs.GetInt(prefKey_BackColor, nColored) == nColored)
        {
            this.keyCollection.octaveBackground = KeyCollection.OctaveHighlighting.Black;
            this.keyDispHighlight.UpdateROYGBIV(KeyCollection.OctaveHighlighting.Black, false);
        }
        else
        {
            this.keyCollection.octaveBackground = KeyCollection.OctaveHighlighting.ROYGBIVM;
            this.keyDispHighlight.UpdateROYGBIV(KeyCollection.OctaveHighlighting.ROYGBIVM, false);
        }

        float keyContentWidth = this.keyCollection.CreateKeyboardRange(1, 7, this, false);
        this.keyCollection.keyboardWidth = keyContentWidth;
        this.pianoRoot.offsetMax = new Vector2(keyContentWidth, 0.0f);

        if(PlayerPrefs.GetInt(prefKey_DoubleHeight, 1)  != 0)
            this.SetScrollbarHeight(false, false);

        if(PlayerPrefs.GetInt(prefKey_ScrollPlace, 1) != 0)
            this.SetScrollbarPlace(ScrollbarPlace.Top, true, false);

        this.UpdateOctaveSpins();

        int sourceSamp = this.App.samplesPerSeconds;
        if(sourceSamp == 0)
            sourceSamp = this.App.samplesPerSeconds;

        this.inputBufferSize.text = sourceSamp.ToString();
        this.inputSamplesPerSec.text = this.App.samplesPerSeconds.ToString();

        this.pianoScroll.onValueChanged.AddListener( this.OnScroll_Update);
        this.pianoScroll.onScale.AddListener(this.OnScroll_Scale);

        UnityEngine.UI.Button estopButton = this.eStop.GetComponent<UnityEngine.UI.Button>();
        this.App.AddEStop(this.eStop, true);

        this.CenterToNoteRange(
            new KeyPair(WesternFreqUtils.Key.C, 4), 
            new KeyPair(WesternFreqUtils.Key.Gs, 4));

        this.pulldownHighlightScale.text.text = "None";
        //
        this.pulldownHighlightRoot.text.text = "C";
        this.pulldownHighlightRoot.button.interactable = false;

        // These items need to be added in the order we want them to appear on the pulldown.
        this.lstKeyDisp.Add(this.keyDispDefault);
        this.lstKeyDisp.Add(this.keyDispHighlight);
        this.lstKeyDisp.Add(this.keyDispDims);
        this.lstKeyDisp.Add(this.keyDispData);
        this.lstKeyDisp.Add(this.keyDispMIDI);
        this.ViewKeyDisplay(null, false);
        this.UpdateBPMInput();

        this.masterSlider.value = this.App.MasterVolume();

        this.UpdateKeyLabels();

        this.quizRecordCorrect.Deactivate();
        this.quizRecordIncorrect.Deactivate();

        ////////////////////////////////////////////////////////////////////////////////
        
        int qualPrior = PlayerPrefs.GetInt(prefKey_QualPrior, (int)QualityPriority.Responsiveness);
        switch(qualPrior)
        { 
            case (int)QualityPriority.ExperimentalPlus:
                this.SetQualityPriority(QualityPriority.ExperimentalPlus, true, false);
                break;

            case (int)QualityPriority.Experimental:
                this.SetQualityPriority(QualityPriority.Experimental, true, false);
                break;

            case (int)QualityPriority.Responsiveness:
                this.SetQualityPriority(QualityPriority.Responsiveness, true, false);
                break;

            case (int)QualityPriority.Balance:
                this.SetQualityPriority(QualityPriority.Balance, true, false);
                break;

            default:
            case (int)QualityPriority.Quality:
                this.SetQualityPriority( QualityPriority.Quality, true, false);
                break;
        }

        ////////////////////////////////////////////////////////////////////////////////

        this.timedAudioSlow = this.gameObject.AddComponent<AudioSource>();
        this.timedAudioSlow.playOnAwake = false;
        this.timedAudioSlow.loop = true;
        this.timedAudioMed = this.gameObject.AddComponent<AudioSource>();
        this.timedAudioMed.playOnAwake = false;
        this.timedAudioMed.loop = true;
        this.timedAudioFast = this.gameObject.AddComponent<AudioSource>();
        this.timedAudioFast.playOnAwake = false;
        this.timedAudioFast.loop = true;
        this.timedAudioSlow.clip    = null;
        this.timedAudioMed.clip     = null;
        this.timedAudioFast.clip    = null;

        GameObject goApp = this.App.gameObject;
        //
        this.audioCorrect = goApp.AddComponent<AudioSource>();
        this.audioCorrect.playOnAwake = false;
        this.audioCorrect.clip = this.clipCorrect;
        //
        this.audioIncorrect = goApp.AddComponent<AudioSource>();
        this.audioIncorrect.playOnAwake = false;
        this.audioIncorrect.clip = this.clipIncorrect;
        //
        this.audioLeaveExercise = goApp.AddComponent<AudioSource>();
        this.audioLeaveExercise.playOnAwake = false;
        this.audioLeaveExercise.clip = this.clipLeaveExercise;
        //
        this.audioTimeout = goApp.AddComponent<AudioSource>();
        this.audioTimeout.playOnAwake = false;
        this.audioTimeout.clip = this.clipTimeout;
        //
        this.audioPerfectApplause = goApp.AddComponent<AudioSource>();
        this.audioPerfectApplause.playOnAwake = false;
        this.audioPerfectApplause.clip = this.clipPerfectApplause;
        //
        this.audioEndBell = goApp.AddComponent<AudioSource>();
        this.audioEndBell.playOnAwake = false;
        this.audioEndBell.clip = this.clipEndBell;
        
        this.SetExerciseVolume(this.exerciseVolume);

        ////////////////////////////////////////////////////////////////////////////////
        
        // Align one last time. Ideally we need to find a way to defer alignment
        // for as long as possible.
        this.AlignKeys(true, true, true);

        ////////////////////////////////////////////////////////////////////////////////

        foreach (KeyDispBase kdb in this.lstKeyDisp)
        {
            kdb.Init(this.App, this);
        }

        ////////////////////////////////////////////////////////////////////////////////
        this.initialized = true;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.LeftArrow) == true)
            this.pianoScroll.value -= 0.2f;
        else if(Input.GetKeyDown(KeyCode.RightArrow) == true)
            this.pianoScroll.value += 0.2f;
#endif
    }

    public void SetExerciseVolume(float baseVolume)
    { 
        this.exerciseVolume = baseVolume;

        if(this.audioCorrect != null)
            this.audioCorrect.volume    = baseVolume * 0.6f;

        if(this.audioIncorrect != null)
            this.audioIncorrect.volume = baseVolume * 0.6f;

        if(this.audioLeaveExercise != null)
            this.audioLeaveExercise.volume = baseVolume * 0.8f;

        if(this.audioTimeout != null)
            this.audioTimeout.volume = baseVolume * 0.5f;

        if(this.audioEndBell != null)
            this.audioEndBell.volume = baseVolume * 0.25f;

        if(this.audioPerfectApplause != null)
            this.audioPerfectApplause.volume = baseVolume * 0.75f;
    }

    public bool EnsureQuizTimerTicksReady()
    {
        bool ret = false;

        if(this.timedAudioSlow.clip == null)
        {
            this.timedAudioSlow.clip = this.GeneratePressureTickAudio(true, false, false, false, false, false, false, false);
            ret = true;
        }

        if(this.timedAudioMed.clip == null)
        {
            this.timedAudioMed.clip = this.GeneratePressureTickAudio(true, false, true, false, true, false, true, false);
            ret = true;
        }

        if(this.timedAudioFast.clip == null)
        {
            this.timedAudioFast.clip = this.GeneratePressureTickAudio(true, true, true, true, true, true, true, true);
            ret = true;
        }
        return ret;
    }

    public void InvalidateQuickTimerTicks()
    { 
        // We may just want to only check one, because it one is null, 
        //they're probably all null.

        if(this.timedAudioSlow != null)
            this.timedAudioSlow.clip = null;

        if(this.timedAudioMed != null)
            this.timedAudioMed.clip = null;

        if(this.timedAudioFast != null)
            this.timedAudioFast.clip = null;
    }

    AudioClip GeneratePressureTickAudio(bool t1, bool t2, bool t3, bool t4, bool t5, bool t6, bool t7, bool t8)
    {
        Application.MetronomeInstr mi = this.App.metronomeInstruments[0];

        int sampleCt = mi.sampleCt;
        float [] rf = new float[sampleCt];

        mi.audioClip.GetData(rf, 0);

        int freqCt = mi.audioClip.frequency;
        float [] clipVal = new float[freqCt];
        for(int i = 0; i < freqCt; ++i)
            clipVal[i] = 0.0f;

        bool [] rb = new bool[]{t1, t2, t3, t4, t5, t6, t7, t8 };

        for(int i = 0; i < rb.Length; ++i)
        { 
            if(rb[i] == false)
                continue;

            int baseIdx = (int)((float)i/(float)(rb.Length) * freqCt);

            // We may need to snip it in case adding the audio
            // goes past the buffer.
            int copyAmt = sampleCt;
            copyAmt = Mathf.Min(baseIdx + copyAmt, freqCt) - baseIdx; 

            for(int j = 0; j < copyAmt; ++j)
                clipVal[baseIdx + j] = rf[j];
        }

        AudioClip ac = AudioClip.Create("AudioClip", freqCt, 1, freqCt, false);
        ac.SetData(clipVal, 0);
        return ac;
    }

    public KeyCollection.Accidental GetAccidental()
    { 
        return this.keyCollection.accidental;
    }

    public Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> GetKeyCreationsInfo()
    { 
        return this.keyCollection.GetKeyCreationsInfo();
    }

    public void ViewKeyDisplay(KeyDispBase display, bool transitionAnim)
    { 
        if(display == null )
            display = this.keyDispDefault;

        if(this.curKeyDisp == display)
            return;

        if (this.curKeyDisp != null)
            this.curKeyDisp.OnHide();

        if(display != this.keyDispDefault)
            this.App.StopMetronome();

        this.curKeyDisp = display;
        this.curKeyDisp.gameObject.SetActive(true);
        //
        Sprite dispIcon = display.GetTabIcon(App.RunningExercise());
        this.App.DoDropdownIconUpdate(this.selKeyView, dispIcon);
        //
        this.curKeyDisp.OnShow(this.App.RunningExercise());

        foreach (KeyDispBase kdb in this.lstKeyDisp)
        { 
            if(kdb == display)
                continue;

            kdb.gameObject.SetActive(false);
        }

        //if(transitionAnim == true)
        //{ 
        //    GameObject goTrans = new GameObject("KeyDispTrans");
        //    goTrans.transform.SetParent(this.controlPanelPlate.transform.parent, false);
        //    UnityEngine.UI.Image imgTrans = goTrans.AddComponent<UnityEngine.UI.Image>();
        //    imgTrans.sprite = this.controlPanelPlate.sprite;
        //    imgTrans.type = Type.Sliced;
        //    imgTrans.rectTransform.pivot            = new Vector2(0.0f, 0.0f);
        //    imgTrans.rectTransform.anchorMin        = this.controlPanelPlate.rectTransform.anchorMin;
        //    imgTrans.rectTransform.anchorMax        = new Vector2(0.0f, this.controlPanelPlate.rectTransform.anchorMax.y);
        //    imgTrans.rectTransform.offsetMin        = this.controlPanelPlate.rectTransform.offsetMin;
        //    imgTrans.rectTransform.offsetMax        = new Vector2(100.0f, this.controlPanelPlate.rectTransform.offsetMax.y);
        //
        //    CanvasGroup cg = goTrans.AddComponent<CanvasGroup>();
        //    cg.blocksRaycasts = false;
        //
        //    // If it's in front, there's some poping on the edges when it's removed.
        //    imgTrans.rectTransform.SetSiblingIndex(
        //        this.controlPanelPlate.transform.GetSiblingIndex());
        //
        //    this.App.uiFXTweener.RectTransformLerpOffsets(
        //        imgTrans.rectTransform, 
        //        null,
        //        this.controlPanelPlate.rectTransform.anchorMin,
        //        this.controlPanelPlate.rectTransform.anchorMax,
        //        this.controlPanelPlate.rectTransform.offsetMin,
        //        this.controlPanelPlate.rectTransform.offsetMax,
        //        0.5f, 
        //        false,
        //        true);;
        //}
    }

    public void UpdateBPMInput()
    {
        this.bpmText.text = 
            this.App.GetBPMString();
    }

    public void ClearKeyboard()
    { 
        this.keyCollection.DestroyAllAndClear();
    }

    public void SwitchKeyboardDisplayMode(ExerciseDisplayMode ? mode)
    {
        // cache this because some things may need to check what mode we're in
        // if we realize we're in exercise mode.
        this.exerciseDisplayMode = mode;

        this.tutorialContainer.gameObject.SetActive(true);

        const float mainBarHeight = 50.0f;

        foreach(GameObject go in this.bpmUIItems)
            go.SetActive(mode.HasValue == false);

        if (mode.HasValue == false)
        {
            this.tutorialContainer.gameObject.SetActive(false);
            this.pianoScroll.gameObject.SetActive(true);
            this.keyboardViewport.gameObject.SetActive(true);

            this.optionsContainer.offsetMax = new Vector2(0.0f, 0.0f);
            this.optionsContainer.offsetMin = new Vector2(0.0f, -this.startingOptionsHeight);

            this.SetKeyboardTopOffset(this.startingOptionsHeight);
            //this.tutorialContainer.sizeDelta = new Vector2(0.0f, exerciseRegionHeight);

            this.mainTutorialBar.gameObject.SetActive(false);
        }
        else
        {
            this.mainTutorialBar.gameObject.SetActive(true);
            this.tutorialContainer.gameObject.SetActive(true);

            this.mainTutorialBar.sizeDelta = new Vector2(0.0f, mainBarHeight);
            this.mainTutorialBar.anchoredPosition = new Vector2(0.0f, 0.0f);

            this.optionsContainer.offsetMax = new Vector2(0.0f, -mainBarHeight);
            this.optionsContainer.offsetMin = new Vector2(0.0f, -this.startingOptionsHeight - mainBarHeight);

            switch (mode.Value)
            { 
                case ExerciseDisplayMode.KeyboardExercise:
                    this.pianoScroll.gameObject.SetActive(true);
                    this.keyboardViewport.gameObject.SetActive(true);

                    this.tutorialContainer.anchorMin = new Vector2(0.0f, 1.0f);
                    this.tutorialContainer.anchorMax = new Vector2(1.0f, 1.0f);
                    this.tutorialContainer.offsetMax = new Vector2(0.0f, -this.startingOptionsHeight - mainBarHeight);
                    this.tutorialContainer.offsetMin = new Vector2(0.0f, -this.startingOptionsHeight - mainBarHeight - exerciseRegionHeight);

                    this.SetKeyboardTopOffset(this.startingOptionsHeight + exerciseRegionHeight + mainBarHeight);
                    break;

                case ExerciseDisplayMode.PlainExercise:
                    this.pianoScroll.gameObject.SetActive(false);
                    this.keyboardViewport.gameObject.SetActive(false);

                    this.tutorialContainer.anchorMin = new Vector2(0.0f, 0.0f);
                    this.tutorialContainer.anchorMax = new Vector2(1.0f, 1.0f);
                    this.tutorialContainer.offsetMax = new Vector2(0.0f, -this.startingOptionsHeight - mainBarHeight);
                    this.tutorialContainer.offsetMin = new Vector2(0.0f, 0.0f);
                    break;
            }
        }
    }

    public void CreateKeyboard(
        float whiteKeyWidth, 
        float blackKeyWidth, 
        float whiteKeySplit,
        float blackKeyPad,
        UnityEngine.UI.Scrollbar scroll,
        RectTransform keyboardviewport,
        RectTransform keyboardContent,
        KeyCollection collection)
    { 
        

        float halfBlackWidth = blackKeyWidth * 0.5f;
        float halfWhiteKeySplit = whiteKeySplit * 0.5f;

        int octave = 1;
        float fX = 0;
        const int numOctavesToCreate = 7;
        //for(int i = 0; i < numOctavesToCreate; ++i)
        //{ 
        //    // cache the starting position because 
        //    // we use it as a starting point for the 
        //    // black keys which need to be done differently.
        //    float startFX = fX;
        //
        //    // Do the white keys
        //    foreach(CreateNoteDescr cnd in KeyCollection.whiteKeys)
        //    {
        //        
        //
        //        collection.AddKey(keyBtn);
        //        fX += whiteKeyWidth + whiteKeySplit;
        //    }
        //
        //    foreach(CreateNoteDescr cnd in KeyCollection.blackKeys)
        //    { 
        //        
        //
        //        collection.AddKey(keyBtn);
        //    }
        //
        //    ++octave;
        //}

        collection.keyboardWidth = fX;
    }

    bool UpdateScrollDims()
    { 
        this.bevScroll.SetDirty(true, true);

        return UpdateScrollDims(
            this.keyCollection,
            this.keyboardViewport.rectTransform,
            this.pianoRoot,
            this.pianoScroll);
    }

    public static bool UpdateScrollDims(
        KeyCollection kc,
        RectTransform keyboardViewport,
        RectTransform keyboardContent,
        UnityEngine.UI.MyScrollbar scroll)
    {
        float orignVal = scroll.value;
        Rect rect = keyboardViewport.rect;
        scroll.size = rect.width / kc.keyboardWidth;
        scroll.value = orignVal;

        // If true, the scrollbar is active, else , we should keep
        // it centered.
        bool ret = true;

        // If we don't need the scrollbar, make sure the keyboard is centered.
        if(kc.keyboardWidth < rect.width)
        {
            keyboardContent.anchoredPosition = 
                new Vector2(
                    (rect.width - kc.keyboardWidth) * 0.5f,
                    keyboardContent.anchoredPosition.y);

            ret = false;
        }

        return ret;
    }

    void UpdateScroll()
    {
        UpdateScroll(
            this.keyCollection,
            this.keyboardViewport.rectTransform,
            this.pianoRoot,
            this.pianoScroll);

        this.bevScroll.UpdateThumbUVs();
    }

    public static void UpdateScroll(
        KeyCollection kc,
        RectTransform keyboardViewport,
        RectTransform keyboardContent,
        UnityEngine.UI.MyScrollbar scroll)
    {
        Rect rect = keyboardViewport.rect;
        float scrollSpace = kc.keyboardWidth - rect.width;
        float scrollVal = scroll.value;

        if(scrollSpace <= 0.0f)
        {
            // If the keyboard fits entirely within the viewport, center it
            keyboardContent.anchoredPosition =
                new Vector2(
                    (rect.width - kc.keyboardWidth) * 0.5f,
                    0.0f);
        }
        else
        {
            keyboardContent.anchoredPosition = 
                new Vector2(
                    -scrollSpace * scrollVal, 
                    0.0f);
        }
    }

    public void CenterToNoteRange(params KeyPair[] keysToFocus)
    { 
        CenterToNoteRange(
            this.keyCollection, 
            this.keyboardViewport.rectTransform,
            this.pianoRoot,
            this.pianoScroll,
            keysToFocus);
    }

    public static void CenterToNoteRange( 
        KeyCollection kc, 
        RectTransform keyboardViewport, 
        RectTransform keyboardContent, 
        UnityEngine.UI.MyScrollbar scroll,
        params KeyPair [] keysToFocus)
    { 
        if(keysToFocus.Length == 0)
            return;

        List<Key> keys = new List<Key>();
        foreach(KeyPair kp in keysToFocus)
        { 
            Key k;
            if(kc.TryGetValue(kp, out k) == true)
                keys.Add(k);
        }

        if(keys.Count == 0)
            return;

        float minx = keys[0].rectTransform.anchoredPosition.x;
        float maxx = keys[0].rectTransform.anchoredPosition.x + keys[0].rectTransform.sizeDelta.x;

        for(int i = 0; i < keys.Count; ++i)
        { 
            minx = Mathf.Min(minx, keys[i].rectTransform.anchoredPosition.x);
            maxx = Mathf.Max(maxx, keys[i].rectTransform.anchoredPosition.x + keys[i].rectTransform.sizeDelta.x);
        }

        float cen = (minx + maxx) * 0.5f;

        float vpWidth = keyboardViewport.rect.width;
        float scrollExcess = kc.keyboardWidth - vpWidth;

        if(scrollExcess < 0.0f)
            return;

        scroll.value = 
            (cen - vpWidth * 0.5f) / scrollExcess;
    }

    void OnScroll_Update(float f)
    {
        this.UpdateScroll();
    }

    void OnScroll_Scale(float f)
    { 
        this.SetKeyWidthPercent(f * this.GetKeyWidthPercent());
    }

    public void LightEstop()
    { 
        this.eStop.color = Color.red;
    }

    public void UnlightEstop()
    { 
        this.eStop.color = Color.white;
    }

    class ScaleSelection : MusicScale
    { 
        public string pulldownLabel;

        public ScaleSelection(string menuName, string pulldownLabel, params int [] offsets)
            : base(menuName, offsets)
        { 
            this.pulldownLabel = pulldownLabel;
        }

        public ScaleSelection(MusicScale ms, string pulldownLabel)
            : base(ms)
        { 
            this.pulldownLabel = pulldownLabel;
        }

        public ScaleSelection(MusicScale ms)
            : base(ms)
        { 
            this.pulldownLabel = ms.name;
        }
    }

    /// <summary>
    /// Button callback for when the scale pulldown is clicked.
    /// </summary>
    public void OnButton_Highlight()
    {
        ScaleSelection [] scaleSelections = 
            new ScaleSelection []
            {
                new ScaleSelection(MusicScale.ScaleNone     ),
                new ScaleSelection(MusicScale.ScaleChromatic),
                new ScaleSelection(MusicScale.ScaleMajor    ),
                new ScaleSelection(MusicScale.ScaleMajorPent,       "Major Pent"),
                new ScaleSelection(MusicScale.ScaleMinor    ),
                new ScaleSelection(MusicScale.ScaleMinorPent,       "Minor Pent"),
                new ScaleSelection(MusicScale.ScaleHungarianMinor,  "Hunganrian Min"),
                new ScaleSelection(MusicScale.ScaleArabic   ),
                new ScaleSelection(MusicScale.ScalePersian  )
            };

        PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();

        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;

        foreach (ScaleSelection ss in scaleSelections)
        {
            bool isSel = 
                this.pulldownHighlightScale.text.text == ss.pulldownLabel;

            Color c = 
                isSel ?
                    dropProp.selectedColor:
                    dropProp.unselectedColor;

            ScaleSelection ssCpy = ss;
            stack.AddAction(
                c, 
                ssCpy.name, 
                ()=>
                {
                    if(ssCpy.name != this.pulldownHighlightScale.text.text)
                        this.App.DoDropdownTextUpdate(this.pulldownHighlightScale.text);

                    this.SetScale(ssCpy.pulldownLabel, ssCpy.offsets); 
                });
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stack.Root,
            this.pulldownHighlightScale.rootPlate);

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Button callback for when the root key is clicked.
    /// </summary>
    public void OnButton_SubHighlight()
    {
        PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();

        Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
            this.keyCollection.GetKeyCreationsInfo();

        PxPre.DropMenu.Props dropProps = 
            PxPre.DropMenu.DropMenuSingleton.MenuInst.props;

        foreach (PxPre.Phonics.WesternFreqUtils.Key k in KeyCollection.keyboardKeyOrder)
        {
            PxPre.Phonics.WesternFreqUtils.Key kCpy = k;

            Color c =
                (this.highlightKey == kCpy) ?
                    dropProps.selectedColor:
                    dropProps.unselectedColor;

            stack.AddAction(
                c,
                keyCreationsInfo[kCpy].noteName,
                ()=>
                { 
                    if(this.highlightKey != k)
                        this.App.DoDropdownTextUpdate(this.pulldownHighlightRoot.text);

                    this.SetHighlightKey(kCpy); 
                });
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stack.Root,
            this.pulldownHighlightRoot.rootPlate);

        this.App.DoVibrateButton();
    }

    public void OnButton_QualityPriority()
    {
        PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();

        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;
        Color cSel = dropProp.selectedColor;
        Color cUns = dropProp.unselectedColor;

        AudioConfiguration ac = AudioSettings.GetConfiguration();

        stack.AddAction(
            this.icoQualExperimentalPlus,
            (ac.dspBufferSize == 32) ? cSel : cUns,
            "Experimental+",   
            ()=>
            { 
                this.SetQualityPriority(QualityPriority.ExperimentalPlus, false);
            });

        stack.AddAction(
            this.icoQualExperimental,
            (ac.dspBufferSize == 64) ? cSel : cUns,
            "Experimental",   
            ()=>
            { 
                this.SetQualityPriority(QualityPriority.Experimental, false);
            });

        stack.AddAction(
            this.icoQualSpeed,
            (ac.dspBufferSize == 128) ? cSel : cUns,
            "Responsiveness",   
            ()=>
            { 
                this.SetQualityPriority(QualityPriority.Responsiveness, false);
            });

        stack.AddAction(
            this.icoQualBalance,
            (ac.dspBufferSize == 512) ? cSel : cUns,
            "Balance",
            ()=>
            { 
                this.SetQualityPriority(QualityPriority.Balance, false);
            });

        stack.AddAction(
            this.icoQualQuality,
            (ac.dspBufferSize > 512) ? cSel : cUns,
            "Quality",
            ()=>
            { 
                this.SetQualityPriority(QualityPriority.Quality,false);
            });

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stack.Root,
            this.pulldownQuality.rootPlate);

        this.App.DoVibrateButton();
    }

    public void OnButton_Accidentals()
    {
        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;
        Color cSel = dropProp.selectedColor;
        Color cUns = dropProp.unselectedColor;

        PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();

        stack.AddAction(
            this.icoAcciSharps,
            (this.keyCollection.accidental == KeyCollection.Accidental.Sharp) ? cSel : cUns,
            "Accidentals as Sharps",
            () => { this.OnMenu_AccidentalSharp(); });

        stack.AddAction(
            this.icoAcciFlats,
            (this.keyCollection.accidental == KeyCollection.Accidental.Flat) ? cSel : cUns,
            "Accidentals as Flats",
            () => { this.OnMenu_AccidentalFlat(); });


        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stack.Root,
            this.pulldownAccidental.rootPlate);

        this.App.DoVibrateButton();
    }

    public void OnMenu_AccidentalSharp()
    {
        this.SetAccidentalsMode(KeyCollection.Accidental.Sharp, false);
    }

    public void OnMenu_AccidentalFlat()
    { 
        this.SetAccidentalsMode(KeyCollection.Accidental.Flat, false);
    }

    public void SetAccidentalsMode(KeyCollection.Accidental acc, bool force)
    { 
        if(acc == this.keyCollection.accidental && force == false)
            return;

        this.keyCollection.accidental = acc;
        this.App.NotifyAccidentalsChanged();
    }

    public override void OnChangedAccidentalMode(KeyCollection.Accidental acc)
    {
        Sprite pulldownSprite = null;
        if (acc == KeyCollection.Accidental.Sharp)
            pulldownSprite = this.icoAcciSharps;
        else
            pulldownSprite = this.icoAcciFlats;

        this.pulldownAccidental.SetInfo(this.App, pulldownSprite, true);

        if (this.modeToggleBlacks.isOn && this.modeToggleKeys.isOn)
            this.UpdateKeyLabels();

        PlayerPrefs.SetInt(prefKey_Accidental, (int)acc);

        this.UpdateHighlightKeyName();
    }

    public void SetOctaveBackground(KeyCollection.OctaveHighlighting oh, bool setPref = true)
    {
        if (this.keyCollection.octaveBackground == oh)
            return;

        this.keyCollection.octaveBackground = oh;

        this.keyDispHighlight.UpdateROYGBIV(oh, true);

        this.AlignKeys(false, false, true);
        this.RehighlightKeys(true);

        if(setPref == true)
            PlayerPrefs.SetInt(prefKey_BackColor, (int)oh);
    }

    public void UpdateKeyLabels()
    {
        this.keyCollection.SetKeyLabels(
            this.modeToggleKeys.isOn,
            this.modeToggleOctaves.isOn,
            this.modeToggleWhites.isOn,
            this.modeToggleBlacks.isOn);
    }

    public void SetKeyLabels( bool labelKeys, bool labelOctaves, bool labelWhites, bool labelBlacks)
    {
        this.keyCollection.SetKeyLabels(
            labelKeys,
            labelOctaves,
            labelWhites,
            labelBlacks);
    }

    public bool SetKeyLabel(int octave, PxPre.Phonics.WesternFreqUtils.Key key, bool labelKey, bool labelOctave)
    { 
        return this.keyCollection.SetKeyLabel(octave, key, labelKey, labelOctave);
    }

    public bool SetKeyLabel(KeyPair note, bool labelKey, bool labelOctave)
    { 
        return this.keyCollection.SetKeyLabel(note, labelKey, labelOctave);
    }

    public void OnToggle_ToggleModeKeys()
    {
        if (this.keyCollection == null)
            return;

        PlayerPrefs.SetInt(prefKey_HighlightKeys, this.modeToggleKeys.isOn ? 1 : 0);

        this.UpdateKeyLabels();

        this.App.DoVibrateButton();
    }

    public void OnToggle_ToggleModeOctaves()
    {
        if (this.keyCollection == null)
            return;

        PlayerPrefs.SetInt(prefKey_HighlightOcts, this.modeToggleOctaves.isOn ? 1 : 0);

        this.UpdateKeyLabels();

        this.App.DoVibrateButton();
    }

    public void OnToggle_ToggleModeWhites()
    {
        if (this.keyCollection == null)
            return;

        PlayerPrefs.SetInt(prefKey_HighlightWhites, this.modeToggleWhites.isOn ? 1 : 0);

        this.UpdateKeyLabels();

        this.App.DoVibrateButton();
    }

    public void OnToggle_ToggleModeBlacks()
    {
        if (this.keyCollection == null)
            return;

        PlayerPrefs.SetInt(prefKey_HighlightBlacks, this.modeToggleBlacks.isOn ? 1 : 0);

        this.UpdateKeyLabels();

        this.App.DoVibrateButton();
    }

    public void SetScale(string scaleName, params int [] offsets)
    { 
        this.highlightIntervals = offsets;

        if(offsets.Length == 0)
        {
            this.keyCollection.RehighlightKeysWithOffset(null);
            this.bevScroll.SetDirty(false, true);
            this.pulldownHighlightRoot.button.interactable = false;
        }
        else
        {

            this.keyCollection.RehighlightKeysWithOffset(offsets);
            this.bevScroll.SetDirty(false, true);
            this.pulldownHighlightRoot.button.interactable = true;
        }

        this.pulldownHighlightScale.SetInfo(this.App, scaleName, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnButton_KeyViewPulldown()
    { 
        PxPre.DropMenu.StackUtil menuStack = 
            new PxPre.DropMenu.StackUtil();

        PxPre.DropMenu.Props dropProp =
                PxPre.DropMenu.DropMenuSingleton.MenuInst.props;

        foreach (KeyDispBase kdb in this.lstKeyDisp)
        {
            KeyDispBase kdbCpy = kdb;

            if( this.App.RunningExercise() == true && 
                this.exerciseDisplayMode.HasValue == true && // Sanity check
                kdb.AvailableDuringExercise(this.exerciseDisplayMode.Value) == false)
            {
                continue;
            }

            bool runex = this.App.RunningExercise();

            Color c = 
                (kdb == this.curKeyDisp) ? 
                    dropProp.selectedColor : 
                    dropProp.unselectedColor;

            menuStack.AddAction(
                kdbCpy.GetTabIcon(runex), 
                c,
                kdbCpy.GetTabString(runex), 
                ()=>{ this.ViewKeyDisplay(kdbCpy, true); });
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            menuStack.Root,
            this.selKeyViewContainer);

        this.App.DoVibrateButton();
    }

    void SetHighlightKey(PxPre.Phonics.WesternFreqUtils.Key k)
    {
        this.highlightKey = k;

        this.keyCollection.HighlightKeys(k);
        this.bevScroll.SetDirty(false, true);

        this.UpdateHighlightKeyName();
    }

    void UpdateHighlightKeyName()
    {
        Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
            this.keyCollection.GetKeyCreationsInfo();

        this.pulldownHighlightRoot.SetInfo(
            this.App, 
            keyCreationsInfo[this.highlightKey].noteName, 
            true);
    }

    public void OnSlider_AdjustBlackKeyHeights()
    { 
        // This won't even happen except during initializtion.
        if(this.keyCollection == null)
            return;

        this.SetBlackKeyRatio(this.sliderBlackHeight.value);
    }

    public void SetBlackKeyRatio(float value)
    { 
        value = Mathf.Clamp01(value);
        this.keyDims.blackPaddPercent = value;
        this.keyCollection.AlignBlacks();

        PlayerPrefs.SetFloat(prefKey_BlackRatio, value);
    }

    public void OnSlider_AdjustKeyWidths()
    { 
        this.SetKeyWidthPercent(this.sliderKeyWidth.value, false, true);
    }

    public void OnButton_BPMTap()
    {
        // If the BPM needs changing, we'll get that as 
        // a signal we'll handle later.
        this.App.PushTapMeter(true);

        this.App.DoVibrateButton();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        // While this shouldn't normally be null, it will be on Start() because
        // this gets called before its initialization happens.
        if(this.keyCollection == null)
            return;

        bool scrollActive = this.UpdateScrollDims();
        this.keyCollection.AlignBlacks();

        if(scrollActive == true)
            this.UpdateScroll();
    }

    /// <summary>
    /// The button callback to set the sample rate to a high quality.
    /// </summary>
    public void OnButton_SamplesSecHigh()
    { 
        if(this.App.samplesPerSeconds != 44100)
            this.App.DoDropdownTextUpdate(this.inputSamplesPerSec.textComponent);

        this.App.samplesPerSeconds = 44100;
        this.inputSamplesPerSec.text = this.App.samplesPerSeconds.ToString();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// The button callback to set the sample rate to a medium quality.
    /// </summary>
    public void OnButton_SamplesSecMed()
    {
        if (this.App.samplesPerSeconds != 22050)
            this.App.DoDropdownTextUpdate(this.inputSamplesPerSec.textComponent);

        this.App.samplesPerSeconds = 22050;
        this.inputSamplesPerSec.text = this.App.samplesPerSeconds.ToString();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// The button callback to set the sample rate to a low quality.
    /// </summary>
    public void OnButton_SamplesSecLow()
    {
        if (this.App.samplesPerSeconds != 11025)
            this.App.DoDropdownTextUpdate(this.inputSamplesPerSec.textComponent);

        this.App.samplesPerSeconds = 11025;
        this.inputSamplesPerSec.text = this.App.samplesPerSeconds.ToString();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// The button callback to set the audio buffer to a high amount.
    /// </summary>
    public void OnButton_BufferSizeHigh()
    {
        this.App.sourceSamples = 100000;
        this.inputBufferSize.text = this.App.sourceSamples.ToString();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// The button callback to set the audio buffer to a medium amount.
    /// </summary>
    public void OnButton_BufferSizeMed()
    {
        this.App.sourceSamples = 44100;
        this.inputBufferSize.text = this.App.sourceSamples.ToString();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnButton_BufferSizeLow()
    {
        this.App.sourceSamples = 4096;
        this.inputBufferSize.text = this.App.sourceSamples.ToString();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnInput_BufferSizeEdit()
    { 
        int val;
        if(int.TryParse(this.inputBufferSize.text, out val) == false)
            return;

        this.App.sourceSamples = val;
    }

    public void OnButton_ScrollHeight()
    { 
        PxPre.DropMenu.StackUtil menuStack = new PxPre.DropMenu.StackUtil();

        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;
        Color cNorm = dropProp.unselectedColor;
        Color cDoub = dropProp.unselectedColor;

        if(this.scrollbarHeightScale == 2.0f)
            cDoub = dropProp.selectedColor;
        else
            cNorm = dropProp.selectedColor;

        menuStack.AddAction(
            this.icoScrollHeightNorm, 
            cNorm,
            "Normal BEV Height", 
            ()=>{ this.OnMenu_ScrollHeightNormal(); },
            0);

        menuStack.AddAction(
            this.icoScrollHeightDouble, 
            cDoub,
            "Double BEV Height", 
            ()=>{ this.OnMenu_ScrollHeightDouble(); },
            0);

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            menuStack.Root,
            this.pulldownScrollHeight.rootPlate);

        this.App.DoVibrateButton();
    }

    public void OnButton_ScrollTopBottom()
    {
        PxPre.DropMenu.StackUtil menuStack = new PxPre.DropMenu.StackUtil();

        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;
        Color cTop = dropProp.unselectedColor;
        Color cBot = dropProp.unselectedColor;

        if(this.scrollbarPlace == ScrollbarPlace.Top)
            cTop = dropProp.selectedColor;
        else
            cBot = dropProp.selectedColor;

        menuStack.AddAction(
            this.icoScrollTop,
            cTop,
            "BEV At Top", 
            () => { this.OnMenu_ScrollTop(); }, 
            0);

        menuStack.AddAction(
            this.icoScrollBot, 
            cBot,
            "BEV At Bottom", 
            () => { this.OnMenu_ScrollBot(); },
            0 );

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            menuStack.Root,
            this.pulldownPlace.rootPlate);

        this.App.DoVibrateButton();
    }

    public int [] HighlightedIntervals()
    { 
        return this.highlightIntervals;
    }

    public PxPre.Phonics.WesternFreqUtils.Key HighlightedRoot()
    { 
        return this.highlightKey;
    }

    public void OnMenu_ScrollHeightNormal()
    {
        if(this.scrollbarHeightScale == 1.0f)
            return;

        this.SetScrollbarHeight(true, true);
    }

    public void OnMenu_ScrollHeightDouble()
    {
        if(this.scrollbarHeightScale > 1.0f)
            return;

        this.SetScrollbarHeight(false, true);
    }

    public void OnMenu_ScrollTop()
    { 
        this.SetScrollbarPlace(ScrollbarPlace.Top, false, true);
    }

    public void OnMenu_ScrollBot()
    {
        this.SetScrollbarPlace(ScrollbarPlace.Bottom, false, true);
    }

    public void SetScrollbarHeight(bool normal, bool anim)
    { 
        Vector3 [] cornersStart = null;
        
        if(anim == true)
        {
            cornersStart = new Vector3[4];
            this.bevScroll.GetComponent<RectTransform>().GetWorldCorners(cornersStart);
        }

        if(normal == true)
        {
            this.SetScrollbarHeightScale(1.0f, this.icoScrollHeightNorm);
            PlayerPrefs.SetInt(prefKey_DoubleHeight, 0);
        }
        else
        {
            this.SetScrollbarHeightScale(2.0f, this.icoScrollHeightDouble);
            PlayerPrefs.SetInt(prefKey_DoubleHeight, 1);
        }

        if(anim == true)
        {
            Vector3[] cornersEnd = new Vector3[4];

            this.bevScroll.GetComponent<RectTransform>().GetWorldCorners(cornersEnd);
            cornersStart[0].x += BEVTransitionEffectPushInScr;
            cornersStart[1].x += BEVTransitionEffectPushInScr;
            cornersStart[2].x -= BEVTransitionEffectPushInScr;
            cornersStart[3].x -= BEVTransitionEffectPushInScr;
            cornersEnd[0].x += BEVTransitionEffectPushInDst;
            cornersEnd[1].x += BEVTransitionEffectPushInDst;
            cornersEnd[2].x -= BEVTransitionEffectPushInDst;
            cornersEnd[3].x -= BEVTransitionEffectPushInDst;


            this.App.StartCoroutine(
                this.App.TransitionReticule(
                    CanvasSingleton.canvas.GetComponent<RectTransform>(), 
                    cornersStart, 
                    cornersEnd,
                    null));
        }
    }

    float BEVTransitionEffectPushInScr = 60.0f;
    float BEVTransitionEffectPushInDst = 30.0f;
    public void SetScrollbarPlace(ScrollbarPlace sp, bool force, bool anim)
    { 
        if(this.scrollbarPlace == sp && force == false)
            return;

        this.scrollbarPlace = sp;

        Vector3[] cornersStart = null;

        if (anim == true)
        {
            cornersStart = new Vector3[4];
            this.bevScroll.GetComponent<RectTransform>().GetWorldCorners(cornersStart);
        }

        Sprite pullbarSprite = null;
        switch(this.scrollbarPlace)
        { 
            case ScrollbarPlace.Top:
                pullbarSprite = this.icoScrollTop;
                PlayerPrefs.SetInt(prefKey_ScrollPlace, 1);
                break;

            case ScrollbarPlace.Bottom:
                pullbarSprite = this.icoScrollBot;
                PlayerPrefs.SetInt(prefKey_ScrollPlace, 0);
                break;
        }

        this.pulldownPlace.SetInfo(this.App, pullbarSprite, true);
        this.AlignPianoAndScroll(true);

        if (anim == true)
        {
            Vector3[] cornersEnd = new Vector3[4];
            this.bevScroll.GetComponent<RectTransform>().GetWorldCorners(cornersEnd);

            cornersStart[0].x += BEVTransitionEffectPushInScr;
            cornersStart[1].x += BEVTransitionEffectPushInScr;
            cornersStart[2].x -= BEVTransitionEffectPushInScr;
            cornersStart[3].x -= BEVTransitionEffectPushInScr;
            cornersEnd[0].x += BEVTransitionEffectPushInDst;
            cornersEnd[1].x += BEVTransitionEffectPushInDst;
            cornersEnd[2].x -= BEVTransitionEffectPushInDst;
            cornersEnd[3].x -= BEVTransitionEffectPushInDst;


            this.App.StartCoroutine(
                this.App.TransitionReticule(
                    CanvasSingleton.canvas.GetComponent<RectTransform>(),
                    cornersStart,
                    cornersEnd,
                    null));
        }
    }

    public void SetScrollbarHeightScale(float newScale, Sprite sprite)
    { 
        if(this.scrollbarHeightScale == newScale)
            return;

        this.scrollbarHeightScale = newScale;
        this.pulldownScrollHeight.SetInfo(this.App, sprite, true);

        this.AlignPianoAndScroll();
    }

    public void SetKeyboardTopOffset(float offset)
    {
        this.keyboardTopOffset = offset;
        this.AlignPianoAndScroll(true);
    }

    public void AlignPianoAndScroll(bool alignKeys = true)
    {
        RectTransform rtScroll = this.pianoScroll.GetComponent<RectTransform>();
        RectTransform rtKB = this.keyboardViewport.rectTransform;
        if (this.scrollbarPlace == ScrollbarPlace.Top)
        {
            float splitPt = -this.keyboardTopOffset - StdScrollbarHeight * this.scrollbarHeightScale;

            rtScroll.pivot = new Vector2(0.0f, 1.0f);
            rtScroll.anchorMin = new Vector2(0.0f, 1.0f);
            rtScroll.anchorMax = new Vector2(1.0f, 1.0f);
            rtScroll.offsetMin = new Vector2(0.0f, splitPt);
            rtScroll.offsetMax = new Vector2(0.0f, -this.keyboardTopOffset);

            rtKB.pivot = new Vector2(0.0f, 0.0f);
            rtKB.anchorMin = Vector2.zero;
            rtKB.anchorMax = Vector2.one;
            rtKB.offsetMax = new Vector2(0.0f, splitPt);
            rtKB.offsetMin = new Vector2(0.0f, 0.0f);
        }
        else
        {
            rtKB.pivot = new Vector2(0.0f, 1.0f);
            rtKB.anchorMin = Vector2.zero;
            rtKB.anchorMax = Vector2.one;
            rtKB.offsetMax = new Vector2(0.0f, -this.keyboardTopOffset);
            rtKB.offsetMin = new Vector2(0.0f, StdScrollbarHeight * this.scrollbarHeightScale);

            rtScroll.pivot = Vector2.zero;
            rtScroll.anchorMin = new Vector2(0.0f, 0.0f);
            rtScroll.anchorMax = new Vector2(1.0f, 0.0f);
            rtScroll.offsetMin = new Vector2(0.0f, 0.0f);
            rtScroll.offsetMax = new Vector2(0.0f, StdScrollbarHeight * this.scrollbarHeightScale);

        }

        if(alignKeys == true)
            this.AlignKeys(false, false, false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnInput_BufferSizeEnd()
    {
        int val;
        if (int.TryParse(this.inputBufferSize.text, out val) == false)
            return;

        this.App.sourceSamples = val;
    }

    /// <summary>
    /// Input callback when the samples-per-second is edited.
    /// </summary>
    public void OnInput_SampleSecEdit()
    {
        int val;
        if (int.TryParse(this.inputSamplesPerSec.text, out val) == false)
            return;

        this.App.samplesPerSeconds = val;
    }

    /// <summary>
    /// Input callback when the samples-per-second is finished being edited.
    /// </summary>
    public void OnInput_SampleSecEnd()
    {
        int val;
        if (int.TryParse(this.inputSamplesPerSec.text, out val) == false)
            return;

        this.App.samplesPerSeconds = val;
    }

    /// <summary>
    /// Button callback to lower the lower octave range.
    /// </summary>
    public void OnButton_LowOctaveDown()
    {
        this.SetLowerOct(this.lowerOctave - 1);

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Button callback to raise the lower octave range.
    /// </summary>
    public void OnButton_LowOctaveUp()
    {
        this.SetLowerOct(this.lowerOctave + 1);

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Button callback to lower the upper octave range.
    /// </summary>
    public void OnButton_HighOctaveDown()
    {
        this.SetUpperOctave(this.upperOctave - 1);

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Button callback to raise the upper octave range.
    /// </summary>
    public void OnButton_HighOctaveUp()
    { 
        this.SetUpperOctave(this.upperOctave + 1);

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Input callback when the lower octave is edited.
    /// </summary>
    public void OnInput_LowerOctaveEdit()
    { 
        int o;
        if(int.TryParse(this.botOctaveSpin.input.text, out o) == false)
            return;

        this.SetLowerOct(o);
    }

    /// <summary>
    /// Input callback when the lower octave is finished being edited.
    /// </summary>
    public void OnInput_LowerOctaveEnd()
    {
        int o;
        if (int.TryParse(this.botOctaveSpin.input.text, out o) == true)
            this.SetLowerOct(o);

        this.UpdateOctaveSpins();

    }

    /// <summary>
    /// Input callback when the higher octave is edited.
    /// </summary>
    public void OnInput_UpperOctaveEdit()
    {
        int o;
        if (int.TryParse(this.topOctaveSpin.input.text, out o) == false)
            return;

        this.SetUpperOctave(o);
    }

    /// <summary>
    /// Input callback when the higher octave is finished being edited.
    /// </summary>
    public void OnInput_UpperOctaveEnd()
    {
        int o;
        if (int.TryParse(this.topOctaveSpin.input.text, out o) == true)
            this.SetUpperOctave(o);

        this.UpdateOctaveSpins();
    }

    /// <summary>
    /// Set the upper octave range that will be shown.
    /// </summary>
    /// <param name="newOct"></param>
    void SetUpperOctave(int newOct)
    {
        newOct = Mathf.Clamp(newOct, this.lowerOctave, KeyCollection.maxOctave);
        if (newOct == this.upperOctave)
            return;

        this.App.DoDropdownTextUpdate(
            this.topOctaveSpin.input.textComponent,
            newOct < this.upperOctave);

        this.upperOctave = newOct;
        this.bevScroll.SetDirty(true, true);

        this.EnforceKeyOctaveSettings();
    }

    /// <summary>
    /// Set the lower octave range taht will be shown.
    /// </summary>
    /// <param name="newOct"></param>
    void SetLowerOct(int newOct)
    {
        newOct = Mathf.Clamp(newOct, KeyCollection.minOctave, this.upperOctave);
        if (newOct == this.lowerOctave)
            return;

        this.App.DoDropdownTextUpdate(
            this.botOctaveSpin.input.textComponent, 
            newOct < this.lowerOctave);

        this.lowerOctave = newOct;
        this.bevScroll.SetDirty(true, true);

        this.EnforceKeyOctaveSettings();
    }

    public bool PushKeyboardSetting(int newLower, int newUpper, PxPre.Phonics.WesternFreqUtils.Key root, int [] intervals)
    { 
        if(newLower < KeyCollection.minOctave)
            newLower = KeyCollection.minOctave;

        if(newUpper > KeyCollection.maxOctave)
            newUpper = KeyCollection.maxOctave;

        if(newLower > newUpper)
            return false;

        this.PushKeyboardSetting();
        this.lowerOctave = newLower;
        this.upperOctave = newUpper;
        this.highlightKey = root;
        this.highlightIntervals = intervals;

        this.EnforceKeyOctaveSettings();

        if(intervals == null || intervals.Length == 0)
            this.RehighlightKeys(false);
        else
            this.keyCollection.HighlightKeysWithOffset(root, intervals);

        this.UpdateKeyLabels();
        this.bevScroll.SetDirty(true, true);
        return true;
    }

    /// <summary>
    /// Make the the octaves state and displays fit contraints.
    /// </summary>
    public void UpdateOctaveSpins()
    {
        botOctaveSpin.input.text = this.lowerOctave.ToString();
        topOctaveSpin.input.text = this.upperOctave.ToString();

        this.botOctaveSpin.downButton.interactable =
            this.lowerOctave > KeyCollection.minOctave;

        this.botOctaveSpin.upButton.interactable =
            this.lowerOctave < this.upperOctave &&
            this.lowerOctave < KeyCollection.maxOctave;

        this.topOctaveSpin.downButton.interactable = 
            this.upperOctave > this.lowerOctave &&
            this.upperOctave > KeyCollection.minOctave;

        this.topOctaveSpin.upButton.interactable = 
            this.upperOctave < KeyCollection.maxOctave;
    }

    /// <summary>
    /// Deconstruct the UI back to the default state after exercises are finished.
    /// </summary>
    /// <param name="exercise"></param>
    public void DestroyExercise(BaseExercise exercise)
    { 
        this.EnableAllKeys();

        this.SwitchKeyboardDisplayMode(null);

        this.quizRecordCorrect.plate.gameObject.SetActive(false);
        this.quizRecordIncorrect.plate.gameObject.SetActive(false);

        foreach(Transform t in this.mainTutorialBar)
            GameObject.Destroy(t.gameObject);

        foreach(Transform t in this.tutorialContainer)
            GameObject.Destroy(t.gameObject);

        this.timedAudioSlow.Stop();
        this.timedAudioMed.Stop();
        this.timedAudioFast.Stop();

    }

    /// <summary>
    /// Setup the UI to start an exercise.
    /// </summary>
    /// <param name="exercise">The exercise to start.</param>
    /// <returns>If the exercise starting process failed, false; else true.</returns>
    public bool SetupExercise(BaseExercise exercise, ExerciseDisplayMode displayMode)
    {
        this.SwitchKeyboardDisplayMode(displayMode);
        this.ViewKeyDisplay(this.keyDispDefault, true);

        exercise.SetupUI(this.tutorialContainer, this.mainTutorialBar, displayMode);

        return true;
    }

    /// <summary>
    /// Recreate the keyboard to have a certain number of octaves
    /// </summary>
    /// <param name="lowerOctave">The lower octave to include.</param>
    /// <param name="higherOctave">The higher octave to include.</param>
    public void CreateKeyboardRange(int lowerOctave, int higherOctave)
    { 
        this.keyCollection.keyboardWidth = 
            this.keyCollection.CreateKeyboardRange(lowerOctave, higherOctave, this, true);

        this.UpdateOctaveSpins();

        this.UpdateScrollDims();
        this.UpdateScroll();
    }

    /// <summary>
    /// Button callback to stop the exercise.
    /// </summary>
    public void OnButton_StopExercise()
    {
        this.App.dlgSpawner.CreateDialogTemplate(
            "Stop Exercise?", 
            "Are you sure you want to stop?", 
            new PxPre.UIL.DlgButtonPair("Cancel", null),
            new PxPre.UIL.DlgButtonPair(
                "Stop Exercise", 
                (x) =>
                {
                    this.App.StopExercise();
                    this.App.SetTabFromType(Application.TabTypes.Options);
                    this.PlayAudio_LeaveExercise();
                    return true;
                })).host.LayoutInRT(false);

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Button callback to toggle the metronome playing.
    /// </summary>
    public void OnButton_ToggleMetronome()
    { 
        this.App.ToggleMetronome();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Slider callback for when the master volume slider changes.
    /// </summary>
    public void OnSlider_MasterVolume()
    { 
        this.App.SetMasterVolume(this.masterSlider.value);
    }

    /// <summary>
    /// The function for emergency stopping - to stop all sounds.
    /// </summary>
    public override void OnEStop()
    { 
        //      LET OWNED KEYBOARD KNOW EVERYTHING IS KILLED
        //////////////////////////////////////////////////
        this.keyCollection.ResetKeyPresses();

        //      STOP ANY EXERCISE AUDIO (IF PLAYING)
        //////////////////////////////////////////////////
        this.audioCorrect.Stop();
        this.audioIncorrect.Stop();
        this.audioLeaveExercise.Stop();
        this.audioTimeout.Stop();

        //      BROADCAST TO KEY DISPLAY PANELS
        //////////////////////////////////////////////////
        foreach(KeyDispBase kdb in this.lstKeyDisp)
            kdb.OnEStop();

        //      CLEAR OUT ANY ANIMATED GLYPHS
        //////////////////////////////////////////////////
        //
        this.ClearMIDIGlyphs();
    }

    public void ClearMIDIGlyphs()
    { 
        // Get rid of the glyphs and make a copy of them first.
        // We make a copy because we don't want to iterate on 
        // the live version because that's would be modified
        // from signal handling.
        //
        // And if we're going to clear it out right after and
        // have a copy, might as well clear it out right after.
        List<MIDIPrevGlyph> glyphs = new List<MIDIPrevGlyph>(this.midiPrevGlyphs.Values);
        this.midiPrevGlyphs.Clear();
        //
        foreach(MIDIPrevGlyph mpg in glyphs)
        {
            if(mpg != null && mpg.gameObject != null)
                GameObject.Destroy(mpg.gameObject);
        }
    }

    /// <summary>
    /// Start the metronome audio.
    /// </summary>
    public override void OnMetronomeStart()
    { 
        this.App.SetMetronomeButtonImage(
            this.metronomeButton,
            true);
    }

    /// <summary>
    /// Stop the metronome audio.
    /// </summary>
    public override void OnMetronomeStop()
    { 
        this.App.SetMetronomeButtonImage(
            this.metronomeButton,
            false);
    }

    public void ReapplyQualityPriority()
    {
        this.SetQualityPriority(this.qualityPriority, true, false);
    }

    public void SetQualityPriority(QualityPriority qp, bool force, bool anim = true)
    { 
        AudioConfiguration ac = AudioSettings.GetConfiguration();

        // These bufferLen values were taken from probin what the different audio setting did
        // when running on PC. Hopefully these settings translate to other platforms.
        switch(qp)
        { 
            case QualityPriority.ExperimentalPlus:
                ac.dspBufferSize = 32;
                break;

            case QualityPriority.Experimental:
                ac.dspBufferSize = 64;
                break;

            case QualityPriority.Responsiveness:
                ac.dspBufferSize = 256;
                break;

            case QualityPriority.Balance:
                ac.dspBufferSize = 512;
                break;

            case QualityPriority.Quality:
                ac.dspBufferSize = 1024;
                break;
        }

        ac.speakerMode = AudioSpeakerMode.Mono;

        bool reset = AudioSettings.Reset(ac);
        if (reset == true)
            PlayerPrefs.SetInt(prefKey_QualPrior, (int)qp);

        this.qualityPriority = qp;

        Sprite newSprite = null;
        string newtext = string.Empty;

        if (reset == true || force == true)
        { 
            switch(qp)
            { 
                case QualityPriority.ExperimentalPlus:
                    newSprite = this.icoQualExperimentalPlus;
                    newtext = "Experimental+";
                    break;

                case QualityPriority.Experimental:
                    newSprite = this.icoQualExperimental;
                    newtext = "Experimental";
                    break;

                case QualityPriority.Responsiveness:
                    newSprite = this.icoQualSpeed;
                    newtext = "Responsive";
                    break;

                case QualityPriority.Balance:
                    newSprite = this.icoQualBalance;
                    newtext = "Balanced";
                    break;

                case QualityPriority.Quality:
                    newSprite = this.icoQualQuality;
                    newtext = "Quality";
                    break;
            }
        }

        if(anim == true)
        {
            this.pulldownQuality.SetInfo(
                this.App, 
                newSprite, 
                newtext, 
                force == false);
        }
        else
        {
            this.pulldownQuality.icon.sprite = newSprite;
            this.pulldownQuality.text.text = newtext;
        }

        // These procedurally generated clips are no longer valid.
        this.InvalidateQuickTimerTicks();
    }

    public override void OnExerciseStart()
    {
        this.selKeyView.sprite = this.curKeyDisp.GetTabIcon(true);

        this.exerciseRectLabel.offsetMax = 
            new Vector2(
                -140.0f, 
                this.exerciseRectLabel.offsetMax.y);

        this.exerciseRectPulldown.offsetMax = 
            new Vector2(
                -140.0f,
                this.exerciseRectLabel.offsetMax.y);
    }

    public void PushKeyboardSetting()
    { 
        this.keyboardSettingStack.Push(
            new KeyboardSetting(
                this.lowerOctave, 
                this.upperOctave,
                this.highlightKey,
                this.highlightIntervals,
                this.pianoScroll.value,
                this.keyCollection.octaveBackground == KeyCollection.OctaveHighlighting.ROYGBIVM));
    }


    public bool FlushKeyboardSettingStack(bool enforce)
    { 
        if(this.keyboardSettingStack.Count == 0)
            return false;

        while(this.keyboardSettingStack.Count > 1)
            this.keyboardSettingStack.Pop();

        
        KeyboardSetting op = this.keyboardSettingStack.Pop();
        this.lowerOctave = op.min;
        this.upperOctave = op.max;
        this.highlightKey = op.highlightRoot;
        this.highlightIntervals = op.highlightIntervals;

        bool curRoyGBivM = (this.keyCollection.octaveBackground == KeyCollection.OctaveHighlighting.ROYGBIVM);
        if(curRoyGBivM != op.roygbivm)
        { 
            if(op.roygbivm == true)
                this.keyCollection.octaveBackground = KeyCollection.OctaveHighlighting.ROYGBIVM;
            else
                this.keyCollection.octaveBackground = KeyCollection.OctaveHighlighting.Black;

            this.bevScroll.SetDirty(true, true);
        }

        if(enforce == true)
        {
            this.EnforceKeyOctaveSettings();
            this.keyCollection.HighlightKeysWithOffset(this.highlightKey, this.highlightIntervals);
            this.UpdateKeyLabels();

            this.pianoScroll.value = op.scroll;
        }

        return true;
    }

    /// <summary>
    /// Callback for when an exercise stops.
    /// </summary>
    public override void OnExerciseStop()
    {
        this.FlushKeyboardSettingStack(true);
        this.selKeyView.sprite = this.curKeyDisp.GetTabIcon(false);

        this.bevScroll.SetDirty(true, true);

        this.exerciseRectLabel.offsetMax = 
            new Vector2(
                -250.0f,
                this.exerciseRectLabel.offsetMax.y);

        this.exerciseRectPulldown.offsetMax =
            new Vector2(
                -250.0f,
                this.exerciseRectPulldown.offsetMax.y);
    }

    public void EnforceKeyOctaveSettings()
    { 
        this.EnforceKeyOctaveSettings(this.lowerOctave, this.upperOctave, HighlightKeyMode.Settings);
    }

    public void EnforceKeyOctaveSettings(int lower, int higher, HighlightKeyMode highlightKeys)
    {
        this.ClearMIDIGlyphs();

        // We don't align with CreateKeyboardRange because we're going to need to with out
        // own function anyways right after.
        this.keyCollection.keyboardWidth =
            this.keyCollection.CreateKeyboardRange(lower, higher, this, false);

        this.AlignKeys(true, true, true);

        this.UpdateOctaveSpins();

        switch(highlightKeys)
        { 
            case HighlightKeyMode.Ignore:
                break;

            case HighlightKeyMode.Settings:
            case HighlightKeyMode.Highlight:
                this.RehighlightKeys(true);
                break;

            case HighlightKeyMode.Unhighlight:
                this.RehighlightKeys(false);
                break;
        }
    }

    public void RehighlightKeys(bool highlight)
    { 
        if(highlight == true)
            this.keyCollection.RehighlightKeys();
        else
            this.keyCollection.UnhighlightKeys();

        this.bevScroll.SetDirty(false, true);
    }

    /// <summary>
    /// Callback for when the BPM changes.
    /// </summary>
    /// <param name="bpm">The new BPM</param>
    /// <param name="str">The preffered text form of the BPM</param>
    public override void OnBPMChange(float bpm, string str)
    { 
        this.bpmText.text = str;
    }

    /// <summary>
    /// Callback for the BPM region.
    /// </summary>
    public void OnButton_BPMDlg()
    { 
        this.OpenBPMDialog();

        this.App.DoVibrateButton();
    }

    /// <summary>
    /// Open the BPM dialog (when the BPM region is clicked).
    /// </summary>
    void OpenBPMDialog()
    {
        PxPre.UIL.Dialog dlg = 
            this.App.dlgSpawner.CreateDialogTemplate(
                new Vector2(-1.0f, -1.0f), 
                PxPre.UIL.LFlag.AlignCenter, 
                0.0f);

        // Insert title text
        dlg.AddDialogTemplateTitle("Metronome and BPM");

        GameObject goBPM = GameObject.Instantiate<GameObject>(this.bpmDlg, dlg.rootParent.RT);
        RectTransform rtBPM = goBPM.GetComponent<RectTransform>();
        Vector2 dlgSz = rtBPM.sizeDelta;
        rtBPM.pivot = new Vector2(0.0f, 1.0f);
        rtBPM.anchorMin = new Vector2(0.0f, 1.0f);
        rtBPM.anchorMax = new Vector2(0.0f, 1.0f);
        BPMPad bpm = goBPM.GetComponent<BPMPad>();
        bpm.Initialize(this.App);

        PxPre.UIL.EleRT ert = new PxPre.UIL.EleRT(rtBPM, dlgSz);
        dlg.bodySizer.Add(ert, 1.0f, PxPre.UIL.LFlag.Grow);

        // Add separator
        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(
            new PxPre.UIL.DlgButtonPair[]
            {
                new PxPre.UIL.DlgButtonPair("Cancel", (x)=>{bpm.RestoreOriginalBPM(); return true; }),
                new PxPre.UIL.DlgButtonPair("OK", null)
            });

        dlg.host.LayoutInRT(false);
        this.App.SetupDialogForTransition(dlg, this.bpmText.rectTransform);
    }

    public void PlayAudio_LeaveExercise()
    { 
        if(this.exerciseVolume == 0.0f)
            return;

        this.audioLeaveExercise.Stop();
        this.audioLeaveExercise.Play();
    }

    public void PlayAudio_Correct()
    { 
        if(this.exerciseVolume == 0.0f)
            return;

        this.audioCorrect.Stop();
        this.audioCorrect.Play();
    }

    public void PlayAudio_Incorrect()
    { 
        if(this.exerciseVolume == 0.0f)
            return;

        this.audioIncorrect.Stop();
        this.audioIncorrect.Play();
    }

    public void PlayAudio_Timeout()
    { 
        if(this.exerciseVolume == 0.0f)
            return;

        this.audioTimeout.Stop();
        this.audioTimeout.Play();
    }

    public void PlayAudio_EndBell()
    { 
        if(this.exerciseVolume == 0.0f)
            return;

        this.audioEndBell.Stop();
        this.audioEndBell.Play();
    }

    public void PlayAudio_PerfectApplause()
    { 
        if(this.exerciseVolume == 0.0f)
            return;

        this.audioPerfectApplause.Stop();
        this.audioPerfectApplause.Play();
    }

    public void EnableAllKeys()
    { 
        if(this.keyCollection == null)
            return;

        this.keyCollection.EnableAllKeys();
    }

    public void DisableAllKeys()
    { 
        if(this.keyCollection == null)
            return;

        this.keyCollection.DisableAllKeys();
    }

    public float GetKeyboardWidth()
    {
        return this.keyCollection.keyboardWidth;
    }

    public KeyCollection.OctaveHighlighting OctaveHighlighting 
    {get{return this.keyCollection.octaveBackground; } }

    public void OnButton_InstrumentPulldown()
    { 
        PxPre.DropMenu.StackUtil dropStack = new PxPre.DropMenu.StackUtil();
        foreach(WiringDocument wd in this.App.Wirings.Documents)
        { 
            WiringDocument wdCpy = wd;
            Sprite ico = this.App.wiringPane.GetCategoryIcon(wd.category);
            dropStack.AddAction(
                wdCpy == this.App.Wirings.Active,
                ico,
                wd.GetProcessedWiringName(this.App.Wirings),
                ()=>
                { 
                    this.App.SetActiveDocument(wdCpy, false); 
                });
        }

        //dropStack.
        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            dropStack.Root,
            this.pulldownActiveInstrument.rootPlate);

        this.App.DoVibrateButton();
    }

    protected override void AppSink_OnWiringActiveChanges(WiringCollection collection, WiringDocument wd)
    {
        if(wd == null)
            this.pulldownActiveInstrument.text.text = "--";
        else
        {
            this.pulldownActiveInstrument.text.text = 
                wd.GetProcessedWiringName(collection);

            if(this.gameObject.activeInHierarchy == true)
                this.pulldownActiveInstrument.SetInfoAnimate(this.App);
        }

    }

    protected override void AppSink_OnWiringRenamed(WiringCollection collection, WiringDocument wd)
    {
        if(wd != collection.Active)
            return;

        string procname = 
            wd.GetProcessedWiringName(collection);

        this.pulldownActiveInstrument.text.text = procname;

        if(this.gameObject.activeInHierarchy == true)
            this.pulldownActiveInstrument.SetInfoAnimate(this.App);
    }

    public override void AppSink_OnNoteStart(Application.NoteStartEvent eventType, int noteID, float velocity, int pressHandle)
    { 
        foreach(KeyDispBase kdb in this.lstKeyDisp)
            kdb.OnNoteStart(eventType, noteID, velocity, pressHandle);

        // If it was a MIDI note, draw a preview on the key for 
        // visual feedback, but only if the key exists.
        if(this.isActiveAndEnabled && eventType == Application.NoteStartEvent.Midi)
        { 
            WesternFreqUtils.Key k;
            int o;
            PxPre.Phonics.WesternFreqUtils.GetNoteInfo( noteID, out k, out o);
            //
            Key key;
            if(this.keyCollection.keyLookup.TryGetValue(new KeyPair(k, o), out key) == true)
            {
                Debug.Log("ENTER " + noteID.ToString());

                MIDIPrevGlyph mpg;
                if(this.midiPrevGlyphs.TryGetValue(noteID, out mpg) == true)
                {
                    Debug.Log("RM " + mpg.gameObject.GetHashCode().ToString());
                    GameObject.Destroy(mpg.gameObject);
                }

                const float midiHeightPos = 50.0f;
                MIDIPrevGlyph g = MIDIPrevGlyph.CreateOnKey( this, key, this.midiNotifier, midiHeightPos);
                if(g != null)
                {
                    Debug.Log("Add " + g.gameObject.GetHashCode().ToString());
                    this.midiPrevGlyphs.Add(noteID, g);
                }
            }
        }
    }

    public void OnEndedMIDIPrevGlyph(MIDIPrevGlyph glyph)
    { 
        if(glyph == null || glyph.gameObject == null)
            return;

        // If we don't have it, there's nothing to remove. Chances are because
        // it was already removed. 
        //
        // Most likely this happens from deletion on disable, and then the second
        // signal is from the delete message.
        MIDIPrevGlyph mpg;
        if(this.midiPrevGlyphs.TryGetValue(glyph.NoteID, out mpg) == false)
            return;

        if(mpg != glyph)
            return;

        if(this.midiPrevGlyphs.Remove(glyph.NoteID) == false)
            return;

        if(glyph == null || glyph.gameObject == null)
            return;

        GameObject.Destroy(glyph.gameObject);
    }

    public override void AppSink_OnNoteEnd(int pressHandle)
    { 
        foreach(KeyDispBase kdb in this.lstKeyDisp)
            kdb.OnNoteEnd(pressHandle);
    }
}
