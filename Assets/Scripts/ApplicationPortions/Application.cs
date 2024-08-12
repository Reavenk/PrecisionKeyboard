// <copyright file="Application.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/23/2020</date>
// <summary>The main application class.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;

using PxPre.Phonics;

public partial class Application : 
    MonoBehaviour, 
    IKeyboardApp,
    IMIDIMsgSink
{
    public const string VersionString = "0.1";
    public const string BuildString = "00.00.01";
    public const string BuildDate = "06/17/2020";

    public const float sleepOffsetMIDI = 60.0f;
    public const float sleepOffsetStartExercise = 120.0f;
    public const float sleepOffsetEndExercise = 60.0f;

    [DllImport("__Internal")]
    public static extern void BrowserTextDownload(string filename, string textContent);

    [DllImport("__Internal")]
    public static extern string BrowserTextUpload(string extFilter, string gameObjName, string dataSinkFn);

    [DllImport("__Internal")]
    public static extern void BrowserAlert(string msg);

    [DllImport("__Internal")]
    public static extern string BrowserGetLinkSearch();

    [DllImport("__Internal")]
    public static extern string BrowserGetLinkHREF();

    public static string GetRandomQuote()
    { 
        string [] rsts = 
            new string []
            { 
                // Quotes deleted from free version
                "RANDOM MESSAGE HERE"
            };

        return rsts[Random.Range(0, rsts.Length)];
    }

    public Color color; // TODO: Where is this used? Can it be removed?
    
    public enum TabTypes
    {
        // Not currently used. Only here to entertain the idea of expanding 
        // the set of tabs in the future.
        Misc,   

        Keyboard,
        Wiring,
        Options
    }
    [System.Serializable]
    public struct TabRecord
    {
        public UnityEngine.UI.Image tab;
        public UnityEngine.UI.Text tabText;
        public PaneBase pane;

        public bool Matches(TabRecord tr)
        { 
            return tr.pane == this.pane;
        }
    }

    public enum EStopState
    { 
        AudioValid,
        NoOutput
    }

    public enum NoteStartEvent
    { 
        // A note was started from being pressed on a keyboard
        Pressed, 

        // This should be treated like Pressed for the most part, 
        // but explicitly labels the note as comming from a MIDI
        // input
        Midi,

        // A note was started from some method other than being
        // pressed on a keyboard.
        Started,
    }

    

    private static Application instance;
    public static Application Instance {get{return instance;}}

    public ExerciseAssets exerciseAssets;
    public HelpAssets helpAssets;

    public UnityEngine.EventSystems.EventSystem eventSystem;

    public TweenUtil uiFXTweener;

    EStopState estopState = EStopState.NoOutput;

    private WiringFile wirings = new WiringFile();
    public WiringFile Wirings {get{return this.wirings; } }

    PxPre.Undo.UndoSession undoMgr = 
        new PxPre.Undo.UndoSession(1000);

    public MIDIMgr midiMgr;

    public SleepHoldMgr sleepHoldMgr;

    // 
    //      PLAYER PREFERENCE VARIABLES
    //////////////////////////////////////////////////
    public const string prefKey_MasterVol = "mastervol";    // The master volume level
    public const string prefKey_TabHeight = "tabheight";    // The tab heights
    public const string prefKey_LastSaveDir = "serialDir";  // The last save directory

    public const string prefApp_EverRun = "everRun";


    // 
    //      HIGH LEVEL APP VARIABLES
    //////////////////////////////////////////////////

    /// <summary>
    /// The RectTransform containing the notebook tabs
    /// </summary>
    public RectTransform tabRect;

    /// <summary>
    /// The RectTransform containing the test of the app besides
    /// the notebook tabs.
    /// </summary>
    public RectTransform appRect;

    public const float tabMinHeight = 30.0f;    // The minimum tab height
    public const float tabMaxHeight = 60.0f;    // The maximum tab height

    /// <summary>
    /// The sprite icon for the "leave exercise" button.
    /// </summary>
    public Sprite leaveExerciseIcon;

    public Sprite replayExerciseIcon;

    public List<Coroutine> exerciseCoroutines = new List<Coroutine>();

    // 
    //      TAB VARIABLES
    //////////////////////////////////////////////////

    /// <summary>
    /// The information for the edit-time authored tabs.
    /// </summary>
    public TabRecord [] tabs;

    /// <summary>
    /// The color of the tab when it's the active tab.
    /// </summary>
    public Color activeTabColor;

    /// <summary>
    /// The color of teh tab when it's not the active tab.
    /// </summary>
    public Color inactiveTabColor;

    /// <summary>
    /// The current active pane.
    /// </summary>
    PaneBase activePane = null;

    public PaneKeyboard keyboardPane;       // The keyboard tab/pane
    public PaneWiring wiringPane;           // The wiring tab/pane
    public PaneOptions optionsPane;         // The options and exercise listing tab/pane

    // 
    //      UI VARIABLES
    //////////////////////////////////////////////////

    /// <summary>
    /// The UI Layout Factory
    /// </summary>
    public PxPre.UIL.Factory uiFactory;
    public PxPre.UIL.DialogSpawner dlgSpawner;

    // 
    //      PLAYBACK VARIABLES
    //////////////////////////////////////////////////

    /// <summary>
    /// The current playback's sample-per-second when generating tone audio.
    /// </summary>
    public int samplesPerSeconds = 44100;

    /// <summary>
    /// The number of samples in the playback buffer.
    /// </summary>
    public int sourceSamples = 0;

    [UnityEngine.SerializeField]
    public GameObject coroutineDlgPrefab;

    /// <summary>
    /// All the regististered eStop button images. This is used to turn on or off
    /// the highlighting of eStop buttons based on if content is playing.
    /// </summary>
    private HashSet<UnityEngine.UI.Image> estopPlates = 
        new HashSet<UnityEngine.UI.Image>();

    public UnityEngine.UI.Image meterBar;

    [UnityEngine.SerializeField]
    private Queue<string> downloads = new Queue<string>();

    [UnityEngine.SerializeField]
    private Queue<string> messages = new Queue<string>();

    private DlgCoroutine openCoroutineDlg = null;

    /// <summary>
    /// The BPM value
    /// </summary>
    private float bpm = 120.0f;

    /// <summary>
    /// The tone generation manager, in charge of all the streaming audio sources.
    /// </summary>
    PxPre.Phonics.GenManager generatorMgr;

    BaseExercise currentExercise = null;
    public UnityEngine.UI.Text keyboardTabText;

    List<IAppEventSink> eventSinks = new List<IAppEventSink>();

    public float defaultVelocity = 0.75f;

    Dictionary<int, int> midiPlayingNotes = 
        new Dictionary<int, int>();

    // 
    //      FPS METERING VARIABLES
    //////////////////////////////////////////////////
    
    // 
    //      WIRING DOCUMENT VARIABLES
    //////////////////////////////////////////////////

    

    private void Awake()
    {
        // https://answers.unity.com/questions/168999/why-frame-per-second-script-always-show-me-60fps-.html
        UnityEngine.Application.targetFrameRate = -1;

        for(int i = 0; i < this.metronomeInstruments.Length; ++i)
        { 
            AudioClip ac = this.metronomeInstruments[i].audioClip;
            float [] rs = new float[ac.samples];
            ac.GetData(rs, 0);

            this.metronomeInstruments[i].samples = rs;
            this.metronomeInstruments[i].sampleCt = ac.samples;
            this.metronomeInstruments[i].samplesPerSec = ac.frequency;
        }

        float tabHeightPerc = PlayerPrefs.GetFloat(prefKey_TabHeight, 0.5f);
        this.SetTabHeightPercent(tabHeightPerc);
        //this.SetTabHeight(60.0f);

        this.uiFXTweener = new TweenUtil(this);

        this.metroTypes = 
            this.metronomePatterns[this.metronomeIdx].pattern;

#if !UNITY_EDITOR && UNITY_WEBGL
        string selflink = BrowserGetLinkHREF();
        System.Uri selfuri = new System.Uri(selflink);
        bool isWebValid = selfuri.Host.ToLower().Contains("pixeleuphoria.com");
        if(isWebValid == false)
        { 
            Debug.Log("Could not find neccessary resources");
            return;
        }
#endif

        PxPre.Phonics.GenWhite.BakeNoise();
        this.generatorMgr = new GenManager(this.gameObject);

        float masterVol = PlayerPrefs.GetFloat(prefKey_MasterVol, 0.4f);
        masterVol = Mathf.Clamp01(masterVol);
        this.generatorMgr.SetMasterVol(masterVol);

        float metroVol = PlayerPrefs.GetFloat(prefMetroVolume, this.GetMetronomeVolume());
        this.SetMetronomeVolume(metroVol);

        if (this.sourceSamples <= 0)
            this.sourceSamples = samplesPerSeconds;

        foreach(TabRecord tr in this.tabs)
            tr.pane.InitPane(this);

        // Do this in a seperate loop in case InitPane does weird stuff
        // that triggers events that wouldn't make it to every pane.
        foreach(TabRecord tr in this.tabs)
            this.eventSinks.Add(tr.pane);

        this.SetTabFromType(TabTypes.Keyboard);

        this.RemakeTitlebar();

        instance = this;
    }

    public void SetTabHeight(float height)
    { 
        height = Mathf.Clamp( height, tabMinHeight, tabMaxHeight);
        this.tabRect.sizeDelta = new Vector2(0.0f, height);
        this.appRect.offsetMax = new Vector2(0.0f, -height);
    }
    

    private void Start()
    {
        this.uiFactory.onCreateButton += (x)=>{ x.onClick.AddListener( ()=>{ this.DoVibrateButton();  });};
        this.uiFactory.onCreateSlider += (x)=>{ this.optionsPane.SetupSliderCreatedFromFactory(x);};
        this.dlgSpawner.onCreateDialog += (x)=>{ ModalStack.AddToStack(x.host.RT.gameObject); };

        PxPre.FileBrowse.FileBrowseSpawner fileBrowser = PxPre.FileBrowse.FileBrowseSingleton.Spawner;
        fileBrowser.onNavigationEvent += 
            (x, y)=>
            { 
                if(x != PxPre.FileBrowse.FileBrowser.NavigationType.Misc)
                    this.DoVibrateButton(); 
            };


        PxPre.DropMenu.DropMenuSingleton.MenuInst.onCreatedModalPlate += (x) => { ModalStack.AddToStack(x.gameObject); };
        this.HandleParameters();

        if(Screen.dpi > 1)
        {
            // http://ilkinulas.github.io/programming/unity/2016/03/18/unity_ui_drag_threshold.html
            int defaultValue = this.eventSystem.pixelDragThreshold;

            this.eventSystem.pixelDragThreshold =
                Mathf.Max(
                     defaultValue,
                     (int)(defaultValue * Screen.dpi / 160f));
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.onAction += 
            (x)=> 
            { 
                if(x == null)
                    this.DoVibrateBlipCancel();
                else
                    this.DoVibrateButton();
            };

        PxPre.DropMenu.DropMenuSingleton.MenuInst.onSubMenuOpened += 
            (x, y) =>
            { 
                this.DoVibrateButton();
            };

        // After everything is initialized, 
        // from Awake() and InitPane(),
        // create the starting wiring.
        if (this.wirings.Active == null)
        {
            try
            {
                this.LoadStartingDocument();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not load default starting document. Creating simple startup. : " + ex.Message);
                this.AddDefaultWiringDocument(false);
            }
        }

        this.RemakeTitlebar();

        // If we're running for the first time, bring up the intro
        if (PlayerPrefs.HasKey(prefApp_EverRun) == false)
        {
            PlayerPrefs.SetInt(prefApp_EverRun, 1);

            PxPre.UIL.Dialog dlgIntro = this.optionsPane.introContent.CreateDialog(this.exerciseAssets);
        }

        this.wiringPane.UpdateUndosButtons(this.undoMgr);
        this.midiMgr.AddDispatch(this);
    }

    

    private void OnApplicationPause(bool paused)
    { 
        //this.BringUpError("Paused " + paused.ToString());

        this.StopAllNotes();
        this.StopMetronome();

        // Forget any drag operations currently happening
        this.eventSystem.enabled = false;
        this.eventSystem.enabled = true;

        if(paused == false)
        { 
            this.keyboardPane.ReapplyQualityPriority();
        }
    }

    public float BeatsPerSecond()
    { 
        return bpm / 60.0f;
    }

    public float BeatsPerMinute()
    { 
        // !TODO:
        return bpm;
    }

    public const float minBMP = 1.0f;
    public const float maxBMP = 300.0f;
    public void SetBPM(float newBPM, bool force = false)
    { 
        float newClampedBPM = Mathf.Clamp(newBPM, minBMP, maxBMP);
        if(force == false && this.bpm == newClampedBPM)
            return;

        this.bpm = newClampedBPM;

        if(this.IsMetronomePlaying() == true)
        {
            this.StopMetronome();
            this.StartMetronome();
        }

        string bpmStr = this.GetBPMString();
        foreach(TabRecord tr in this.tabs)
            tr.pane.OnBPMChange(this.bpm, bpmStr);
    }

    void HandleParameters()
    {
        //string link2 = "http://localhost:81/webkeys/index.html?wire=webkeys/TestDL.phon";
        //System.Uri uri2 = new System.Uri(link2);

#if UNITY_WEBGL && !UNITY_EDITOR

        System.Uri uri = new System.Uri(BrowserGetLinkHREF());
        string linkParams = uri.Query;

        if(linkParams.Length == 0)
            return;

        if(linkParams[0] == '?')
            linkParams = linkParams.Substring(1);
        
        string [] sections = linkParams.Split(new char [] { '&' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach(string sec in sections)
        { 
            string [] split = sec.Split(new char[]{'=' }, System.StringSplitOptions.RemoveEmptyEntries);
            if(split.Length == 0)
                continue;

            if(split.Length == 2)
            {
                string keyword = split[0].ToLower();
                if(keyword == "wire")
                {
                    string targLink = split[1];

                    if(targLink.Contains("://") == false)
                    {    
                        // Relative
                        if (targLink[0] == '/')
                        {
                            List<string> lstSegs = new List<string>(uri.Segments);
                            if(lstSegs[0] == "/")
                                lstSegs.RemoveAt(0);

                            if(lstSegs.Count > 0)
                                lstSegs.RemoveAt(lstSegs.Count - 1);

                            targLink = uri.Scheme + "://" + uri.Authority + "/" + string.Join("/", lstSegs) + targLink;
                        }
                        else
                        {
                            string pureLink = uri.Scheme + "://" + uri.Authority + uri.LocalPath;
                            System.Uri uriPure = new System.Uri(pureLink);

                            targLink = pureLink + "/" + targLink;
                        }
                    }

                    Debug.Log("Added download target " + targLink);
                    this.AddDownload(targLink);
                }
            }

        }
#else
#endif
    }

    

    float fallingMeter = 0.0f;
    public void Update()
    {
        
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Pause) == true)
        { 
            GNUIHost.HonorEncasements = !GNUIHost.HonorEncasements;

            Vector2 origScroll = this.wiringPane.wiringRegion.normalizedPosition;
            this.SetActiveDocument(this.Wirings.Active, true);
            this.wiringPane.wiringRegion.normalizedPosition = origScroll;
        }

        if(Input.GetKeyDown(KeyCode.F5) == true)
        { 
            Vector2 origScroll = this.wiringPane.wiringRegion.normalizedPosition;
            this.SetActiveDocument(this.Wirings.Active, true);
            this.wiringPane.wiringRegion.normalizedPosition = origScroll;
        }
        
#endif

        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            this.StopAllNotes();
            ModalStack.Pop();
        }

        this.optionsPane.TickFPS(Time.unscaledDeltaTime);

        int finished = this.generatorMgr.CheckFinishedRelease();
        if(finished != 0)
        { 
            if(this.generatorMgr.AnyPlaying() == false)
                this.SetEStops(false);
        }

        if(this.generatorMgr.metered == true)
        {
            // The rate at which the meter falls
            const float meterFall = 2.0f;

            this.fallingMeter = 
                Mathf.Max(0.0f, this.fallingMeter - Time.deltaTime * meterFall);

            float meter = this.generatorMgr.EstimateMeter();
            this.fallingMeter = Mathf.Max( this.fallingMeter, meter);
            this.fallingMeter = Mathf.Clamp01(this.fallingMeter);
        }
        else
            this.fallingMeter = 0.0f;

        this.meterBar.fillAmount = this.fallingMeter;

#if UNITY_EDITOR
        //if (Input.GetKeyDown(KeyCode.O) == true)
        //{ 
        //    string docString = this.wiringPane.ConvertToXMLString();
        //
        //    System.IO.File.WriteAllText(
        //        "C:\\users\\reavenk\\desktop\\document.phon",
        //        docString);
        //    
        //}
        //if(Input.GetKeyDown(KeyCode.P) == true)
        //{ 
        //    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        //    doc.Load("C:\\users\\reavenk\\desktop\\document.phon");
        //    
        //    WiringDocument wdActive = null;
        //    this.wiringPane.LoadDocument(doc, ref wdActive);
        //    
        //    if(wdActive != null)
        //        this.wiringPane.SetActiveDocument(wdActive);
        //
        //    this.AddDownload("file://C:\\users\\reavenk\\desktop\\document.phon");
        //}
#endif
    }

    public void DestroySelf()
    { 
        GameObject.Destroy(this.gameObject);
    }

    public bool AddEStop(UnityEngine.UI.Image buttonPlate, bool addcallback)
    { 
        bool added = this.estopPlates.Add(buttonPlate);

        if(added == false)
            return false;

        if(addcallback == true)
        { 
            UnityEngine.UI.Button btn = buttonPlate.GetComponent<UnityEngine.UI.Button>();
            if(btn != null)
            {
                btn.onClick.AddListener(
                    ()=>
                    { 
                        this.OnButton_EStop(); 
                    });
            }
        }

        return true;
    }

    public bool RemoveEStop(UnityEngine.UI.Image buttonPlate)
    { 
        return this.estopPlates.Remove(buttonPlate);
    }

    Color GetEStopColor(bool activeNotes)
    {
        if (activeNotes == true)
                return Color.red;

        // The active notes value can come from either the eStop
        // return, or just directly checking if any are playing.
        if (this.estopState == EStopState.AudioValid)
                return Color.white;
        else
            // Orange
            return new Color(1.0f, 0.6f, 0.0f);
    }

    Color GetEStopColor()
    { 
        return this.GetEStopColor(this.generatorMgr.AnyPlaying());
    }

    public void SetEStops(bool active)
    {
        Color c = this.GetEStopColor(active);
        foreach (UnityEngine.UI.Image img in this.estopPlates)
            img.color = c;
    }

    public void UpdateEStops()
    {
        Color c = this.GetEStopColor();
        foreach (UnityEngine.UI.Image img in this.estopPlates)
            img.color = c;
    }

    public bool SetTabFromPane(RectTransform rt)
    {
        foreach(TabRecord tr in this.tabs)
        { 
            if(tr.pane == rt)
            { 
                this.SetTabFromRecord(tr);
                return true;
            }
        }
        return false;
    }

    public bool SetTabFromTab(UnityEngine.UI.Image tab)
    {
        foreach(TabRecord tr in this.tabs)
        { 
            if(tr.tab == tab)
            { 
                this.SetTabFromRecord(tr);
                return true;
            }
        }
        return false;
    }

    public bool SetTabFromType(TabTypes tt)
    {

        foreach(TabRecord tr in this.tabs)
        { 
            if(tr.pane.TabType() == tt)
            { 
                this.SetTabFromRecord(tr);
                return true;
            }
        }
        return false;
    }

    public void NotifyActiveWiringChanged(WiringDocument wd)
    { 
        if(wd == null)
        {
            this.estopState = EStopState.NoOutput;
            this.UpdateEStops();
        }
        else
            NotifyActiveOutputUpdated(wd.Output);

    }

    public void RefreshActiveOutput()
    { 
        LLDNOutput gno = this.Wirings.ActiveOutput();
        this.NotifyActiveOutputUpdated(gno);
    }

    public void NotifyActiveOutputUpdated(LLDNOutput gno)
    { 
        // This might be fragile because it assumes gno came from 
        // this.Wirings and this.Wirings.Active, but we'll run with
        // it for now.
        if(gno == null || gno.TreeAppearsValid(this.Wirings, this.Wirings.Active) == false)
            this.estopState = EStopState.NoOutput;
        else
            this.estopState = EStopState.AudioValid;

        this.UpdateEStops();
    }

    private void SetTabFromRecord(TabRecord trActive)
    {
        if(trActive.pane != this.activePane)
            this.StopMetronome();

        // If there are keys pressed, unpress them. Note that this isn't just the keys on the
        // keyboard tab, there's key in other tabs and other places. If we don't stop them, they'll
        // continue on afterwards until an E-stop.
        Key.EndAllNotes(this);

        trActive.tab.rectTransform.SetAsLastSibling();
        trActive.tab.color = this.activeTabColor;
        trActive.pane.gameObject.SetActive(true);

        foreach (TabRecord tr in this.tabs)
        { 
            if(tr.Matches(trActive) == true)
                continue;

            tr.pane.gameObject.SetActive(false);
            tr.tab.color = this.inactiveTabColor;
        }

        this.UpdateTabOpacitiesForExercises(this.RunningExercise());
    }

    public void SetTabKeyboard()
    {
        this.DoVibrateButton();

        this.SetTabFromType(TabTypes.Keyboard);
    }

    public void SetTabWiring()
    { 
        this.DoVibrateButton();

        if(this.currentExercise != null)
            this.QueryStopExerciseForTab("Wiring", TabTypes.Wiring);
        else
            this.SetTabFromType(TabTypes.Wiring);
    }

    public void SetTabOptions()
    {
        this.DoVibrateButton();

        if (this.currentExercise != null)
            this.QueryStopExerciseForTab("Options", TabTypes.Options);
        else
            this.SetTabFromType(TabTypes.Options);
    }

    void QueryStopExerciseForTab(string tabName, TabTypes tab)
    { 
        PxPre.UIL.Dialog dlg = 
            this.dlgSpawner.CreateDialogTemplate(
                "Stop Exercise?",
                $"In order to view the {tabName} tab, you must stop the current exercise.",
                new PxPre.UIL.DlgButtonPair("Cancel", null),
                new PxPre.UIL.DlgButtonPair(
                    "Stop Exercise", 
                    (x)=>
                    {
                        this.StopExercise();
                        this.SetTabFromType(tab);
                        this.keyboardPane.PlayAudio_LeaveExercise();
                        return true;
                    }));

        dlg.host.LayoutInRT(false);
    }

    public bool StopAllNotes()
    {
        bool ret = this.generatorMgr.StopAllNotes();

        this.SetEStops(false);
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gen"></param>
    /// <param name="noteId">Not functionally used, just for messaging purposes.</param>
    /// <param name="velocity"></param>
    /// <param name="keypress"></param>
    /// <returns></returns>
    public int StartNote(GenBase gen, int noteId, float velocity, NoteStartEvent keypress)
    { 
        int idx = this.generatorMgr.StartGenerator(gen, velocity, this.sourceSamples, this.samplesPerSeconds);
        this.SetEStops(true);

        foreach(IAppEventSink iaes in this.eventSinks)
            iaes.OnNoteStart(keypress, noteId, velocity, idx);

        if(keypress == NoteStartEvent.Pressed || keypress == NoteStartEvent.Midi)
        {
            this.DoVibrateKeyboard();

            // We don't need to worry about presses on mobile devices, the OS will do
            // whatever in that case.
            if(keypress == NoteStartEvent.Midi)
                this.sleepHoldMgr.StayAwakeFor(sleepOffsetMIDI);

            if(this.currentExercise != null)
            {
                PxPre.Phonics.WesternFreqUtils.Key k;
                int octave;
                PxPre.Phonics.WesternFreqUtils.GetNoteInfo(noteId, out k, out octave);

                this.currentExercise.OnNoteStart(idx, new KeyPair(k, octave));
            }
        }

        return idx;
    }

    public bool EndNote(int keyplayHandle, bool release = true)
    {
        this.generatorMgr.StopNote(keyplayHandle, release);

        if(this.generatorMgr.AnyPlaying() == false)
            this.SetEStops(false);

         foreach(IAppEventSink iaes in this.eventSinks)
            iaes.OnNoteEnd(keyplayHandle);

        if(this.currentExercise != null)
            this.currentExercise.OnNoteEnd(keyplayHandle);

        return true;
    }

    void AddDownload(string link)
    { 
        this.downloads.Enqueue(link);

        if(this.openCoroutineDlg == null)
        { 
            GameObject go = GameObject.Instantiate(this.coroutineDlgPrefab);
            go.transform.SetParent(CanvasSingleton.canvas.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            DlgCoroutine dlgc = go.GetComponent<DlgCoroutine>();
            RectTransform rt= dlgc.GetComponent<RectTransform>();
            rt.anchorMin = Vector3.zero;
            rt.anchorMax = Vector3.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            this.openCoroutineDlg = dlgc;
            dlgc.coroutine = this.StartCoroutine(this.DownloadsCoroutine(dlgc));
        }
    }

    IEnumerator DownloadsCoroutine(DlgCoroutine dlg)
    { 
        dlg.title.text = "Downloading Wiring";
        dlg.cancelText.text = "Cancel";

        WiringDocument wd = null;

        while(this.downloads.Count > 0)
        { 
            string toDownload = 
                this.downloads.Dequeue();

            System.Uri path = new System.Uri(toDownload);
            string localPath = path.LocalPath;

            dlg.message.text = "Downloading:\n" + localPath;
            WWW www = new WWW(toDownload);
            yield return www;

            if(string.IsNullOrEmpty(www.error) == false)
            { 
                dlg.message.text = "Error downloading:\n" + localPath + "\n\n" + www.error;
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            dlg.message.text = "Loading...";

            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
            xmldoc.LoadXml(www.text);


            if(this.wirings.LoadDocument(xmldoc, false) == false)
            {
                dlg.message.text = "Error loading\n" + localPath;
                yield return new WaitForSeconds(0.5f);
            }

            dlg.message.text = "Finished loading\n" + localPath;
            yield return new WaitForSeconds(0.5f);
        }

        if(this.openCoroutineDlg == dlg)
            this.openCoroutineDlg = null;

        GameObject.Destroy(dlg.gameObject);

        if(wd != null)
            this.SetActiveDocument(wd, false);

        this.BringUpNextMessage();
    }

    // TODO: Mechanize better
    void BringUpNextMessage()
    { 
        if(this.messages.Count == 0)
            return;

        string msg = this.messages.Dequeue();
        string close = "Gotcha!";
        string title = "Message";

        if (this.messages.Count != 0)
        {
            title = "Messages(" + this.messages.Count.ToString() + ")";
            close = "Next";
        }
        this.dlgSpawner.CreateDialogTemplate(
            title, 
            msg, 
            new PxPre.UIL.DlgButtonPair(
                close, 
                (x)=>
                {
                    this.BringUpNextMessage();
                    return true;
                })).host.LayoutInRT();
    }

    public void EnqueueDocumentMessage(string message)
    { 
        this.messages.Enqueue(message);
    }

    public void BringUpError(string message)
    {
        this.dlgSpawner.CreateDialogTemplate(
            "Sorry!", 
            message, 
            new PxPre.UIL.DlgButtonPair("OK", null)).host.LayoutInRT();
    }

    public void OnButton_EStop(bool vibrate = true)
    { 
        this.DoVibrateButton();
        this.EStop(vibrate);
    }

    public void EStop(bool vibrate = true)
    {
        if(this.StopAllNotes() == true)
        {
#if !UNITY_WEBGL
            if (vibrate == true)
                Handheld.Vibrate();
#endif
        }

        if (this.currentExercise != null)
            this.currentExercise.OnEStop();

        foreach(TabRecord tr in this.tabs)
            tr.pane.OnEStop();

        this.StopMetronome();

        this.midiPlayingNotes.Clear();
    }

    public bool RunningExercise()
    { 
        return this.currentExercise != null;
    }

    public Coroutine StartExerciseCoroutine(IEnumerator ie)
    { 
        if(this.RunningExercise() == false)
            return null;

        Coroutine cr = this.StartCoroutine(ie);
        this.exerciseCoroutines.Add(cr);
        return cr;
    }

    public bool RecordExerciseCoroutine(Coroutine cr)
    { 
        if(this.RunningExercise() == false)
            return false;

        this.exerciseCoroutines.Add(cr);
        return true;
    }

    public bool StopExerciseCoroutine(Coroutine cr)
    { 
        if(this.exerciseCoroutines.Remove(cr) == false)
            return false;

        this.StopCoroutine(cr);
        return true;
    }

    public bool StopExercise()
    { 
        if(this.currentExercise == null)
            return false;
        
        this.keyboardTabText.text = "Keyboard";
        this.keyboardPane.DestroyExercise(this.currentExercise);

        foreach(Coroutine c in this.exerciseCoroutines)
            this.StopCoroutine(c);

        this.exerciseCoroutines.Clear();

        this.EStop();

        //GameObject.Destroy(this.currentExercise.gameObject);
        this.currentExercise = null;
        
        this.UpdateTabOpacitiesForExercises(false);
        
        foreach (TabRecord tr in this.tabs)
            tr.pane.OnExerciseStop();

        return false;
    }

    public bool StartExercise(BaseExercise exercise, PaneKeyboard.ExerciseDisplayMode displayMode)
    { 
        this.EStop();
        this.StopExercise();

        if(exercise == null)
            return false;

        if(this.keyboardPane.SetupExercise(exercise, displayMode) == false)
        {
            this.keyboardPane.SwitchKeyboardDisplayMode(null);
            this.keyboardTabText.text = "Keyboard";
            return false;
        }

        this.currentExercise = exercise;

        this.SetTabKeyboard();
        this.keyboardPane.SwitchKeyboardDisplayMode(displayMode);
        this.currentExercise.StartExercise(displayMode);
        this.keyboardTabText.text = "Exercise";

        this.UpdateTabOpacitiesForExercises(true);

        foreach (TabRecord tr in this.tabs)
            tr.pane.OnExerciseStart();

        return true;
    }

    public void UpdateTabOpacitiesForExercises(bool exercise)
    {
        foreach (TabRecord tr in this.tabs)
        {
            if (tr.pane == this.keyboardPane)
                continue;

            Color c = tr.tab.color;
            if(exercise == true)
            {
                //tr.tab.color = new Color(c.r, c.g, c.b, 0.5f);
                tr.tabText.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            }
            else
            {
                //tr.tab.color = new Color(c.r, c.g, c.b, 1.0f);
                tr.tabText.color = Color.black;
            }
        }
    }

    public int PressKey(PxPre.Phonics.WesternFreqUtils.Key k, int octave, NoteStartEvent keypress)
    {
        int noteid = PxPre.Phonics.WesternFreqUtils.GetNote(k, octave);
        return this.PressKey(noteid, keypress);
    }

    public int PressKey(KeyPair note, NoteStartEvent keypress)
    {
        int noteid = PxPre.Phonics.WesternFreqUtils.GetNote(note.key, note.octave);
        return this.PressKey(noteid, keypress);
    }

    public int PressKey(int i, NoteStartEvent keypress)
    {
        float fr = PxPre.Phonics.WesternFreqUtils.GetFrequency(i);
        GenBase gb = this.wiringPane.GenerateForFrequency(fr);
        int noteHandle = this.StartNote(gb, i, this.defaultVelocity, keypress);
        return noteHandle;
    }

    public PxPre.UIL.Dialog CreateRenameDialog(
        string title,
        string message,
        string label,
        string placeholder,
        string startingString,
        string okButton,
        System.Action<string> onOK,
        int limit = 0)
    {
        PxPre.UIL.Dialog dlg = this.dlgSpawner.CreateDialogTemplate();

        dlg.AddDialogTemplateTitle(title);

        dlg.contentSizer.Add(new PxPre.UIL.EleSpace(1.0f), 1.0f, PxPre.UIL.LFlag.Grow);

        // Message
        PxPre.UIL.EleText exeTxt = this.uiFactory.CreateText(dlg.rootParent, message, 20, true);
        dlg.contentSizer.Add(exeTxt, 0.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz);
        exeTxt.text.alignment = TextAnchor.MiddleCenter;

        // Rename item

        dlg.contentSizer.Add(new PxPre.UIL.EleSpace(1.0f), 1.0f, PxPre.UIL.LFlag.Grow);

        PxPre.UIL.EleBoxSizer inputBS = this.uiFactory.HorizontalSizer(dlg.contentSizer, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        PxPre.UIL.EleText labelTxt = this.uiFactory.CreateText(dlg.rootParent, label, 20, false);
        inputBS.Add(new PxPre.UIL.EleSpace(30.0f), 0.0f, 0);
        inputBS.Add(labelTxt, 0.0f, PxPre.UIL.LFlag.AlignBot);
        inputBS.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
        PxPre.UIL.EleInput inputTxt = this.uiFactory.CreateInput(dlg.rootParent);
        inputBS.Add(inputTxt, 1.0f, PxPre.UIL.LFlag.AlignBot|PxPre.UIL.LFlag.GrowHoriz);
        inputBS.Add(new PxPre.UIL.EleSpace(30.0f), 0.0f, 0);
        inputTxt.input.lineType = UnityEngine.UI.InputField.LineType.MultiLineSubmit;

        if(limit > 0)
            inputTxt.input.characterLimit = limit;

        //UnityEngine.EventSystems.EventTrigger et = inputTxt.input.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        //et.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry>();
        //UnityEngine.EventSystems.EventTrigger.Entry e = new UnityEngine.EventSystems.EventTrigger.Entry();
        //e.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
        //e.callback.AddListener((x)=>{ Debug.Log("yo!");});
        //et.triggers.Add(e);
        inputTxt.input.onEndEdit.AddListener(
            (x)=>
            { 
                if(Input.GetKeyDown(KeyCode.Return) == true)
                {
                    onOK(x);
                    GameObject.Destroy(dlg.host.RT.gameObject);
                }

            });

        inputTxt.input.text = startingString;
        inputTxt.text.text = placeholder;

        dlg.contentSizer.Add(new PxPre.UIL.EleSpace(1.0f), 1.0f, PxPre.UIL.LFlag.Grow);

        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(
            new PxPre.UIL.DlgButtonPair("Cancel", null),
            new PxPre.UIL.DlgButtonPair(okButton, (x)=>{ onOK(inputTxt.input.text); return true;}));

        return dlg;
    }

    public void SetMasterVolume(float newMaster)
    {
        this.generatorMgr.SetMasterVol(newMaster);

        foreach(TabRecord tr in this.tabs)
            tr.pane.OnMasterVolumeChange(newMaster);

        PlayerPrefs.SetFloat(prefKey_MasterVol, newMaster);
    }

    public void SetTabHeightPercent(float percent)
    { 
        percent = Mathf.Clamp01(percent);
        PlayerPrefs.SetFloat(prefKey_TabHeight, percent);

        float height = Mathf.Lerp(tabMinHeight, tabMaxHeight, percent);
        this.SetTabHeight(height);
    }

    public float GetTabHeightPercent()
    { 
        return 
            Mathf.InverseLerp(
                tabMinHeight, 
                tabMaxHeight,
                this.tabRect.sizeDelta.y);
    }

    public void NewDocument()
    { 
        // TODO: Dialog & Dirty check

        this.wirings.Clear();
        this.wirings.AddNewWiringDocument("Default", true);

        this.undoMgr.ClearSession();
        this.FlagAppStateDirty();

        foreach(TabRecord tr in this.tabs)
            tr.pane.OnDocumentChanged(this.wirings, this.wirings.Active, false);
    }

    public void Save(System.Action afterSave, RectTransform spawner)
    {
        if(string.IsNullOrEmpty(this.wirings.Path) == true)
        { 
            SaveAsDialog(afterSave, spawner);
            return;
        }

        this.wirings.Save();

        this.NotifyDocumentSaved();


        afterSave?.Invoke();
    }

    public void DownloadFile(string filename)
    {
        string docString = this.wirings.ConvertToXMLString();
        Application.BrowserTextDownload(filename, docString);

        this.NotifyDocumentSaved();
        this.FlagAppStateDirty();
    }

    void NotifyDocumentSaved()
    {
        foreach (TabRecord tr in this.tabs)
            tr.pane.OnSave();

        this.undoMgr.ResetDirtyCounter();
        this.FlagAppStateDirty();
    }

    const string dialogFilter = "Precision Keyboard Phonics File (*.phon)|*.phon";

    public PxPre.UIL.Dialog SaveAsDialog(System.Action afterSave, RectTransform spawner)
    {
        string startingDir = string.Empty;
        string startingFile = this.wirings.Filename;

        if(string.IsNullOrEmpty(this.wirings.Path) == false)
            startingDir = System.IO.Path.GetDirectoryName(this.wirings.Path);

        if (string.IsNullOrEmpty(this.wirings.Path) == true)
            startingDir = PlayerPrefs.GetString(prefKey_LastSaveDir, "");
        
        PxPre.UIL.Dialog dlg = 
            this.CreateFileDialog(
                "Save Wiring", 
                false,
                startingDir,
                startingFile,
                dialogFilter, 
                true,
                (x)=>
                { 
                    try
                    {
                        string dir = System.IO.Path.GetDirectoryName(x);
                        PlayerPrefs.SetString(prefKey_LastSaveDir, dir);

                        this.wirings.Save(x);

                        this.NotifyDocumentSaved();

                        afterSave?.Invoke();
                    }
                    catch(System.Exception ex)
                    { 
                        Debug.LogError("Exception caught: " + ex.Message);
                    }
                });

        if(spawner != null)
            this.SetupDialogForTransition(dlg, spawner);

        return dlg;
    }

    public PxPre.UIL.Dialog OpenDialog()
    { 
        string savedStartingDir = PlayerPrefs.GetString(prefKey_LastSaveDir, "");

        PxPre.UIL.Dialog dlg = 
            this.CreateFileDialog(
                "Open Wiring", 
                true,
                savedStartingDir, 
                "",
                dialogFilter, 
                false,
                (x)=>
                {
                    try
                    {
                        string dir = System.IO.Path.GetDirectoryName(x);
                        PlayerPrefs.SetString(prefKey_LastSaveDir, dir);

                        this.LoadDocumentFile(x);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Exception caught: " + ex.Message);
                    }
                });

        return dlg;
    }

    public PxPre.UIL.Dialog AppendDialog()
    { 
        string savedStartingDir = PlayerPrefs.GetString(prefKey_LastSaveDir, "");

        PxPre.UIL.Dialog dlg = 
            this.CreateFileDialog(
                "Append Wiring", 
                true,
                savedStartingDir, 
                "",
                dialogFilter, 
                false,
                (x)=>
                {
                    try
                    {
                        string dir = System.IO.Path.GetDirectoryName(x);
                        PlayerPrefs.SetString(prefKey_LastSaveDir, dir);

                        this.AppendDocumentFile(x);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Exception caught: " + ex.Message);
                    }
                });

        return dlg;
    }

    public PxPre.UIL.Dialog CreateFileDialog(
        string title, 
        bool mustExist, 
        string startingDirectory, 
        string startingFilename, 
        string filter, 
        bool saving,
        System.Action<string> onok)
    {
        PxPre.UIL.Dialog dlg = this.dlgSpawner.CreateDialogTemplate(false, true);

        // Insert title text
        dlg.AddDialogTemplateTitle(title);

        PxPre.FileBrowse.FileBrowser fb = 
            PxPre.FileBrowse.FileBrowseSingleton.Spawner.CreateFileBrowser(dlg.rootParent.RT, filter, saving);

        fb.mustExist = mustExist;

        RectTransform rtFB = fb.gameObject.AddComponent<RectTransform>();
        rtFB.pivot = new Vector2(0.0f, 1.0f);
        rtFB.anchorMin = new Vector2(0.0f, 1.0f);
        rtFB.anchorMax = new Vector2(0.0f, 1.0f);

        PxPre.UIL.EleRT ert = new PxPre.UIL.EleRT(rtFB, new Vector2(500.0f, 350.0f));
        dlg.contentSizer.Add(ert, 1.0f, PxPre.UIL.LFlag.Grow);

        fb.closeFn = () => {};

        if(string.IsNullOrEmpty(startingDirectory) == true)
            startingDirectory = UnityEngine.Application.persistentDataPath;

        // Canonlicalize
        System.IO.DirectoryInfo diCur = new System.IO.DirectoryInfo(startingDirectory);
        fb.ViewDirectory(diCur.FullName, PxPre.FileBrowse.FileBrowser.NavigationType.Misc);
        fb.RestartHistory();

        fb.onOK = onok;
        fb.closeFn = ()=>{ GameObject.Destroy(dlg.host.RT.gameObject); };

        string startingFilepath = string.Empty;
        if(string.IsNullOrEmpty(startingFilename) == false)
            startingFilepath = System.IO.Path.Combine(startingDirectory, startingFilename);

        fb.SelectFile(startingFilepath, false, false);

        // Add separator
        dlg.AddDialogTemplateSeparator();
        PxPre.UIL.Dialog.OptionsButton [] optBtns = 
            dlg.AddDialogTemplateButtons(
                new PxPre.UIL.DlgButtonPair[]
                {
                    new PxPre.UIL.DlgButtonPair("Cancel", (x)=>{return true;}),
                    new PxPre.UIL.DlgButtonPair( saving ? "Save" : "Open", (x)=>{ return fb.OK(); })
                }); ;

        UnityEngine.UI.Button actionBtn = optBtns[1].button;
        fb.onViewDirectory += 
            (x,y)=>
            { 
                bool accessible = false;
                try
                { 
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(y);
                    System.IO.FileInfo [] rfi = di.GetFiles();
                    Debug.Log(rfi.Length.ToString()); // Make sure rfi isn't optimized out

                    accessible = true;
                }
                catch(System.Exception)
                { }
                actionBtn.interactable = accessible;
            };

        dlg.host.LayoutInRT();

        return dlg;
    }

    public IEnumerator TransitionReticule(RectTransform parent, Vector3 [] srcRect, Vector3 [] dstRect, UnityEngine.UI.Graphic graphic)
    {
        Sprite[] sps =
            new Sprite[]
            {
                this.exerciseAssets.reticuleBL,
                this.exerciseAssets.reticuleTL,
                this.exerciseAssets.reticuleTR,
                this.exerciseAssets.reticuleBR
            };

        List<UnityEngine.UI.Image> rts = new List<UnityEngine.UI.Image>();

        for (int i = 0; i < 4; ++i)
        {
            GameObject go = new GameObject("Corner_" + i.ToString());
            go.transform.SetParent(parent, false);
            UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
            img.sprite = sps[i];

            switch (i)
            {
                case 0:
                    img.rectTransform.pivot = new Vector2(0.0f, 0.0f);
                    break;

                case 1:
                    img.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                    break;

                case 2:
                    img.rectTransform.pivot = new Vector2(1.0f, 1.0f);
                    break;

                case 3:
                    img.rectTransform.pivot = new Vector2(1.0f, 0.0f);
                    break;
            }
            img.rectTransform.sizeDelta = img.sprite.rect.size;
            rts.Add(img);
            img.rectTransform.SetAsLastSibling();
        }

        const float offs = 10.0f;
        Vector3[] rvs =
            new Vector3[]
            {
                new Vector3(    -offs,  -offs    , 0.0f),
                new Vector3(    -offs,   offs    , 0.0f),
                new Vector3(    offs,   offs   , 0.0f),
                new Vector3(    offs,  -offs   , 0.0f),
            };

        float startTime = Time.time;
        const float durRetMove = 0.4f;
        const float durRetFade = 0.6f;


        float alpha = 1.0f;

        if (graphic != null)
            alpha = graphic.color.a;

        while (Time.time < startTime + durRetMove)
        {
            float lam = (Time.time - startTime) / durRetMove;

            if (graphic != null)
            {
                Color c = graphic.color;
                c.a = alpha * lam;
                graphic.color = c;
            }

            for (int i = 0; i < 4; ++i)
            {
                Vector3 s = srcRect[i] + rvs[i];
                Vector3 d = dstRect[i] + rvs[i];

                rts[i].transform.position = Vector3.Lerp(s, d, lam);
            }

            yield return null;
        }

        for (int i = 0; i < 4; ++i)
        {
            Vector3 d = dstRect[i] + rvs[i];
            rts[i].transform.position = d;
        }

        if (graphic != null)
        {
            Color col = graphic.color;
            col.a = alpha;
            graphic.color = col;
        }

        startTime = Time.time;
        while (Time.time < startTime + durRetFade)
        {
            float lam = (Time.time - startTime) / durRetFade;
            for (int i = 0; i < 4; ++i)
                rts[i].color = new Color(1.0f, 1.0f, 1.0f, 1.0f - lam);

            yield return null;
        }

        for (int i = 0; i < 4; ++i)
        {
            if (rts[i] == null || rts[i].gameObject == null)
                continue;

            GameObject.Destroy(rts[i].gameObject);
        }
    }

    public IEnumerator TransitionReticule(RectTransform parent, RectTransform src, RectTransform dst, UnityEngine.UI.Graphic graphic)
    {
        Vector3[] srcRect = new Vector3[4];
        src.GetWorldCorners(srcRect);

        Vector3[] dstRect = new Vector3[4];
        dst.GetWorldCorners(dstRect);

        return TransitionReticule(parent, srcRect, dstRect, graphic);
    }

    public void TransitionReticule(RectTransform src, RectTransform dst)
    { 
        this.StartCoroutine(
            this.TransitionReticule( 
                CanvasSingleton.canvas.GetComponent<RectTransform>(), 
                src, 
                dst, 
                null));
    }

    public void TransitionReticule(RectTransform targ, Vector2 offset)
    { 
        Vector3 [] dst = new Vector3[4];
        targ.GetWorldCorners(dst);

        Vector3 [] src = 
            new Vector3[]
            { 
                new Vector3(dst[0].x - offset.x, dst[0].y - offset.y, dst[0].z),
                new Vector3(dst[1].x - offset.x, dst[1].y + offset.y, dst[1].z),
                new Vector3(dst[2].x + offset.x, dst[2].y + offset.y, dst[2].z),
                new Vector3(dst[3].x + offset.x, dst[3].y - offset.y, dst[3].z)
            };

        this.StartCoroutine(
            this.TransitionReticule( 
                CanvasSingleton.canvas.GetComponent<RectTransform>(), 
                src,
                dst,
                null));
    }

    public void TransitionReticule(RectTransform targ)
    { 
        const float highlightRetOff = 50.0f;

        this.TransitionReticule(
            targ,
            new Vector2(highlightRetOff, highlightRetOff));
    }


    public FutureInterupt SetupDialogForTransition(PxPre.UIL.Dialog dlg, RectTransform spawner, bool fadePlate = true, bool setupReverse = true)
    { 
        UnityEngine.UI.Graphic graphic = null;
        if(fadePlate == true)
            graphic = dlg.host.RT.GetComponent<UnityEngine.UI.Graphic>();

        DoNothing dn = dlg.host.RT.gameObject.AddComponent<DoNothing>();
        dn.StartCoroutine( 
            this.TransitionReticule(
                dlg.host.RT,
                spawner,
                dlg.rootParent.RT,
                graphic));

        if(setupReverse == true)
        {
            FutureInterupt ret = new FutureInterupt();

            dlg.onClose += 
                ()=>
                { 
                    if(ret.IsInterrupted() == true)
                        return;

                    this.StartCoroutine(
                        this.TransitionReticule(
                            CanvasSingleton.canvas.GetComponent<RectTransform>(),
                            dlg.rootParent.RT,
                            spawner,
                            null));
                };

            return ret;
        }

        return null;
    }

    public float MasterVolume()
    { 
        return this.generatorMgr.MasterVol;
    }

    public void NotifyAccidentalsChanged()
    { 
        KeyCollection.Accidental acc = this.keyboardPane.GetAccidental();

        foreach(TabRecord tr in this.tabs)
            tr.pane.OnChangedAccidentalMode(acc);
    }

    // When an icon changes in the UI from the pulldown, this is the standard
    // function to animate the icon to pull the user's attention.
    public void DoDropdownIconUpdate(UnityEngine.UI.Graphic g)
    {
        DoDropdownIconUpdate(this.uiFXTweener, g);
    }

    public static void DoDropdownIconUpdate(TweenUtil tu, UnityEngine.UI.Graphic g)
    {
        tu.SlidingAnchorFade(
            g.rectTransform,
            g,
            new Vector2(0.0f, 30.0f),
            true,
            true,
            0.2f,
            TweenUtil.RestoreMode.RestoreLocal);
    }

    public void DoChevyIconEffect(UnityEngine.UI.Graphic g, bool delAfter = false)
    {
        DoChevyIconEffect(this.uiFXTweener, g, delAfter);
    }

    public static void DoChevyIconEffect(TweenUtil tu, UnityEngine.UI.Graphic g, bool delAfter = false)
    {
        tu.SlidingAnchorFade(
            g.rectTransform,
            g,
            new Vector2(0.0f, -20.0f),
            false,
            false,
            0.3f,
            delAfter ? TweenUtil.RestoreMode.Delete : TweenUtil.RestoreMode.RestoreLocal);
    }

    public static void DoDropdownTextUpdate(TweenUtil tu, UnityEngine.UI.Text t, bool revDir = false)
    {

        Vector2 dir = new Vector2(10.0f, 0.0f);
        if(revDir == true)
            dir.x = -dir.x;

        tu.SlidingAnchorFade(
            t.rectTransform,
            t,
            dir,
            true,
            true,
            0.2f,
            TweenUtil.RestoreMode.RestoreLocal|TweenUtil.RestoreMode.Alpha);
    }

    public void DoDropdownTextUpdate(UnityEngine.UI.Text t, bool revDir = false)
    {
        DoDropdownTextUpdate(this.uiFXTweener, t, revDir);
    }

    public void DoDropdownIconUpdate(UnityEngine.UI.Image img, Sprite sprite)
    { 
        if(img.sprite == sprite)
            return;

        img.sprite = sprite;
        this.DoDropdownIconUpdate(img);
    }

    public void OpenLink(string url, string description, RectTransform invoker)
    {
        if(this.optionsPane.AskBeforeOpeningLink() == true)
        {
            PxPre.UIL.Dialog dlg = 
                this.dlgSpawner.CreateDialogTemplate(
                    new Vector2(650.0f, 0.0f), 
                    PxPre.UIL.LFlag.AlignCenter,
                    0);

            dlg.AddDialogTemplateTitle("Open Link?", this.wiringPane.nodeicoLink);

            PxPre.UIL.UILStack uiStack = 
                new PxPre.UIL.UILStack(
                    this.uiFactory, 
                    dlg.rootParent,
                    dlg.contentSizer);

            uiStack.AddText($"Do you want to open the link?", true, 1.0f, PxPre.UIL.LFlag.Grow);
            //
            uiStack.PushImage(this.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.GrowHoriz).Chn_SetImgSliced();
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);
                uiStack.AddText(url, true, 1.0f, PxPre.UIL.LFlag.Grow).Chn_TextAlignment(TextAnchor.MiddleLeft);
            uiStack.Pop();
            uiStack.Pop();

            if(string.IsNullOrEmpty(description) == false)
            { 
                uiStack.AddSpace(20.0f, 0.0f, 0);

                uiStack.AddText("<b>Description:</b>", false, 0.0f, 0);
                uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.Grow);
                    uiStack.AddSpace(10.0f, 0.0f, 0);
                    uiStack.AddText(description, true, 1.0f, PxPre.UIL.LFlag.Grow);
                uiStack.Pop();
            }

            const float contentToggleSpace = 40.0f;            
            uiStack.AddVertSpace(contentToggleSpace, 0.0f, 0);

            uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
                PxPre.UIL.EleGenToggle<PushdownToggle> togAsk = uiStack.AddToggle<PushdownToggle>("", 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                uiStack.AddHorizSpace(10.0f, 0.0f, 0);
                uiStack.AddText("Always Ask Before Opening Links", true, 1.0f, PxPre.UIL.LFlag.GrowHoriz|PxPre.UIL.LFlag.AlignBot);
            uiStack.Pop();

            this.optionsPane.SetupNewPushdownToggle(togAsk.toggle);
            togAsk.toggle.isOn = true;
            togAsk.toggle.onValueChanged.AddListener(
                (x)=>
                { 
                    this.optionsPane.OnToggle_AskBeforeLink(x);
                });

            dlg.AddDialogTemplateSeparator();
            dlg.AddDialogTemplateButtons(
                new PxPre.UIL.DlgButtonPair[]
                { 
                    new PxPre.UIL.DlgButtonPair("Cancel", null),
                    new PxPre.UIL.DlgButtonPair(
                        "Open Link", 
                        (x)=>
                        { 
                            UnityEngine.Application.OpenURL(url); 
                            return true;
                        })
                });

            dlg.host.LayoutInRT(false);

            if(invoker != null)
                this.SetupDialogForTransition(dlg, invoker);
        }
        else
            UnityEngine.Application.OpenURL(url);
    }

    public bool IsDirty()
    { 
        return this.undoMgr.IsDirty();
    }

    public void Undo()
    { 
        bool isDirty = this.undoMgr.IsDirty();

        if(this.undoMgr.HasUndos() == false)
            return;

        string uname = this.undoMgr.GetTopUndoName();
        if(this.undoMgr.Undo() == true)
        { 
            foreach(IAppEventSink iaes in this.eventSinks)
                iaes.OnUndoRedo(uname, true);
        }

        if(isDirty != this.undoMgr.IsDirty())
            this.FlagAppStateDirty();
    }

    public void Redo()
    {
        bool isDirty = this.undoMgr.IsDirty();

        if(this.undoMgr.HasRedos() == false)
            return;

        string rname = this.undoMgr.GetTopRedoName();
        if(this.undoMgr.Redo() == true)
        { 
            foreach(IAppEventSink iaes in this.eventSinks)
                iaes.OnUndoRedo(rname, false);
        }

        if(isDirty != this.undoMgr.IsDirty())
            this.FlagAppStateDirty();
    }

    Coroutine appStateDirtCorout = null;

    public void FlagAppStateDirty()
    { 
        if(this.appStateDirtCorout != null)
            return;

        this.appStateDirtCorout = 
            this.StartCoroutine(this.AppStateDirtyCoroutine());
    }

    IEnumerator AppStateDirtyCoroutine()
    { 
        yield return new WaitForEndOfFrame();

        this.wiringPane.UpdateUndosButtons(this.undoMgr);
        this.RemakeTitlebar();
        this.appStateDirtCorout = null;
    }

    public void RenameWiring(WiringDocument wd, string newName)
    { 
        string curProcName = wd.GetProcessedWiringName(this.Wirings);
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, $"Renamed {curProcName}") )
        { 
            _RenameWiring(wd, newName, uds.GetUndoGroup());
        }
    }

    bool IKeyboardApp.RenameWiring(WiringCollection collection, WiringDocument wd, string name)
    { 
        if(collection != this.wirings)
            return false;

        this._RenameWiring(wd, name, null);

        return true;
    }

    public bool _RenameWiring(WiringDocument wd, string newName, PxPre.Undo.UndoGroup insUndo)
    { 
        string oldName = wd.GetName();
        wd.SetName(newName);

        if(wd.GetName() == oldName)
            return false;

        //if(setDirty == true)
        //    this.Wirings.SetDirty();

        foreach(IAppEventSink es in this.eventSinks)
            es.OnWiringRenamed(this.Wirings, wd);

        if(insUndo != null)
        { 
            insUndo.AddUndo( 
                new Undo_Rename(
                    true, 
                    this, 
                    this.Wirings, 
                    wd, 
                    oldName));
        }

        this.FlagAppStateDirty();

        return true;
    }

    bool IKeyboardApp.SetLLDAWParamValue(WiringCollection collection, WiringDocument wd, LLDNBase owner, ParamBase parameter, string newStringValue)
    {
        if(collection != this.wirings)
            return false;

        return this._SetLLDAWParamValue( 
            wd, 
            owner, 
            parameter, 
            newStringValue, 
            null);
    }

    public bool SetLLDAWParamValue(LLDNBase owner, ParamBase parameter, string oldValue, string newStringValue)
    { 

        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, $"Modified {parameter.name}") )
        { 
            return this._SetLLDAWParamValue(
                this.Wirings.Active, 
                owner, 
                parameter, 
                oldValue, 
                newStringValue, 
                uds.GetUndoGroup());
        }
    }

    bool _SetLLDAWParamValue(WiringDocument wd, LLDNBase owner, ParamBase parameter, string newStringValue, PxPre.Undo.UndoGroup insUndo)
    { 
        string oldValue = parameter.GetStringValue();
        return this._SetLLDAWParamValue(wd, owner, parameter, oldValue, newStringValue, insUndo);
    }

    bool _SetLLDAWParamValue(WiringDocument wd, LLDNBase owner, ParamBase parameter, string oldStringValue, string newStringValue, PxPre.Undo.UndoGroup insUndo)
    {
        if(parameter.SetValueFromString(newStringValue) == false)
            return false;

        foreach(IAppEventSink es in this.eventSinks)
            es.OnWiringParamValueChanged(this.Wirings, wd, owner, parameter);

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_SetParamValue(
                    true,
                    this,
                    this.Wirings,
                    wd,
                    owner,
                    parameter,
                    oldStringValue));
        }

        this.FlagAppStateDirty();

        return true;
    }

    void IKeyboardApp.AddWiringDocument(WiringCollection collection, WiringDocument wd)
    { 
        if(this.Wirings != collection)
            return;

        this._AddWiringDocument(wd, null);
    }

    public void AddWiringDocument(WiringDocument wd, bool setActive = true)
    { 
        using( PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Added New Wiring"))
        {
            WiringDocument wdOldSel = this.Wirings.Active;

            _AddWiringDocument(wd, uds.GetUndoGroup());
            
            if(setActive == true)
            {
                this.SetActiveDocument(wd, false);

                uds.GetUndoGroup().AddUndo( 
                    new Undo_ActiveWiring(
                        true,
                        this,
                        this.Wirings,
                        wd,
                        wdOldSel));
            }
        }
    }

    void _AddWiringDocument(WiringDocument wd, PxPre.Undo.UndoGroup insUndo)
    {
        this.Wirings.AddDocument(wd);

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_AddWiring(
                    true, 
                    this, 
                    this.Wirings, 
                    wd));
        }

        foreach(IAppEventSink es in this.eventSinks)
            es.OnWiringAdded(this.Wirings, wd);

        this.FlagAppStateDirty();
    }

    bool IKeyboardApp.RewireNodeAudio(
        WiringCollection collection, 
        WiringDocument wd, 
        LLDNBase output, 
        LLDNBase input, 
        ParamConnection parameter)
    { 
        if(this.Wirings != collection)
            return false;

        return this._RewireNodeAudio(wd, output, input, parameter, null);
    }

    public bool RewireNodeAudio(
        WiringDocument doc,
        LLDNBase output,
        LLDNBase input,
        ParamConnection parameter)
    { 
        using( PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, $"Modified Socket {parameter.name}"))
        {
            return this._RewireNodeAudio(
                this.Wirings.Active,
                output,
                input,
                parameter,
                uds.GetUndoGroup());
        }
    }

    bool _RewireNodeAudio(
        WiringDocument doc, 
        LLDNBase output, 
        LLDNBase input, 
        ParamConnection connection, 
        PxPre.Undo.UndoGroup insUndo)
    { 
        if(connection.Reference == output)
            return false;

        LLDNBase oldOutput = connection.Reference;

        // There's there's something already there, clear is out first with
        // a recursive call.
        if(connection.IsConnected() == true && output != null)
            _RewireNodeAudio(doc, null, input, connection, insUndo);

        // We trust whatever called us already check for cyclic dependencies or is in
        // a safe state.
        connection.SetReference(output);

        foreach(IAppEventSink es in this.eventSinks)
            es.OnWiringLinkChanged(this.Wirings, doc, output, input, connection);

        if(insUndo != null)
        {
            insUndo.AddUndo(
                new Undo_ConnectAudio(
                    true,
                    this,
                    this.Wirings,
                    doc,
                    oldOutput,
                    input,
                    connection));
        }

        this.RefreshActiveOutput();

        this.FlagAppStateDirty();
        return true;
    }

    bool IKeyboardApp.SetWiringCategory(
        WiringCollection collection, 
        WiringDocument wd, 
        WiringDocument.Category category)
    { 
        if(this.Wirings != collection)
            return false;

        return this._SetWiringCategory(wd, category, null);
    }

    public bool SetWiringCategory(
        WiringDocument wd, 
        WiringDocument.Category category)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, $"Changed Wiring Category"))
        { 
            return 
                this._SetWiringCategory(
                    wd, 
                    category, 
                    uds.GetUndoGroup());
        }
    }

    bool _SetWiringCategory(
         WiringDocument wd,
         WiringDocument.Category category,
         PxPre.Undo.UndoGroup insUndo)
    { 
        if(wd.category == category)
            return false;

        WiringDocument.Category old = wd.category;

        wd.category = category;

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_SetCategory(
                    true,
                    this,
                    this.Wirings,
                    this.Wirings.Active,
                    old));
        }

        this.FlagAppStateDirty();

        return true;
    }

    bool IKeyboardApp.InsertWirings(WiringCollection collection, params WiringIndexPair [] docs)
    { 
        if(this.Wirings != collection)
            return false;

        this._InsertWirings(docs, null);

        return false;
    }

    bool _InsertWirings(WiringIndexPair [] docs, PxPre.Undo.UndoGroup insUndo)
    {
        List<WiringDocument> added = new List<WiringDocument>();

        foreach(WiringIndexPair wip in docs)
        {
            if(this.Wirings.InsertDocument(wip.doc, wip.idx) == true)
                added.Add(wip.doc);
        }

        if(added.Count == 0)
            return false;

        if(insUndo != null)
        { 
            new Undo_AddWiring(true, this, this.Wirings, added.ToArray());
        }

        this.FlagAppStateDirty();

        return true;
    }    

    public bool InsertWirings(params WiringIndexPair [] docs)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Inserted Wiring(s)" ))
        {
            return this._InsertWirings(docs, uds.GetUndoGroup());
        }
    }

    bool IKeyboardApp.DeleteWirings(WiringCollection collection, params WiringDocument [] docs)
    { 
        return _DeleteWirings(docs, null);
    }

    bool _DeleteWirings(
        WiringDocument [] docs, 
        PxPre.Undo.UndoGroup insUndo)
    {
        // TODO: If the active is in any of the docs, set an active
        WiringDocument wdOld = this.Wirings.Active;

        List<WiringIndexPair> wips = new List<WiringIndexPair>();
        for(int i = 0; i < docs.Length; ++i)
        { 
            int idx = this.Wirings.GetWiringIndex(docs[i]);
            if(idx == -1)
                continue;

            wips.Add(new WiringIndexPair(docs[i], idx));
        }

        if(wips.Count == 0)
            return false;

        // We'll use wips because we already have it and it's gone through
        // the deletion process
        foreach(WiringIndexPair wip in wips)
        {
            if(this.Wirings.RemoveDocument(wip.doc) == false)
                continue;

            foreach(IAppEventSink iaes in this.eventSinks)
                iaes.OnWiringDeleted( this.Wirings, wip.doc);
        }

        if(this.Wirings.Count() == 0)
        { 
            WiringDocument wd = WiringCollection.CreateBlankWiringDocument("Default");
            this._AddWiringDocument(wd, insUndo);
        }

        if(this.Wirings.Active == null)
            this.Wirings.EnsureActive();

        if(this.Wirings.Active != wdOld)
        {
            this.SetActiveDocument(this.Wirings.Active, true);

            if(insUndo != null)
            {
                insUndo.AddUndo(
                    new Undo_ActiveWiring(
                        true,
                        this,
                        this.Wirings,
                        this.Wirings.Active,
                        wdOld));
            }
        }

        if(insUndo != null)
        { 
            // Delete goes last because there are other undos in this function
            // that might get mucked up on undo/redo if they're done too early.
            insUndo.AddUndo(
                new Undo_DeleteWiring(
                    true, 
                    this, 
                    this.Wirings, 
                    wips));
        }

        this.FlagAppStateDirty();

        return true;
    }

    bool DeleteWirings(params WiringDocument [] docs)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Deleted Wiring(s)"))
        { 
            return 
                this._DeleteWirings(
                    docs,
                    uds.GetUndoGroup());
        }
    }

    bool IKeyboardApp.SetActiveDocument(WiringCollection collection, WiringDocument active)
    { 
        if(this.Wirings != collection)
            return false;

        if(collection.ContainsDocument(active) == false)
            return false;

        return this.SetActiveDocument(active, false);
    }

    bool IKeyboardApp.SetWiringIndex(WiringCollection collection, WiringDocument wd, int index)
    { 
        if(this.Wirings != collection)
            return false;

        return this._SetWiringIndex(wd, index, null);
    }

    public bool _SetWiringIndex(WiringDocument wd, int index, PxPre.Undo.UndoGroup insUndo)
    { 
        int oldIdx = this.Wirings.GetWiringIndex(wd);
        if(oldIdx == index)
            return false;

        if(this.Wirings.RepositionIndex(oldIdx, index) == false)
            return false;

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_ReindexWiring(
                    true, 
                    this, 
                    this.Wirings, 
                    wd, 
                    oldIdx));
        }

        this.FlagAppStateDirty();

        return true;
    }

    public bool SetWiringIndex(WiringDocument wd, int index)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Move Wiring Ordering"))
        { 
            return this._SetWiringIndex(wd, index, uds.GetUndoGroup());
        }
    }

    public bool MoveWiringOffset(WiringDocument wd, int offset)
    { 
        if(offset == 0)
            return false;

        int origIdx = this.Wirings.GetWiringIndex(wd);
        if(origIdx == -1)
            return false;

        return this.SetWiringIndex(wd, origIdx + 1);
    }

    public bool SortWiringsByName()
    {
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Sort By Name"))
        {
            List<WiringDocument> curDocs = this.Wirings.OrganizedByName();
            return this._ResetWirings(this.Wirings, curDocs, uds.GetUndoGroup());
        }
    }

    public bool SortWiringsByCategory()
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Sort By Category"))
        {
            List<WiringDocument> sortedDocs = this.Wirings.OrganizedByCategory();
            return this._ResetWirings(this.Wirings, sortedDocs, uds.GetUndoGroup());
        }
    }

    bool IKeyboardApp.ResetWirings(WiringCollection collection, IEnumerable<WiringDocument> newWirings)
    { 
        if(this.Wirings != collection)
            return false;

        return this._ResetWirings(collection, newWirings, null);
    }

    bool _ResetWirings(WiringCollection collection, IEnumerable<WiringDocument> newWirings, PxPre.Undo.UndoGroup insUndo)
    { 
        List<WiringDocument> curDocs = collection.GetDocumentsListCopy();

        if(System.Linq.Enumerable.SequenceEqual<WiringDocument>(curDocs, newWirings) == true)
                return false;

        collection.Reset(newWirings);

        if(insUndo != null)
            insUndo.AddUndo( new Undo_ResetWirings(true, this, this.Wirings, curDocs));

        this.FlagAppStateDirty();

        return true;
    }

    bool IKeyboardApp.AddWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node)
    { 
        if(this.Wirings != collection)
            return false;

        return this._AddWiringNode(wd, node, node.cachedUILocation, null);
    }

    public bool AddWiringNode(WiringDocument doc, LLDNBase lld, Vector2 position)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Added Node"))
        {
            return this._AddWiringNode(doc, lld, position, uds.GetUndoGroup());
        }
    }

    bool _AddWiringNode(WiringDocument doc, LLDNBase lld, Vector2 position, PxPre.Undo.UndoGroup insUndo)
    {
        lld.cachedUILocation = position;
        doc.AddGenerator(lld);

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_AddNode(
                    true,
                    this,
                    this.Wirings,
                    doc,
                    lld));

        }

        foreach(IAppEventSink ies in this.eventSinks)
            ies.OnWiringNodeAdded(this.Wirings, doc, lld);

        this.FlagAppStateDirty();
        return true;
    }

    bool IKeyboardApp.DeleteWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node)
    { 
        if(this.Wirings != collection)
            return false;

        return this._DeleteWiringNode(wd, node, null);
    }

    public bool DeleteWiringNode(WiringDocument doc, LLDNBase  lld)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Deleted Node"))
        {
            return this._DeleteWiringNode(doc, lld, uds.GetUndoGroup());
        }
    }

    bool _DeleteWiringNode(WiringDocument doc, LLDNBase lld, PxPre.Undo.UndoGroup insUndo)
    {
        // Remove all connections is has
        foreach(ParamConnection pc in lld.GetParamConnections())
        { 
            if(pc.IsConnected() == true)
                _RewireNodeAudio(doc, null, lld, pc, insUndo);
        }

        // Remove all stuff connected to it
        foreach(LLDNBase gbIt in doc.Generators)
        {
            if(gbIt == lld)
                continue;

            foreach(ParamConnection pc in gbIt.GetParamConnections())
            { 
                if(pc.Reference == lld)
                    _RewireNodeAudio(doc, null, gbIt, pc, insUndo);
            }
        }

        doc.RemoveNode(lld, true);

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_DeleteNode(
                    true,
                    this,
                    this.Wirings,
                    doc,
                    lld));

        }

        foreach(IAppEventSink ies in this.eventSinks)
            ies.OnWiringNodeDeleted(this.Wirings, doc, lld);

        this.FlagAppStateDirty();
        return true;
    }

    bool IKeyboardApp.MoveWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node, Vector2 position)
    { 
        if(this.Wirings != collection)
            return false;

        return this._MoveWiringNode(wd, node, position, null);
    }

    public bool MoveWiringNode(WiringCollection collection, WiringDocument wd, LLDNBase node, Vector2 position)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Moved Node"))
        {
            return this._MoveWiringNode(wd, node, position, uds.GetUndoGroup());
        }
    }

    bool _MoveWiringNode(WiringDocument wd, LLDNBase node, Vector2 position, PxPre.Undo.UndoGroup insUndo)
    { 
        Vector2 origPos = node.cachedUILocation;

        node.cachedUILocation = position;

        foreach(IAppEventSink iaes in this.eventSinks)
            iaes.OnWiringNodeMoved(this.Wirings, wd, node);

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_MoveNode(
                    true, 
                    this,
                    this.Wirings,
                    wd,
                    node,
                    origPos));
        }

        this.FlagAppStateDirty();

        return true;
    }

    bool IKeyboardApp.ShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 offset)
    { 
        if(collection != this.Wirings)
            return false;

        this._ShiftDocument(collection, wd, offset, null);

        return true;
    }

    bool _ShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 offset, PxPre.Undo.UndoGroup insUndo)
    { 
        foreach(LLDNBase gnb in wd.Generators)
            gnb.cachedUILocation += offset;

        foreach(IAppEventSink iaes in this.eventSinks)
            iaes.OnShiftDocument(collection, wd, offset);

        if(insUndo != null)
        { 
            insUndo.AddUndo(
                new Undo_ShiftDocument(
                    true,
                    this,
                    collection,
                    wd,
                    offset));
        }

        this.FlagAppStateDirty();
        return true;
    }

    public bool ShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 offset)
    { 
        using(PxPre.Undo.UndoDropSession uds = new PxPre.Undo.UndoDropSession(this.undoMgr, "Redimensioned Wiring"))
        { 
            return this._ShiftDocument(collection, wd, offset, uds.GetUndoGroup());
        }
    }

    public PxPre.Undo.UndoDropSession CreateAppDropSession(string undoAction)
    { 
        return new PxPre.Undo.UndoDropSession(this.undoMgr, undoAction);
    }

    public bool IsInUndoSession()
    {
        return 
            PxPre.Undo.UndoDropSession.HasActiveUndoSession(this.undoMgr);
    }

    const int C4MIDI_ID = 60;

    void IMIDIMsgSink.MIDIMsg_OnSystemMsg(MIDIMgr mgr, MIDISystemMsg msg)
    { 
        if(msg == MIDISystemMsg.SystemReset)
            this.EStop();
    }

    void IMIDIMsgSink.MIDIMsg_OnNoteOff(int channel, int keyNum, float velocity)
    { 
        int phonKey = MIDINoteToPhonicsNote(keyNum);

        int playingId;
        if(this.midiPlayingNotes.TryGetValue(phonKey, out playingId) == true)
        { 
            this.EndNote(playingId);
            this.midiPlayingNotes.Remove(phonKey);
        }
    }

    void IMIDIMsgSink.MIDIMsg_OnNoteOn(int channel, int keyNum, float velocity)
    { 
        int phonKey = MIDINoteToPhonicsNote(keyNum);

        int playingId;
        if(this.midiPlayingNotes.TryGetValue(phonKey, out playingId) == true)
        { 
            this.EndNote(playingId);
            this.midiPlayingNotes.Remove(phonKey);
        }

        float fr = PxPre.Phonics.WesternFreqUtils.GetFrequency(phonKey);
        PxPre.Phonics.GenBase gb = this.wiringPane.GenerateForFrequency(fr);
        playingId = this.StartNote(gb, phonKey, velocity, Application.NoteStartEvent.Midi);

        this.midiPlayingNotes[phonKey] = playingId;
    }

    void IMIDIMsgSink.MIDIMsg_OnKeyPressure(int channel, int keyNum, float velocity)
    { 
        int phonKey = MIDINoteToPhonicsNote(keyNum);
    }

    void IMIDIMsgSink.MIDIMsg_OnControlChange(int channel, int controllerNum, int controllerValue)
    { 
    }

    void IMIDIMsgSink.MIDIMsg_OnProgramChange(int channel, int program)
    { 
        this.SetActiveDocument(program, true, false);
    }

    void IMIDIMsgSink.MIDIMsg_OnChannelPressure(float pressure)
    { 
        this.SetMasterVolume(pressure);
    }

    void IMIDIMsgSink.MIDIMsg_OnPitchBend(float msb, float lsb)
    { 
    }

    static int MIDINoteToPhonicsNote(int midiNote)
    { 
        int phonC4 = PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 4);
        int phonKey = midiNote - C4MIDI_ID + phonC4;

        return phonKey;
    }

}
