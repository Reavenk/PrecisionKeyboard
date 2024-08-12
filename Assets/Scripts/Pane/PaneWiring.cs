// <copyright file="PaneWiring.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The wiring pane where the piano instrument can be 
// programmed and selected.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using PxPre.Phonics;

public class PaneWiring: 
    PaneBase,
    IGNParamUICreator,
    IWiringEditorBridge
{
    const float paramReferenceUIHeightExtra = 20.0f;

    enum TrashIcon
    { 
        Button,
        DragZoneInactive,
        DragZoneReady
    }

    [System.Serializable]
    public struct CategorySpritePair
    {
        public WiringDocument.Category category;
        public Sprite sprite;
    }

    [System.Flags]
    enum Dirty
    { 
        DocumentSize        = 1 << 0,
        NodeOutputStyle     = 1 << 1
    }

    
    /// <summary>
    /// Tracking information for all the WireReference node's UI element
    /// that are currently being shown in the canvas.
    /// </summary>
    class WireReferenceEntry
    { 
        public LLDNBase node;
        public ParamWireReference param;
        public RectTransform pulldownRect;
        public UnityEngine.UI.Text entryText;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct NodePlate
    {
        public UnityEngine.UI.Image plate;
        public UnityEngine.UI.Image icon;
        public UnityEngine.UI.Text text;
        public UnityEngine.UI.Button button;
    }

    public class ThingToHighlight
    { 
        public enum Type
        { 
            NodePlate,
            NodeOutput,
            NodeParam,
            WiringDoc
        }

        public readonly Type type;
        public readonly ParamBase param = null;
        public readonly LLDNBase lldaw = null;
        public readonly WiringDocument doc = null;


        private ThingToHighlight(Type type, WiringDocument doc, LLDNBase lldaw, ParamBase param)
        { 
            this.doc = doc;
            this.type = type;
            this.param = param;
            this.lldaw = lldaw;
        }

        public static ThingToHighlight CreateHLParameter(WiringDocument doc, LLDNBase lldaw, ParamBase param)
        { 
            return new ThingToHighlight(Type.NodeParam, doc, lldaw, param);
        }

        public static ThingToHighlight CreateHLNodeOutput(WiringDocument doc, LLDNBase lldaw)
        { 
            return new ThingToHighlight(Type.NodeOutput, doc, lldaw, null);
        }

        public static ThingToHighlight CreateHLNodePlate(WiringDocument doc, LLDNBase lldaw)
        { 
            return new ThingToHighlight(Type.NodePlate, doc, lldaw, null);
        }

        public static ThingToHighlight CreateHLNodePlate(WiringDocument doc)
        { 
            return new ThingToHighlight(Type.WiringDoc, null, null, null);
        }
    }

    public class CachedWiringViewSettings
    { 
        public Vector2 scroll;
        public float zoom;

        public CachedWiringViewSettings(Vector2 scroll, float zoom)
        { 
            this.scroll = scroll;
            this.zoom = zoom;
        }
    }

    /// <summary>
    /// The current type of dragging occuring.
    /// </summary>
    public enum DragMode
    { 
        /// <summary>
        /// Dragging is not occuring.
        /// </summary>
        None,

        /// <summary>
        /// A factory plate is being dragged. If dropped onto the wiring
        /// canvas, a new node will be created on the drop location.
        /// </summary>
        FactoryNode,

        /// <summary>
        /// A GNUINode is being dragged.
        /// </summary>
        WiringNode,

        /// <summary>
        /// The socket input is being dragged. It's expected that the 
        /// drop will happen on a socket output.
        /// </summary>
        SocketInput,

        /// <summary>
        /// The socket output is being dragged. It's expeted that the
        /// drop will happen on a socket input.
        /// </summary>
        SocketOutput
    }

    // This is both the space we add extra below the 
    // factory plates so they can overscroll and have the last
    // factory plate not be right next to the delete button
    // AND the the extra fudge factor when scrolling to an item
    // so that it's not on the edge, for aesthetic reasons.
    const float scrollBuffer            = 50.0f;

    ////////////////////////////////////////////////////////////////////////////////
    
    public Sprite nodeicoAbs;           // The node icon for the absolute value node.
    public Sprite nodeicoConst;         // The node icon for the constant value waveform node.
    public Sprite nodeicoNeg;           // The node icon for the negated value node.
    public Sprite nodeicoInv;           // The node icon for the inverted value node.
    public Sprite nodeicoLerp;          // The node icon for the linear interpolation node.
    public Sprite nodeicoQuant;         // The node icon for the quantize node.
    public Sprite nodeicoSign;          // The node icon for the positive/negative sign node.
    public Sprite nodeicoOutput;        // The node icon for the output node.
    public Sprite nodeicoAttack;        // The node icon for the attack node.
    public Sprite nodeicoCombAdd;       // The node icon for the addition node.
    public Sprite nodeicoCombMod;       // The node icon for the multiply node.
    public Sprite nodeicoCombSub;       // The node icon for the subtraction node.
    public Sprite nodeicoADSR;          // The node icon for the ADSR envelope node.
    public Sprite nodeicoDecay;         // The node icon for the envelope decay node.
    public Sprite nodeicoRelease;       // The node icon for the envelope release node.
    public Sprite nodeicoDelay;         // The node icon for the delay node.
    public Sprite nodeicoChorus;        // The node icon for the chorus node.
    public Sprite nodeicoGenNoise;      // The node icon for the white noise node.
    public Sprite nodeicoGenSaw;        // The node icon for the saw wave node.
    public Sprite nodeicoGenSine;       // The node icon for the sine wave node.
    public Sprite nodeicoGenTri;        // The node icon for the triange wave node.
    public Sprite nodeicoGenSquare;     // The node icon for the square wave node.
    public Sprite nodeicoGate;          // The node icon for the gate signal node.
    public Sprite nodeicoGateWave;      // The node icon for the 
    public Sprite nodeicoMin;           // The node icon for the minimize node.
    public Sprite nodeicoMax;           // The node icon for the maximize node.
    public Sprite nodeicoAmplify;       // The node icon for the multiply (by constant) node.
    public Sprite nodeicoMad;           // The node icon for the multiple and add (by constants) node.
    public Sprite nodeicoCube;          // The node icon for the cube power node.
    public Sprite nodeicoSquare;        // The node icon for the square power node.
    public Sprite nodeicoReference;     // The node icon for referencing another wiring.
    public Sprite nodeicoComment;       // The node icon for the comment nodes.
    public Sprite nodeicoGateList;      // The node icon for the utility gate list.
    public Sprite nodeicoPowerAmplify;
    public Sprite nodeicoStutter;
    public Sprite nodeicoHold;
    public Sprite nodeicoClamp;
    public Sprite nodeicoSmear;
    public Sprite nodeIcoCycle;
    public Sprite nodeIcoQuickOut;
    public Sprite nodeIcoHightlight;
    public Sprite nodeicoVideo;
    public Sprite nodeicoLink;

    public Sprite factoryPlate;
    public Sprite hollowFactoryPlate;

    public Sprite paramContainerSprite;
    public Sprite paramPlateSprite;

    public Sprite categoryDecorationPlate;

    public Sprite socketArrowSprite;

    public UnityEngine.UI.ScrollRect dragFactoryRegion;
    public WiringScrollRect wiringRegion;
    public UnityEngine.UI.Image garbageDragRegion;

    /// <summary>
    /// 
    /// </summary>
    public float dragPlateHeight        = 50.0f;

    /// <summary>
    /// 
    /// </summary>
    public float dragPlatePadIconLeft   = 10.0f;

    /// <summary>
    /// 
    /// </summary>
    public float dragPlatePadIconRight  = 10.0f;

    /// <summary>
    /// 
    /// </summary>
    public int dragFontSize             = 20;

    /// <summary>
    /// 
    /// </summary>
    public Color dragFontColor          = Color.black;

    /// <summary>
    /// 
    /// </summary>
    public float dragPlateSpaces        = 10.0f;

    /// <summary>
    /// 
    /// </summary>
    public Font dragLabelFont;

    /// <summary>
    /// 
    /// </summary>
    private UnityEngine.UI.Image curDragIcon = null;

    /// <summary>
    /// 
    /// </summary>
    private LLDNBase.NodeType curDragType = LLDNBase.NodeType.Void;

    /// <summary>
    /// A mapping of all the UI hosts for the nodes.
    /// </summary>
    Dictionary<LLDNBase, GNUIHost> uiNodes = 
        new Dictionary<LLDNBase, GNUIHost>();

    // A hack for editor scripts
    public IEnumerable<GNUIHost> _uiNodesForEditor 
    {get{return this.uiNodes.Values; } }

    /// <summary>
    /// All the graphics of curves, as well as useful cached topology 
    /// data about what the parameter connections, both ways
    /// - Input to output
    /// - Output to input
    /// </summary>
    List<HermiteLink> linkCurves = 
        new List<HermiteLink>();

    /// <summary>
    /// 
    /// </summary>
    public RectTransform documentPulldownRect;
    public UnityEngine.UI.Text pulldownDocumentText;

    public Sprite sliderSprite;
    public Sprite sliderThumbSprite;

    WiringDocument viewingDocument = null;
    GNUIHost selectedNode = null;
    public GNUIHost GetSelectedNode(){return this.selectedNode; }
    Dictionary<LLDNBase, List<WireReferenceEntry>> wireReferencesUIData = 
        new Dictionary<LLDNBase, List<WireReferenceEntry>>();

    // Tracking if we reselect the previously selected node WITHOUT
    // dragging, bring up help.
    // If all things line up with this varaible, when they double tap (without dragging)
    // the factory plate will be scrolled to.
    GNUIHost potentialFactoryScrollTo = null;

    private DragMode drag = DragMode.None;
    private SocketUIInput draggingInput = null;
    private SocketUIOutput draggingOutput = null;
    private HermiteUICurve previewCurve = null;
    private SocketUIInput lastInputHover = null;
    private SocketUIOutput lastOutputHover = null;
    private GNUIHost draggedHost = null;

    // If non-null, the current drag operation of an output was initiated by
    // dragging it from the input (the other end of the curve)
    LLDNBase previousInputDragHost = null;
    ParamConnection previousInputDraggedDC = null;
    // If true, the current drag operation involved disconnecting a curve
    // (from the input).
    bool dcInputOnDrag = false;

    public UnityEngine.UI.Image eStop;  // Location of the eStop button graphic

    /// <summary>
    ///  The amount of client spaceing to place around the visual
    ///  representation of a document.
    /// </summary>
    public float documentPadding = 200.0f;
    private float docXMax = 0.0f;
    private float docYMin = 0.0f;
    private bool documentDimDirty = false;

    public GameObject paramThumbToggle;     // The prefab for the thumb icon of toggle params.
    public GameObject paramThumbRoller;     // The prefab for the thumb icon of roller widgets
    public GameObject paramThumbDial;       // The prefab for the thumb icon of dial widgets
    public GameObject paramThumbSlider;
    public GameObject paramThumbFreq;
    public GameObject paramThumbDur;
    public GameObject paramThumbNickname;
    public GameObject paramThumbWiringReference;

    public GameObject paramEditRoller;
    public GameObject paramEditDial;
    public GameObject paramEditSlider;
    public GameObject paramEditReference;
    public GameObject paramEditNick;

    public GameObject paramEditRoot;

    /// <summary>
    /// When dragging a wiring node and for some reason (usually if the node
    /// is dragged out of bounds).
    /// </summary>
    const float trashAreaHeight = 100.0f;
    Vector2 originalGNUIPredragPos = Vector2.zero;
    UnityEngine.UI.Image wiredDragOutBoundsIcon = null;
    Vector2 trashIconAnchor;
    public UnityEngine.UI.Image trashIcon;
    public UnityEngine.UI.Image deleteNodeImg;
    public PushdownButton deleteNoteButton;
    public UnityEngine.UI.Text deleteButtonCaption;

    public Sprite menuSelIcon;
    public RectTransform menuSelContainer;

    public List<GNUIHost> dialatedOutputs = new List<GNUIHost>();
    public List<GNUIHost> dialatedInputs = new List<GNUIHost>();

    public float wiringZoom = 1.0f;         // The current zoom value for the wiring canvas.
    public const float minZoom = 0.5f;      // The minimum zoom allowed for the wiring canvas.
    public const float maxZoom = 2.0f;      // The maximum zoom allowed for the wiring canvas.

    public RectTransform factoryPalletRegion;

    public Coroutine processingOutputStyles = null;

    public Vector2 selectionHaloRad = new Vector2(10.0f, 10.0f);
    public Sprite selectionHaloSprite;
    RectTransform selectionHalo = null;

    /// <summary>
    /// RectTransform of the crosshair graphic used to show the 
    /// target of socket drag and dropping
    /// </summary>
    RectTransform crosshair;

    public RectTransform alignNamePulldown;
    public RectTransform alignAddWire;
    public RectTransform alignWebDownload;
    public RectTransform alignWebUpload;
    public RectTransform alignOptions;

    public RectTransform keyboardTestArea;

    // 
    //      VARIOUS PULLDOWN SPRITES
    //////////////////////////////////////////////////

    public Sprite pulldownClone;        // The pulldown icon to clone a wiring document
    public Sprite pulldownRename;       // The pulldown icon to rename a wiring document.

    public CategorySpritePair[] categoryIcons;
    public Sprite unlabledIconCat;
    Dictionary<WiringDocument.Category, Sprite> catIconLookups =
        new Dictionary<WiringDocument.Category, Sprite>();

    public Sprite icoAddNewWiring;
    public Sprite icoOpen;      // The open icon for the wiring document pulldown
    public Sprite icoOpenInternalDoc;
    public Sprite icoAppend;    // The open icon for the wiring document pulldown
    public Sprite icoSave;      // The save icon for the wiring document pulldown.
    public Sprite icoSaveAs;    // The save-as icon for the wiring document pulldown.
    public Sprite icoNewDoc;    // The clear icon for the wiring document pulldown.
    public Sprite icoDelCur;    // The delete icon for the wiring document pulldown.

    public Sprite icoOrganizeName;
    public Sprite icoOrganizeIcon;
    public Sprite icoOrganizeMoveUp;
    public Sprite icoOrganizeMoveDown;
    public Sprite icoOrganizeMoveTop;
    public Sprite icoOrganizeMoveBottom;

    UnityEngine.UI.Image imgPreviewPlate = null;

    Dictionary<WiringDocument, CachedWiringViewSettings> lastScroll = 
        new Dictionary<WiringDocument, CachedWiringViewSettings>();


    /// <summary>
    /// The undo button
    /// </summary>
    public UnityEngine.UI.Button buttonUndo;

    /// <summary>
    /// The redo button
    /// </summary>
    public UnityEngine.UI.Button buttonRedo;

    Dirty dirtyFlags = 0;
    Coroutine dirtyCoroutine = null;

    // After an undo operation, highlight the 
    IEnumerator undoHighlightAfterwardsCoroutine = null;
    ThingToHighlight thingToHighlight = null;

    void Update()
    { 
        if(Input.GetKeyDown(KeyCode.Escape) == true)
        {
            if(this.drag != DragMode.None)
            {
                if(this.dcInputOnDrag == true)
                {
                    // We got here by disconnecting an input that wasn't restored.
                    //this.App.FlagDirty();
                }

                if(this.draggedHost != null)
                {
                    this.draggedHost.slave.cachedUILocation = this.cachedGNUIDragPos;
                    this.draggedHost.UpdateFromBasePosition();

                    // Update any curves
                    this.UpdateGNUIHostFromDrag(this.draggedHost);
                }

                this.ClearDragState();
            }
        }
    }

    IEnumerator DirtyCoroutine()
    { 
        yield return new WaitForFixedUpdate();
        this.ProcessDirtyFlag();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if(this.dirtyFlags != 0)
            this.ProcessDirtyFlag();

        this.undoHighlightAfterwardsCoroutine = null;
        this.thingToHighlight = null;
    }

    void FlagDirty(Dirty flags)
    { 
        this.dirtyFlags |= flags;

        if(this.dirtyCoroutine == null && this.isActiveAndEnabled)
            this.dirtyCoroutine = this.StartCoroutine(this.DirtyCoroutine());
    }

    void ProcessDirtyFlag()
    { 
        if((this.dirtyFlags & Dirty.DocumentSize) != 0)
        { 
            this.UpdateDocumentSize();
        }

        if((this.dirtyFlags & Dirty.NodeOutputStyle) != 0)
        { 
            this.SetNodeOutputStyleDirty();
            this.App.RefreshActiveOutput();
        }

        this.dirtyFlags = 0;

        if(this.dirtyCoroutine != null)
        {
            this.StopCoroutine(this.dirtyCoroutine);
            this.dirtyCoroutine = null;
        }
    }

    public void RestoreDialations()
    { 
        foreach(GNUIHost gnui in this.dialatedOutputs)
        {
            // if it's selected, it should maintain a dialated output
            if(this.selectedNode == gnui)
                continue;

            gnui.UndialateOutputArrow();
        }

        foreach(GNUIHost gnui in this.dialatedInputs)
            gnui.UndialateInputArrows();

        this.dialatedOutputs.Clear();
        this.dialatedInputs.Clear();
    }

    public void ClearDragState(UnityEngine.EventSystems.PointerEventData data = null)
    {
        this.drag = DragMode.None;
        this.curDragType = LLDNBase.NodeType.Void;

        this.previousInputDragHost = null;
        this.previousInputDraggedDC = null;
        this.dcInputOnDrag = false;

        this.draggedHost = null;

        this.ShowDeleteButtonDroppable(TrashIcon.Button);
        this.wiredDragOutBoundsIcon.gameObject.SetActive(false);

        if (this.curDragIcon != null)
            this.curDragIcon.gameObject.SetActive(false);

        if (this.wiredDragOutBoundsIcon != null)
            this.wiredDragOutBoundsIcon.gameObject.SetActive(false);

        if(this.previewCurve != null)
            this.previewCurve.gameObject.SetActive(false);

        if (this.curDragIcon != null)
        {
            GameObject.Destroy(this.curDragIcon.gameObject);
            this.curDragIcon = null;
        }

        if (data != null)
        { 
            data.dragging = false;
            data.pointerDrag = null;
        }

        if(this.lastOutputHover != null)
            this.lastOutputHover.targetGraphic.color = Color.white;

        this.lastOutputHover = null;

        if(this.lastInputHover != null)
            this.lastInputHover.targetGraphic.color = Color.white;

        this.lastInputHover = null;

        if(this.imgPreviewPlate != null)
            this.imgPreviewPlate.gameObject.SetActive(false);

        this.ApplyCrosshair(null);

        this.RestoreDialations();
    }

    public NodeInfo GetNodeInfo(LLDNBase.NodeType nt)
    { 
        switch(nt)
        {
            case LLDNBase.NodeType.Abs:
                return new NodeInfo(LLDNBase.NodeType.Abs, "Abs Value", this.nodeicoAbs);

            case LLDNBase.NodeType.Amplify:
                return new NodeInfo(LLDNBase.NodeType.Amplify, "Amplify", this.nodeicoAmplify);

            case LLDNBase.NodeType.Constant:
                return new NodeInfo(LLDNBase.NodeType.Constant, "Constant", this.nodeicoConst);

            case LLDNBase.NodeType.Cube:
                return new NodeInfo(LLDNBase.NodeType.Cube, "Cube", this.nodeicoCube);

                case LLDNBase.NodeType.Cycle:
                return new NodeInfo(LLDNBase.NodeType.Cycle, "Cycle", this.nodeIcoCycle);

            case LLDNBase.NodeType.MAD:
                return new NodeInfo(LLDNBase.NodeType.MAD, "MulAdd", this.nodeicoMad);

            case LLDNBase.NodeType.Negate:
                return new NodeInfo(LLDNBase.NodeType.Negate, "Negate", this.nodeicoNeg);

            case LLDNBase.NodeType.Lerp:
                return new NodeInfo(LLDNBase.NodeType.Lerp, "Lerp", this.nodeicoLerp);

            case LLDNBase.NodeType.Quant:
                return new NodeInfo(LLDNBase.NodeType.Quant, "Quant", this.nodeicoQuant);

            case LLDNBase.NodeType.Sign:
                return new NodeInfo(LLDNBase.NodeType.Sign, "Sign", this.nodeicoSign);

            case LLDNBase.NodeType.Output:
                return new NodeInfo(LLDNBase.NodeType.Output, "Output", this.nodeicoOutput);

            case LLDNBase.NodeType.SineWave:
                return new NodeInfo(LLDNBase.NodeType.SineWave, "Sine Wave", this.nodeicoGenSine);

            case LLDNBase.NodeType.TriWave:
                return new NodeInfo(LLDNBase.NodeType.TriWave, "Triangle Wave", this.nodeicoGenTri);

            case LLDNBase.NodeType.SquareWave:
                return new NodeInfo(LLDNBase.NodeType.SquareWave, "Square Wave", this.nodeicoGenSquare);

            case LLDNBase.NodeType.SawWave:
                return new NodeInfo(LLDNBase.NodeType.SawWave, "Saw Wave", this.nodeicoGenSaw);

            case LLDNBase.NodeType.NoiseWave:
                return new NodeInfo(LLDNBase.NodeType.NoiseWave, "White Noise", this.nodeicoGenNoise);

            case LLDNBase.NodeType.CombAdd:
                return new NodeInfo(LLDNBase.NodeType.CombAdd, "Combine Add", this.nodeicoCombAdd);

            case LLDNBase.NodeType.CombSub:
                return new NodeInfo(LLDNBase.NodeType.CombSub, "Combine Sub", this.nodeicoCombSub);

            case LLDNBase.NodeType.CombMod:
                return new NodeInfo(LLDNBase.NodeType.CombMod, "Combine Mod", this.nodeicoCombMod);

            case LLDNBase.NodeType.Release:
                return new NodeInfo(LLDNBase.NodeType.Release, "Release", this.nodeicoRelease);

            case LLDNBase.NodeType.EnvADSR: 
                return new NodeInfo(LLDNBase.NodeType.EnvADSR, "ADSR", this.nodeicoADSR);

            case LLDNBase.NodeType.EnvAttack:
                return new NodeInfo(LLDNBase.NodeType.EnvAttack, "Attack", this.nodeicoAttack);

            case LLDNBase.NodeType.EnvDecay:
                return new NodeInfo(LLDNBase.NodeType.EnvDecay, "Decay", this.nodeicoDecay);

            case LLDNBase.NodeType.Gate:
                return new NodeInfo(LLDNBase.NodeType.Gate, "Gate", this.nodeicoGate);

            case LLDNBase.NodeType.GateWave:
                return new NodeInfo(LLDNBase.NodeType.GateWave, "Gate Wave", this.nodeicoGateWave);

            case LLDNBase.NodeType.Chorus:
                return new NodeInfo(LLDNBase.NodeType.Chorus, "Chorus", this.nodeicoChorus);

            case LLDNBase.NodeType.Delay:
                return new NodeInfo(LLDNBase.NodeType.Delay, "Delay", this.nodeicoDelay);

            case LLDNBase.NodeType.Min:
                return new NodeInfo(LLDNBase.NodeType.Min, "Min", this.nodeicoMin);

            case LLDNBase.NodeType.Max:
                return new NodeInfo(LLDNBase.NodeType.Max, "Max", this.nodeicoMax);

            case LLDNBase.NodeType.PowerAmplify:
                return new NodeInfo(LLDNBase.NodeType.PowerAmplify, "Power Amp", this.nodeicoPowerAmplify);

            case LLDNBase.NodeType.Square:
                return new NodeInfo(LLDNBase.NodeType.Square, "Square", this.nodeicoSquare);

            case LLDNBase.NodeType.Reference:
                return new NodeInfo(LLDNBase.NodeType.Reference, "Reference", this.nodeicoReference);

            case LLDNBase.NodeType.Comment:
                return new NodeInfo(LLDNBase.NodeType.Comment, "Comment", this.nodeicoComment);

#if !DEPLOYMENT || UNITY_EDITOR
            case LLDNBase.NodeType.GateList:
                return new NodeInfo(LLDNBase.NodeType.GateList, "Gate List", this.nodeicoGateList);
#endif

            case LLDNBase.NodeType.Invert:
                return new NodeInfo(LLDNBase.NodeType.Invert, "Invert", this.nodeicoInv);

            case LLDNBase.NodeType.Hold:
                return new NodeInfo(LLDNBase.NodeType.Hold, "Hold", this.nodeicoHold);

            case LLDNBase.NodeType.Smear:
                return new NodeInfo(LLDNBase.NodeType.Smear, "Smear", this.nodeicoSmear);

            case LLDNBase.NodeType.Clamp:
                return new NodeInfo(LLDNBase.NodeType.Clamp, "Clamp", this.nodeicoClamp);

            case LLDNBase.NodeType.Stutter:
                return new NodeInfo(LLDNBase.NodeType.Stutter, "Stutter", this.nodeicoStutter);

            case LLDNBase.NodeType.QuickOut:
                return new NodeInfo(LLDNBase.NodeType.QuickOut, "QuickOut", this.nodeIcoQuickOut);

            case LLDNBase.NodeType.Highlight:
                return new NodeInfo(LLDNBase.NodeType.Highlight, "Highlight", this.nodeIcoHightlight);

        }

        return new NodeInfo(LLDNBase.NodeType.Void, "", null);
    }

    

    public override void InitPane(Application app)
    {
        base.InitPane(app);

        this.categoryIcons = 
            this.categoryIcons.OrderBy( (x)=>{ return WiringDocument.GetCategoryName(x.category); }).ToArray();

        foreach(CategorySpritePair csp in this.categoryIcons)
            this.catIconLookups.Add(csp.category, csp.sprite);

        this.trashIconAnchor = this.trashIcon.rectTransform.anchoredPosition;
        this.HideTrashRegion();

        this.docXMax = 0.0f;
        this.docYMin = 0.0f;

        this.FillDragCreate();

        this.InitializeWiringDocumentBar();

        GameObject goPreviewCurve = new GameObject("PreviewCurve");
        goPreviewCurve.transform.SetParent(this.wiringRegion.content);
        goPreviewCurve.transform.localRotation = Quaternion.identity;
        goPreviewCurve.transform.localScale = Vector3.one;
        this.previewCurve = goPreviewCurve.AddComponent<HermiteUICurve>();
        this.previewCurve.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        this.previewCurve.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        this.previewCurve.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        this.previewCurve.rectTransform.anchoredPosition = Vector2.zero;
        this.previewCurve.rectTransform.sizeDelta = Vector2.zero;
        this.previewCurve.gameObject.SetActive(false);

        this.App.AddEStop(this.eStop, true);

        GameObject goDragIcon = new GameObject("DragIcon");
        goDragIcon.transform.SetParent(this.transform);
        goDragIcon.transform.localRotation = Quaternion.identity;
        goDragIcon.transform.localScale = Vector3.one;
        this.wiredDragOutBoundsIcon = goDragIcon.AddComponent<UnityEngine.UI.Image>();
        this.wiredDragOutBoundsIcon.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        this.wiredDragOutBoundsIcon.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        this.wiredDragOutBoundsIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        goDragIcon.gameObject.SetActive(false);
        this.FlagDirty(Dirty.NodeOutputStyle);

        GameObject goPrev = new GameObject("DragPreview");
        goPrev.SetActive(false);
        goPrev.transform.SetParent(this.wiringRegion.content, false);
        goPrev.transform.SetParent(this.wiringRegion.content, false);
        this.imgPreviewPlate = goPrev.AddComponent<UnityEngine.UI.Image>();
        RectTransform rtPreviewPlate = this.imgPreviewPlate.rectTransform;
        rtPreviewPlate.anchorMin = new Vector2(0.0f, 1.0f);
        rtPreviewPlate.anchorMax = new Vector2(0.0f, 1.0f);
        rtPreviewPlate.pivot = new Vector2(0.0f, 1.0f);
        rtPreviewPlate.sizeDelta = new Vector2(100.0f, 100.0f);
        this.imgPreviewPlate.type = Type.Sliced;
        this.imgPreviewPlate.fillCenter = false;
        this.imgPreviewPlate.sprite = this.hollowFactoryPlate;

        this.wiringRegion.controller = this;
    }

    public Sprite GetCategoryIcon(WiringDocument.Category cat)
    { 
        Sprite ret;
        if(this.catIconLookups.TryGetValue(cat, out ret) == true)
            return ret;

        return this.unlabledIconCat;
    }

    void InitializeWiringDocumentBar()
    {
        // things to align from right to left;
        List<RectTransform> buttonsToAlign = new List<RectTransform>();
        HashSet<RectTransform> allButtons = 
            new HashSet<RectTransform>
            {
                this.alignWebUpload,
                this.alignWebDownload,
                this.alignOptions,
                this.alignAddWire
            };

        if(UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
        {
            buttonsToAlign.Add(this.alignWebUpload);
            buttonsToAlign.Add(this.alignWebDownload);
            buttonsToAlign.Add(this.alignAddWire);
        }
        else
        { 
            buttonsToAlign.Add(this.alignOptions);
        }

        float rightAlign = 0.0f;
        foreach(RectTransform rt in buttonsToAlign)
        { 
            Vector2 sd = rt.sizeDelta;
            rt.anchoredPosition = new Vector2(rightAlign - sd.x, 0.0f);
            rightAlign -= sd.x;

            allButtons.Remove(rt);
        }

        foreach(RectTransform rt in allButtons)
            rt.gameObject.SetActive(false);

        this.alignNamePulldown.offsetMax = new Vector2(rightAlign, 0.0f);

    }

    public void SelectNode(GNUIHost gnui)
    {
        if(gnui == null)
        { 
            this.DeselectNode();
            return;
        }

        if(this.selectedNode == gnui)
            return;

        this.DeselectNode();
        this.selectedNode = gnui;
        this.potentialFactoryScrollTo = null;

        if (this.selectionHalo == null)
        { 
            GameObject goHalo = new GameObject("SelectionHalo");
            UnityEngine.UI.Image imgHalo = goHalo.AddComponent<UnityEngine.UI.Image>();
            imgHalo.sprite = this.selectionHaloSprite;
            imgHalo.type = Type.Sliced;
            imgHalo.fillCenter = false;
            imgHalo.color = new Color(1.0f, 1.0f, 1.0f, 0.8f);
            this.selectionHalo = imgHalo.rectTransform;

            CanvasGroup cg = goHalo.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
        }

        this.selectionHalo.gameObject.SetActive(true);
        this.selectionHalo.SetParent(gnui.transform);
        this.selectionHalo.localRotation = Quaternion.identity;
        this.selectionHalo.localScale = Vector3.one;
        this.selectionHalo.anchorMin = Vector2.zero;
        this.selectionHalo.anchorMax = Vector3.one;
        this.selectionHalo.offsetMin = -this.selectionHaloRad;
        this.selectionHalo.offsetMax = this.selectionHaloRad;
        this.selectionHalo.SetAsLastSibling();

        if(this.selectedNode.outputSocket != null)
            this.selectedNode.outputSocket.transform.localScale = new Vector3(3.0f, 2.0f, 1.0f);

        if (gnui.slave.nodeType == LLDNBase.NodeType.Output)
            this.HideTrashRegion();
        else
        {
            this.ShowTrashArea();
            this.ShowDeleteButtonDroppable(TrashIcon.Button);
        }
    }

    public void DeselectNode()
    {
        if(this.selectionHalo != null)
        {
            this.selectionHalo.gameObject.SetActive(false);
            this.selectionHalo.SetParent(null);
        }

        if(this.selectedNode != null)
        { 
            this.HideTrashRegion();

            if(this.selectedNode.outputSocket != null)
                this.selectedNode.outputSocket.transform.localScale = Vector3.one;

            this.selectedNode = null;
            this.potentialFactoryScrollTo = null;
        }

    }

    protected override void AppSink_OnWiringCleared(WiringCollection collection)
    { 
        this.lastScroll.Clear();
    }

    public override void OnDocumentChanged(WiringFile newWiring, WiringDocument active, bool appended)
    {
        this.lastScroll.Clear();
        this.viewingDocument = null;

        this.AppSink_OnWiringActiveChanges(newWiring, active);
    }

    protected override void AppSink_OnWiringActiveChanges(WiringCollection collection, WiringDocument wd)
    { 
        if(this.viewingDocument != null)
        {
            this.lastScroll[this.viewingDocument] = 
                new CachedWiringViewSettings( 
                    this.wiringRegion.normalizedPosition, 
                    this.wiringZoom);
        }

        foreach (KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
            GameObject.Destroy(kvp.Value.gameObject);
        this.uiNodes.Clear();

        foreach(HermiteLink hl in this.linkCurves)
            GameObject.Destroy(hl.uiCurve.gameObject);
        this.linkCurves.Clear();

        this.wireReferencesUIData.Clear();

        this.viewingDocument = wd;
        this.selectedNode = null;
        this.potentialFactoryScrollTo = null;

        if (wd != null)
        {
            // Create the node plates
            foreach(LLDNBase gnb in wd.Generators)
            {
                GNUIHost gui = this.CreateHostFromGNGen(gnb, gnb.cachedUILocation);

                if(gui == null)
                    continue;

                this.uiNodes.Add(gnb, gui);
            }

            // Create UI records for connecting the node plates
            foreach(KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
            {
                // Create the curves for connecting items
                GNUIHost host = kvp.Value;
                foreach(GNUIHost.InputConnectionPos icp in host.inputConnections)
                { 
                    // The linguo here is going to get a little weird, because
                    // the audio output is the node's input. We're going to word
                    // stuff here in respect of the nodes.

                    if(icp.connectionParam.IsConnected() == false)
                        continue;

                    LLDNBase gnbInput = host.slave;
                    ParamConnection inputCon = icp.connectionParam;
                    LLDNBase gnbOutput = inputCon.Reference;

                    this.CreateLink(gnbOutput, gnbInput, inputCon, true);
                }
            }
        }

        this.UpdateDocumentText();
        this.UpdateDocumentSize();

        this.App.DoDropdownTextUpdate(this.pulldownDocumentText);

        this.FlagDirty(Dirty.NodeOutputStyle);

        CachedWiringViewSettings cwvs = 
            new CachedWiringViewSettings(
                new Vector2(0.0f, 1.0f), 
                1.0f);

        if(this.viewingDocument != null)
        {
            CachedWiringViewSettings cwvsLookup;
            if(this.lastScroll.TryGetValue(this.viewingDocument, out cwvsLookup) == true)
                cwvs = cwvsLookup;
        }
        //
        this.wiringRegion.content.localScale = new Vector3(cwvs.zoom, cwvs.zoom, 1.0f);
        this.wiringZoom =
            Mathf.Clamp(
                cwvs.zoom,
                PaneWiring.minZoom,
                PaneWiring.maxZoom);
        //
        this.wiringRegion.normalizedPosition = cwvs.scroll;

        this.HideTrashRegion();
    }

    public override void AppSink_OnWiringDeleted(WiringCollection collection, WiringDocument wd)
    { 
        this.lastScroll.Remove(wd);
    }

    public void FindExtents(out Vector2 min, out Vector2 max)
    { 
        if(this.uiNodes.Count == 0)
        { 
            min = Vector2.zero;
            max = Vector2.zero;
            return;
        }

        min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        foreach (KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
        {
            // Calcuate the mins and maxes to add to the document
            Vector2 anch = kvp.Value.rectTransform.anchoredPosition;
            Vector2 sizd = kvp.Value.rectTransform.sizeDelta;

            min.x = Mathf.Min(min.x, anch.x);
            min.y = Mathf.Min(min.y, anch.y - sizd.y);

            max.x = Mathf.Max(max.x, anch.x + sizd.x);
            max.y = Mathf.Max(max.y, anch.y);
        }
    }

    public void UpdateDocumentSize()
    {
        Vector2 min, max;
        this.FindExtents(out min, out max);

        bool realignAll = false;
        float pushRight = Mathf.Max(0.0f, -min.x);
        if(min.x != 0.0f)
        {
            
            foreach (KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
                kvp.Key.cachedUILocation.x += pushRight;
            
            this.docXMax += pushRight;
            realignAll = true;
        }

        float pushUp =  Mathf.Min(0.0f, -max.y);
        if(max.y != 0.0f)
        { 
            foreach (KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
                kvp.Key.cachedUILocation.y += pushUp;
        
            this.docYMin += pushUp;
            realignAll = true;
        }
        
        if(realignAll == true)
        {
            foreach (KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
                kvp.Value.UpdateFromBasePosition();
        
            foreach(HermiteLink hl in this.linkCurves)
                this.UpdateLink(hl);
        }
        
        // Add padding to the bottom right
        this.docYMin = min.y;
        this.docXMax = max.x;
        //
        this.docXMax += this.documentPadding;
        this.docYMin -= this.documentPadding;
        this.wiringRegion.content.sizeDelta = 
            new Vector2(
                this.docXMax, 
                -this.docYMin);
        
        this.documentDimDirty = false;
    }

    private HermiteLink CreateLink(LLDNBase outputNode, LLDNBase inputNode, ParamConnection input, bool addRecord = true)
    {
        GNUIHost guiOutput = null;
        GNUIHost guiInput = null;

        if (uiNodes.TryGetValue(outputNode, out guiOutput) == false)
            return new HermiteLink();

        if(uiNodes.TryGetValue(inputNode, out guiInput) == false)
            return new HermiteLink();

        GNUIHost.InputConnectionPos inputICP = guiInput.GetInputConnection(input);
        if(inputICP.connectionParam == null)
            return new HermiteLink();

        HermiteLink hl = new HermiteLink();
        hl.nodeOutput = outputNode;
        hl.inputParam = input;
        hl.nodeInput = inputNode;

        GameObject goHermite = new GameObject("Curve");
        goHermite.transform.SetParent(this.wiringRegion.content);
        goHermite.transform.localRotation = Quaternion.identity;
        goHermite.transform.localScale = Vector3.one;
        HermiteUICurve huic = goHermite.AddComponent<HermiteUICurve>();
        huic.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        huic.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        huic.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        huic.rectTransform.anchoredPosition = Vector2.zero;
        huic.rectTransform.sizeDelta = Vector2.zero;
        //huic.rectTransform.offsetMin = Vector2.zero;
        //huic.rectTransform.offsetMax = Vector2.zero;

        hl.uiCurve = huic;
        if(addRecord == true)
            this.linkCurves.Add(hl);

        this.UpdateLink(hl);

        return hl;
    }

    // TODO: Remove?
    //private bool RemoveLink(GNBase inputNode, ParamConnection input)
    //{ 
    //    for(int i = 0; i < this.linkCurves.Count; ++i)
    //    { 
    //        HermiteLink hl = this.linkCurves[i];

    //        if(hl.nodeInput != inputNode || hl.inputParam != input)
    //            continue;

    //        this.linkCurves.RemoveAt(i);
    //        GameObject.Destroy(hl.uiCurve.gameObject);
    //        return true;
    //    }
    //    return false;
    //}

    private void UpdateLink(HermiteLink hl)
    {
        GNUIHost guiOutput = null;
        GNUIHost guiInput = null;

        if (uiNodes.TryGetValue(hl.nodeOutput, out guiOutput) == false)
            return;

        if (uiNodes.TryGetValue(hl.nodeInput, out guiInput) == false)
            return;

        GNUIHost.InputConnectionPos inputICP = guiInput.GetInputConnection(hl.inputParam);
        if (inputICP.connectionParam == null)
            return;

        Vector2 outputPos = this.GetNodeOutputSocketPosition(guiOutput);
        Vector2 inputPos = GetNodeInputSocketPosition(guiInput, inputICP);

        this.UpdateLink(hl.uiCurve, outputPos, inputPos);
    }

    private Vector2 GetNodeOutputSocketPosition(GNUIHost host)
    {
        return host.rectTransform.anchoredPosition + host.outputPos;
    }

    private Vector2 GetNodeInputSocketPosition(GNUIHost host, GNUIHost.InputConnectionPos connection)
    {
        return host.rectTransform.anchoredPosition + connection.inputPos;
    }

    private void UpdateLink(HermiteUICurve hermite, Vector2 left, Vector2 right)
    {
        float xDiff = right.x - left.x;
        xDiff = Mathf.Max(xDiff, 20.0f);

        hermite.pointA = left;
        hermite.pointB = right;
        hermite.tangentA = new Vector2(xDiff, 0.0f);
        hermite.tangentB = new Vector2(xDiff, 0.0f);

        hermite.SetVerticesDirty();
    }

    void UpdateDocumentText()
    { 
        if(this.viewingDocument == null)
            this.pulldownDocumentText.text = "--";
        else
        {
            this.pulldownDocumentText.text = 
                this.viewingDocument.GetProcessedWiringName(this.App.Wirings);
        }
    }

    Dictionary<LLDNBase.NodeType, NodePlate> factoryNodePlates = 
        new Dictionary<LLDNBase.NodeType, NodePlate>();

    public void FillDragCreate()
    { 
        List<WiringFactoryListing> createFor = 
            new List<WiringFactoryListing>
            {
                new WiringFactoryListing(GetCategoryName(LLDNBase.Category.Wave), LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.SineWave,       LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.TriWave,        LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.SawWave,        LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.SquareWave,     LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.NoiseWave,      LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.GateWave,       LLDNBase.Category.Wave),
                new WiringFactoryListing(LLDNBase.NodeType.Constant,       LLDNBase.Category.Wave),

                new WiringFactoryListing(GetCategoryName(LLDNBase.Category.Combines), LLDNBase.Category.Combines),
                new WiringFactoryListing(LLDNBase.NodeType.CombAdd,        LLDNBase.Category.Combines),
                new WiringFactoryListing(LLDNBase.NodeType.CombSub,        LLDNBase.Category.Combines),
                new WiringFactoryListing(LLDNBase.NodeType.CombMod,        LLDNBase.Category.Combines),
                new WiringFactoryListing(LLDNBase.NodeType.Min,            LLDNBase.Category.Combines),
                new WiringFactoryListing(LLDNBase.NodeType.Max,            LLDNBase.Category.Combines),
                new WiringFactoryListing(LLDNBase.NodeType.Lerp,           LLDNBase.Category.Combines),

                new WiringFactoryListing(GetCategoryName(LLDNBase.Category.Envelopes), LLDNBase.Category.Envelopes),
                new WiringFactoryListing(LLDNBase.NodeType.EnvADSR,        LLDNBase.Category.Envelopes),
                new WiringFactoryListing(LLDNBase.NodeType.EnvAttack,      LLDNBase.Category.Envelopes),
                new WiringFactoryListing(LLDNBase.NodeType.EnvDecay,       LLDNBase.Category.Envelopes),
                new WiringFactoryListing(LLDNBase.NodeType.Release,        LLDNBase.Category.Envelopes),

                new WiringFactoryListing(GetCategoryName(LLDNBase.Category.Voices), LLDNBase.Category.Voices),
                new WiringFactoryListing(LLDNBase.NodeType.Chorus,         LLDNBase.Category.Voices),
                new WiringFactoryListing(LLDNBase.NodeType.Delay,          LLDNBase.Category.Voices),

                new WiringFactoryListing(GetCategoryName(LLDNBase.Category.Operations), LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Abs,            LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Amplify,        LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Clamp,          LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Cube,           LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Cycle,          LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Hold,           LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Invert,         LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.MAD,            LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Negate,         LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.PowerAmplify,   LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Quant,          LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Sign,           LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Smear,          LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Square,         LLDNBase.Category.Operations),
                new WiringFactoryListing(LLDNBase.NodeType.Stutter,        LLDNBase.Category.Operations),

                new WiringFactoryListing(GetCategoryName(LLDNBase.Category.Special), LLDNBase.Category.Special),
                new WiringFactoryListing(LLDNBase.NodeType.Comment,    LLDNBase.Category.Special),
                new WiringFactoryListing(LLDNBase.NodeType.Gate,       LLDNBase.Category.Special),
                new WiringFactoryListing(LLDNBase.NodeType.GateList,   LLDNBase.Category.Special),
                new WiringFactoryListing(LLDNBase.NodeType.Highlight,  LLDNBase.Category.Special),
                new WiringFactoryListing(LLDNBase.NodeType.QuickOut,   LLDNBase.Category.Special),
                new WiringFactoryListing(LLDNBase.NodeType.Reference,  LLDNBase.Category.Special),
            };

        float fYPlate = 0.0f;
        foreach(WiringFactoryListing wfl in createFor)
        {
            if (string.IsNullOrEmpty(wfl.labelName) == false)
            {
                // ARE WE CREATING A DECORATIVE FACTORY-DRAG PLATE 
                // AS A HEADER FOR A CATEGORY?
                const float plateHeight = 80.0f;
                const float bottomPushup = 5.0f;

                GameObject goPlate = new GameObject("Plate_" + wfl.labelName);
                goPlate.transform.SetParent(dragFactoryRegion.content.transform, false);
                UnityEngine.UI.Image imgPlate = goPlate.AddComponent<UnityEngine.UI.Image>();
                imgPlate.sprite = this.categoryDecorationPlate;
                imgPlate.type = Type.Sliced;
                imgPlate.color = LLDNBase.GetCategoryColor(wfl.category);
                imgPlate.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                imgPlate.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                imgPlate.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                imgPlate.rectTransform.offsetMax = new Vector2(0.0f, -fYPlate);
                imgPlate.rectTransform.offsetMin = new Vector2(0.0f, -fYPlate - plateHeight);

                fYPlate += plateHeight;
                fYPlate += this.dragPlateSpaces;

                GameObject goText = new GameObject("Text_" + wfl.labelName);
                goText.transform.SetParent(goPlate.transform, false);
                UnityEngine.UI.Text txtLabel = goText.AddComponent<UnityEngine.UI.Text>();
                txtLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                txtLabel.text = $"<b>{wfl.labelName}</b>";
                txtLabel.color = Color.black;
                txtLabel.font = this.dragLabelFont;
                txtLabel.fontSize = 20;
                txtLabel.alignment = TextAnchor.MiddleCenter;
                txtLabel.rectTransform.anchorMin = Vector2.zero;
                txtLabel.rectTransform.anchorMax = Vector2.one;
                txtLabel.rectTransform.offsetMin = new Vector2(0.0f, bottomPushup);
                txtLabel.rectTransform.offsetMax = Vector2.zero;
                txtLabel.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                UnityEngine.UI.Button btnHeader = goPlate.AddComponent<UnityEngine.UI.Button>();
                btnHeader.onClick.AddListener(
                    ()=>
                    {
                        this.App.DoVibrateButton();
                        this.ShowCategoryDialog(wfl.category, imgPlate.rectTransform);
                    });
            }
            else
            {
                NodePlate npLog = new NodePlate();

                // ARE WE CREATING A FACTORY-DRAG PLATE FOR A NODE?

                NodeInfo ni = this.GetNodeInfo(wfl.nodeType);

                GameObject goPlate = new GameObject("DragPlate_" + ni.label);
                goPlate.transform.SetParent(dragFactoryRegion.content.transform, false);
                UnityEngine.UI.Image imgPlate = goPlate.AddComponent<UnityEngine.UI.Image>();
                imgPlate.color = LLDNBase.GetCategoryColor(wfl.category);
                imgPlate.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                imgPlate.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                imgPlate.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                const float factoryHorizPadding = 4.0f;
                imgPlate.rectTransform.offsetMin = new Vector2(factoryHorizPadding, -fYPlate - this.dragPlateHeight);
                imgPlate.rectTransform.offsetMax = new Vector2(-factoryHorizPadding, -fYPlate);
                imgPlate.type = UnityEngine.UI.Image.Type.Sliced;
                imgPlate.sprite = this.factoryPlate;
                GenerateDragFactory factory = goPlate.AddComponent<GenerateDragFactory>();
                factory.parent = this;
                factory.app = this.App;
                factory.nodeType = ni.type;
                factory.dragSprite = ni.icon;

                LLDNBase.NodeType ntCpy = ni.type;
                factory.onClick.AddListener(
                    ()=>
                    { 
                        this.App.DoVibrateButton();
                        this.ShowNodeDialog(ntCpy, imgPlate.rectTransform); 
                    });

                GameObject goIco = new GameObject("Icon");
                goIco.transform.SetParent(goPlate.transform, false);
                UnityEngine.UI.Image imgIco = goIco.AddComponent<UnityEngine.UI.Image>();
                imgIco.sprite = ni.icon;
                imgIco.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                imgIco.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                imgIco.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                imgIco.rectTransform.anchoredPosition = new Vector2(this.dragPlatePadIconLeft, -(this.dragPlateHeight-ni.icon.rect.height) * 0.5f);
                imgIco.rectTransform.sizeDelta = ni.icon.rect.size;

                GameObject goText = new GameObject("text");
                goText.transform.SetParent(goPlate.transform, false);
                UnityEngine.UI.Text txtText = goText.AddComponent<UnityEngine.UI.Text>();
                txtText.font = this.dragLabelFont;
                txtText.text = ni.label;
                txtText.color = this.dragFontColor;
                txtText.fontSize = this.dragFontSize;
                txtText.alignment = TextAnchor.MiddleLeft;
                txtText.horizontalOverflow = HorizontalWrapMode.Overflow;
                txtText.verticalOverflow = VerticalWrapMode.Overflow;
                txtText.rectTransform.pivot = new Vector2(0.0f, 0.5f);
                txtText.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                txtText.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                txtText.rectTransform.offsetMin = new Vector2(
                    this.dragPlatePadIconLeft + 
                    ni.icon.rect.width + 
                    this.dragPlatePadIconRight, 
                    0.0f);
                txtText.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);

                fYPlate += this.dragPlateHeight;
                fYPlate += this.dragPlateSpaces;

                npLog.text = txtText;
                npLog.icon = imgIco;
                npLog.plate = imgPlate;
                npLog.button = factory;
                this.factoryNodePlates.Add(wfl.nodeType, npLog);
            }

        }

        fYPlate += scrollBuffer;

        RectTransform scrollContentRT = this.dragFactoryRegion.content;
        scrollContentRT.anchorMin = new Vector2(0.0f, 1.0f);
        scrollContentRT.anchorMax = new Vector2(1.0f, 1.0f);
        scrollContentRT.pivot = new Vector2(0.0f, 1.0f);
        scrollContentRT.offsetMin = new Vector2(0.0f, -fYPlate);
        scrollContentRT.offsetMax = new Vector2(0.0f, 0.0f);
    }

    public override Application.TabTypes TabType()
    { 
        return Application.TabTypes.Wiring;
    }

    public GenBase GenerateForNote(WesternFreqUtils.Key key, int octave, float amp = 0.8f)
    { 
        float freq = WesternFreqUtils.GetFrequency(key, octave);
        return this.GenerateForFrequency(freq, amp);
    }

    public GenBase GenerateForFrequency(float fq, float amp = 1.0f)
    { 
        if ( this.viewingDocument == null)
            return null;

        GenBase gb = 
            this.viewingDocument.CreateGenerator(
                fq, 
                this.App.BeatsPerSecond(),
                amp, 
                this.App.samplesPerSeconds,
                this.viewingDocument,
                this.App.Wirings);

        return gb;
    }

    public override void OnShowPane(bool exercise)
    { 
        this.processingOutputStyles = null;

        base.OnShowPane(exercise);

        this.ClearDragState();
}

    public override void OnHidePane()
    { 
        base.OnHidePane();

        this.ClearDragState();
    }

    public RectTransform EnsureDragState(Sprite sprite, LLDNBase.NodeType type)
    { 
        this.curDragType = type;

        if(this.curDragIcon == null)
        { 
            GameObject go = new GameObject("GeneratorDragIcon"); 
            go.transform.SetParent(this.transform, false);
            UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
            img.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            img.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
            img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            this.curDragIcon = img;
        }
        this.curDragIcon.sprite = sprite;
        this.curDragIcon.rectTransform.sizeDelta = sprite.rect.size;

        return this.curDragIcon.rectTransform;
    }

    public void OnDefferedDragFactoryOnBeginDrag(
        GenerateDragFactory invoker, 
        UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.drag = DragMode.FactoryNode;

        RectTransform rtDrag = EnsureDragState(invoker.dragSprite, invoker.nodeType);
        rtDrag.position = eventData.position;

        // Sacraficial version of the generator used to see what kind
        // of internals it will have an calculate the size of our proxy.
        // We just need to be careful not to add this temp object to the
        // document.
        LLDNBase tmp = this.CreateGenerator(invoker.nodeType, false, true);
        Vector2 dim = GNUIHost.EstimateHeight(invoker.nodeType, tmp.nodeParams, this);
        dim.x = Mathf.Max(dim.x, nodeWidth);
        // Set the size
        this.imgPreviewPlate.gameObject.SetActive(true);
        this.imgPreviewPlate.rectTransform.sizeDelta = dim;
        // But we don't expect it to be visible instantly
        this.imgPreviewPlate.gameObject.SetActive(false);
        this.imgPreviewPlate.transform.SetAsLastSibling();
    }

    public void OnDefferedDragFactoryOnDrag(
        GenerateDragFactory invoker,
        UnityEngine.EventSystems.PointerEventData eventData)
    {

        if (this.drag != DragMode.FactoryNode)
        {
            this.ClearDragState(eventData);
            return;
        }

        RectTransform rtDrag = EnsureDragState(invoker.dragSprite, invoker.nodeType);
        rtDrag.position = eventData.position;

        if(this.WorldPointIntWiringViewport(eventData.position) == true)
        {
            this.imgPreviewPlate.gameObject.SetActive(true);
            this.imgPreviewPlate.transform.position = eventData.position;
            this.imgPreviewPlate.transform.localPosition -= new Vector3(35.0f, -25.0f);
        }
        else
            this.imgPreviewPlate.gameObject.SetActive(false);
    }

    public void OnDefferedDragFactoryOnEndDrag(
        GenerateDragFactory invoker,
        UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (this.drag != DragMode.FactoryNode)
        { 
            this.ClearDragState(eventData);
            return;
        }

        this.HandleFactoryDrop(invoker.nodeType, eventData.position);
        this.ClearDragState(eventData);
        this.FlagDirty(Dirty.NodeOutputStyle);
    }

    public LLDNBase CreateGenerator(LLDNBase.NodeType nodeType, bool addToDoc = true, bool allowOutput = false)
    {
        if (nodeType == LLDNBase.NodeType.Output && allowOutput == false)
            return null;

        LLDNBase ret = LLDNBase.CreateGenerator(nodeType, string.Empty);

        if(ret == null)
        {
            Debug.Log("Unhandled request for generator " + nodeType.ToString());
            return null;
        }

        if(addToDoc == true)
            this.viewingDocument.AddGenerator(ret);

        return ret;
    }

    const float nodeWidth = 200.0f;

    GNUIHost CreateHostFromGNGen(LLDNBase gnbase, Vector2 pos)
    {
        GameObject goNewNode = new GameObject("Node");
        goNewNode.transform.SetParent(this.wiringRegion.content, false);

        UnityEngine.UI.Image imgNewNode = goNewNode.AddComponent<UnityEngine.UI.Image>();
        imgNewNode.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        imgNewNode.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        imgNewNode.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        imgNewNode.rectTransform.anchoredPosition = pos + new Vector2(0.0f, 0.0f);
        imgNewNode.rectTransform.sizeDelta = new Vector2(nodeWidth, 100.0f);
        imgNewNode.type = UnityEngine.UI.Image.Type.Sliced;
        imgNewNode.sprite = this.factoryPlate; // This may need to be changed later.
        imgNewNode.color = gnbase.GetColor();

        NodeInfo typeInfo = GetNodeInfo(gnbase.nodeType);

        GameObject goNodeIcon = new GameObject("Icon");
        goNodeIcon.transform.SetParent(goNewNode.transform, false);
        UnityEngine.UI.Image imgNodeIcon = goNodeIcon.AddComponent<UnityEngine.UI.Image>();
        imgNodeIcon.sprite = typeInfo.icon;
        imgNodeIcon.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        imgNodeIcon.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        imgNodeIcon.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        imgNodeIcon.rectTransform.sizeDelta = typeInfo.icon.rect.size;
        imgNodeIcon.rectTransform.anchoredPosition = new Vector2(5.0f, -5.0f);

        GameObject goText = new GameObject("Title");
        goText.transform.SetParent(goNewNode.transform, false);
        UnityEngine.UI.Text titleNode = goText.AddComponent<UnityEngine.UI.Text>();
        titleNode.color = Color.black;
        titleNode.font = this.dragLabelFont;
        titleNode.fontSize = this.dragFontSize;
        titleNode.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleNode.verticalOverflow = VerticalWrapMode.Overflow;
        TextGenerationSettings tgs = titleNode.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
        TextGenerator tg = titleNode.cachedTextGenerator;
        titleNode.text = typeInfo.label;
        titleNode.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        titleNode.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        titleNode.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        float titleHeight = tg.GetPreferredHeight(typeInfo.label, tgs);
        titleHeight = Mathf.Ceil(titleHeight) + 1.0f;
        titleNode.rectTransform.offsetMax = new Vector2(0.0f, -15.0f);
        titleNode.rectTransform.offsetMin = new Vector2(70.0f, -15.0f - titleHeight);

        GNUIHost gnui = goNewNode.AddComponent<GNUIHost>();
        gnui.InitCaches();
        gnui.parentWindow = this;
        gnui.slave = gnbase;
        gnui.icon = imgNodeIcon;
        gnui.title = titleNode;
        gnui.GenerateUI(this, this.App);

        // Check secret overrides
        ParamText ptTitleOver = gnui.slave.GetParamText(LLDNBase.secretTitleProperty);
        if(ptTitleOver != null)
            gnui.title.text = ptTitleOver.value;

        if (gnbase.HasOutput() == true)
        {
            GameObject goOutArrow = new GameObject("OutArrow");
            goOutArrow.transform.SetParent(goNewNode.transform, false);
            UnityEngine.UI.Image arrowImg = goOutArrow.AddComponent<UnityEngine.UI.Image>();
            Vector2 spriteSz = this.socketArrowSprite.rect.size;
            arrowImg.sprite = this.socketArrowSprite;
            arrowImg.rectTransform.pivot = new Vector2(0.0f, 0.5f);
            arrowImg.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            arrowImg.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
            arrowImg.rectTransform.sizeDelta = this.socketArrowSprite.rect.size;
            arrowImg.rectTransform.anchoredPosition = 
                new Vector2(
                    200.0f, 
                    -10.0f - spriteSz.y * 0.5f);
            SocketUIOutput socketOut = goOutArrow.AddComponent<SocketUIOutput>();
            socketOut.targetGraphic = arrowImg;
            socketOut.parentPane = this;
            socketOut.host = gnui;

            gnui.outputSocket = socketOut;
        }


        gnui.UpdateBasePosition();
        return gnui;
    }

    bool WorldPointIntWiringViewport(Vector3 worldUI)
    {
        Vector3[] rvCorners = new Vector3[4];
        Vector3 vpDrop = this.wiringRegion.viewport.worldToLocalMatrix.MultiplyPoint(worldUI);
        this.wiringRegion.viewport.GetLocalCorners(rvCorners);

        if (
            vpDrop.x < rvCorners[1].x ||
            vpDrop.x > rvCorners[3].x ||
            vpDrop.y > rvCorners[1].y ||
            vpDrop.y < rvCorners[3].y)
        {
            return false;
        }

        return true;
    }

    public bool HandleFactoryDrop(LLDNBase.NodeType nodeType, Vector3 worldDrop)
    { 
        if(this.WorldPointIntWiringViewport(worldDrop) == false)
            return false;

        if(nodeType == LLDNBase.NodeType.GateList)
        { 
            if(this.viewingDocument.GateList != null)
                return false;
        }

        if(this.imgPreviewPlate != null)
            this.imgPreviewPlate.gameObject.SetActive(false);

        LLDNBase gen = this.CreateGenerator(nodeType, false, false);
        if(gen == null)
            return false;

        Vector2 pos = this.wiringRegion.content.worldToLocalMatrix.MultiplyPoint(worldDrop);
        pos -= new Vector2(30.0f, -20.0f);
        //
        this.App.AddWiringNode(this.viewingDocument, gen, pos);
        return true;
    }

    public void OnButton_DropdownDocument()
    { 
        PxPre.DropMenu.StackUtil dropdown = new PxPre.DropMenu.StackUtil("Document"); 
        if(this.viewingDocument != null)
        { 
            dropdown.AddAction(this.pulldownRename, "Rename", ()=>{ this.OnMenu_Rename(); });
            dropdown.AddAction(this.pulldownClone, "Duplicate", () => { this.OnMenu_Duplicate(); });

            dropdown.PushMenu("Organize");
                if(this.App.Wirings.Count() > 1)
                {
                    if(this.App.Wirings.GetActiveIndex() != 0)
                        dropdown.AddAction(this.icoOrganizeMoveUp,      "Move Up",           ()=>{ this.App.MoveWiringOffset(this.viewingDocument, -1); });

                    if(this.App.Wirings.GetActiveIndex() != this.App.Wirings.Count() - 1)
                        dropdown.AddAction(this.icoOrganizeMoveDown,    "Move Down",         ()=>{ this.App.MoveWiringOffset(this.viewingDocument, 1); });

                    if(this.App.Wirings.GetActiveIndex() != 0)
                        dropdown.AddAction(this.icoOrganizeMoveTop,     "Move To Top",       ()=>{ this.App.SetWiringIndex(this.viewingDocument, 0); });

                    if(this.App.Wirings.GetActiveIndex() != this.App.Wirings.Count() - 1)
                        dropdown.AddAction(this.icoOrganizeMoveBottom,  "Move To Bottom",    ()=>{ this.App.SetWiringIndex(this.viewingDocument, this.App.Wirings.Count() - 1); });

                    dropdown.AddSeparator();

                    dropdown.AddAction(this.icoOrganizeName,        "Organize All By Name", ()=>{ this.App.SortWiringsByName(); });
                    dropdown.AddAction(this.icoOrganizeIcon,        "Organize All By Icon", ()=>{ this.App.SortWiringsByCategory(); });

                    dropdown.AddSeparator();
                }
                
                foreach(CategorySpritePair csp in this.categoryIcons)
                { 
                    CategorySpritePair cspCpy = csp;
                    string title = WiringDocument.GetCategoryName(cspCpy.category); 
                    title = char.ToUpper(title[0]) + title.Substring(1);
                
                    dropdown.AddAction(
                        this.viewingDocument.category == cspCpy.category,
                        csp.sprite, 
                        $"Set Icon {title}", 
                        ()=>
                        { 
                            if(this.viewingDocument.category != cspCpy.category)
                            {
                                this.App.SetWiringCategory(
                                    this.viewingDocument, 
                                    cspCpy.category);
                                //this.App.Wirings.SetDirty();
                            }
                        });
                }
            dropdown.PopMenu();

            // TODO: Go to rename
            dropdown.AddSeparator();
        }

        PxPre.DropMenu.Props dropProp = PxPre.DropMenu.DropMenuSingleton.MenuInst.props;
        foreach (WiringDocument wd in this.App.Wirings.Documents)
        {
            WiringDocument wdCpy = wd;

            Sprite icon;
            if(this.catIconLookups.TryGetValue(wd.category, out icon) == false)
                icon = this.unlabledIconCat;
            
            Color c = dropProp.unselectedColor;

            if (wdCpy == this.viewingDocument)
            {
                // Nothing else changes the icon of a selected pulldown so we're disabling this.
                // in the end, we're probably just going to delete the selected pulldown icon.
                //
                //icon = this.pulldownWiringSel;
                c = dropProp.selectedColor;
            }

            dropdown.AddAction(
                icon, 
                c,
                wd.GetProcessedWiringName(this.App.Wirings), 
                ()=>{ this.OnMenu_SelectWiring(wdCpy); });
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            dropdown.Root,
            this.documentPulldownRect);

        this.App.DoVibrateButton();
    }

    private void OnMenu_Rename()
    {
        if(this.viewingDocument == null)
            return;

        PxPre.UIL.Dialog dlg = 
            this.App.CreateRenameDialog(
               "Rename Wiring",
               $"Enter a new name for instrument {this.viewingDocument.GetName()}.",
               "Name",
               "Wiring name",
               this.viewingDocument.GetName(),
               "Rename",
               (x) => 
               { 
                   if(x == this.viewingDocument.GetName())
                       return;

                    this.App.RenameWiring(this.viewingDocument, x);
               },
               WiringDocument.MaxNameLen);
        
        dlg.host.LayoutInRT(false);

        this.App.SetupDialogForTransition(dlg, this.alignNamePulldown, false);
    }

    private void OnMenu_Duplicate()
    {
        if(this.viewingDocument == null)
            return;

        PxPre.UIL.Dialog dlg = 
            this.App.CreateRenameDialog(
               "Duplicate Wiring",
               $"Enter a new name for duplicate of {this.viewingDocument.GetName()} .",
               "Name",
               "New wiring",
               this.viewingDocument.GetName(),
               "Duplicate",
               (x) =>
               {
                   this.App.CloneActiveWiring(x);
               },
               WiringDocument.MaxNameLen);

        dlg.host.LayoutInRT(false);

        this.App.SetupDialogForTransition(dlg, this.alignNamePulldown, false);
    }

    private void OnMenu_SelectWiring(WiringDocument document)
    { 
        if(document == null)
            return;

        this.App.SetActiveDocument(document, false);
    }

    PxPre.UIL.Dialog AddNewWiringDialog()
    { 
        PxPre.UIL.Dialog dlg = 
            this.App.CreateRenameDialog(
                "Add New Wiring", 
                "Enter a name for the new instrument wiring.", 
                "Name", 
                "Name",
                "New Instrument",
                "Create New",  
                (x)=>{ this.App.AddNewWiringDocument(x); },
                WiringDocument.MaxNameLen);

        return dlg;
    }

    public void OnButton_AddDocument()
    {
        PxPre.UIL.Dialog dlg = AddNewWiringDialog();

        dlg.host.LayoutInRT(false);

        this.App.SetupDialogForTransition(dlg, this.alignAddWire);

        this.App.DoVibrateButton();
    }

    

    public void OnButton_UploadDocument()
    {
        Application.BrowserTextUpload(".phon", "Managers", "LoadDocumentString");

        this.App.DoVibrateButton();
    }

    public void OnButton_WiringOptions()
    { 
        PxPre.DropMenu.StackUtil stack = new PxPre.DropMenu.StackUtil();

        stack.AddAction(this.icoAddNewWiring,"Add New Wiring", ()=>{ this.OnMenu_AddNewWiring(); });
        stack.AddSeparator();

        if(string.IsNullOrEmpty(this.App.Wirings.Path) == false)
            stack.AddAction(this.icoSave, "Save", () => { this.OnMenu_Save(); });

        stack.AddAction(this.icoSaveAs, "Save As", () => { this.OnMenu_SaveAs(); });
        stack.AddSeparator();

        stack.AddAction(this.icoOpen,   "Open", () => { this.OnMenu_Open(); });
        stack.AddAction(this.icoAppend, "Append", () => { this.OnMenu_Append(); });
        stack.PushMenu("Included Examples");
            foreach(Application.InternalDocument idoc in this.App.internalDocuments)
            { 
                Application.InternalDocument idocCpy = idoc;
                stack.AddAction(
                    this.icoOpenInternalDoc, 
                    idocCpy.title, 
                    ()=>
                    { 
                        this.OnMenu_LoadInternalDocument(idocCpy.idName);
                    });
            }
        stack.PopMenu();
        stack.AddSeparator();

        stack.AddAction(this.icoDelCur, "Delete Wiring",() => { this.OnMenu_DeleteWiring(); });

        stack.AddAction(this.icoNewDoc, "New Document", () => { this.OnMenu_NewDocument(); });

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stack.Root,
            this.alignOptions);

        this.App.DoVibrateButton();
    }

    public void OnMenu_LoadInternalDocument(string idName)
    { 
        if(this.App.Wirings.Dirty == true)
        { 
            FutureInteruptHold cancelRetBack = new FutureInteruptHold();

            PxPre.UIL.Dialog dlg = 
                this.App.dlgSpawner.CreateDialogTemplate(
                    "Unsaved changes",
                    "There are unsaved changes.\nDo you wish to save before loading another wiring document?",
                    new PxPre.UIL.DlgButtonPair("Cancel", null),
                    new PxPre.UIL.DlgButtonPair(
                        "Discard",
                        (x) =>
                        {
                            cancelRetBack.Interrupt();
                            this.App.LoadInternalDocument(idName);

                            return true;
                        }),
                    new PxPre.UIL.DlgButtonPair(
                        "Save Changes First",
                        (x) =>
                        {
                            cancelRetBack.Interrupt();
                            this.App.Save(
                                () => 
                                { 
                                    this.App.LoadInternalDocument(idName);
                                }, 
                                null);

                            return true;
                        }));

            dlg.host.LayoutInRT(false);
            dlg.options[1].button.targetGraphic.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            FutureInterupt fi = this.App.SetupDialogForTransition(dlg, this.alignOptions, false);
            cancelRetBack.SetInterrupt(fi);
        }
        else
            this.App.LoadInternalDocument(idName);
    }

    public void OnMenu_Save()
    { 
        this.App.Save(null, this.alignOptions);
    }

    public void OnMenu_AddNewWiring()
    { 
        PxPre.UIL.Dialog dlg = AddNewWiringDialog();
        dlg.host.LayoutInRT(false);

        this.App.SetupDialogForTransition(dlg, this.alignOptions);

        this.App.DoVibrateButton();
    }

    public void OnMenu_SaveAs()
    {
        this.App.SaveAsDialog(null, this.alignOptions);
    }

    public void OnMenu_Open()
    {
        if (this.App.IsDirty() == true)
        {
            FutureInteruptHold cancelRetBack = new FutureInteruptHold();

            PxPre.UIL.Dialog dlg = 
                this.App.dlgSpawner.CreateDialogTemplate(
                    "Unsaved changes",
                    "There are unsaved changes.\nDo you wish to save before loading another wiring document?",
                    new PxPre.UIL.DlgButtonPair("Cancel", null),
                    new PxPre.UIL.DlgButtonPair(
                        "Discard",
                        (x) =>
                        {
                            cancelRetBack.Interrupt();
                            PxPre.UIL.Dialog dlgOpen = this.App.OpenDialog();

                            // Transition from button to full open dialog.
                            this.App.SetupDialogForTransition(dlgOpen, x.targetGraphic.rectTransform, false, false);

                            return true;
                        }),
                    new PxPre.UIL.DlgButtonPair(
                        "Save Changes First",
                        (x) =>
                        {
                            cancelRetBack.Interrupt();
                            this.App.Save(
                                () => 
                                { 
                                    PxPre.UIL.Dialog dlgOpen = this.App.OpenDialog();
                                }, 
                                null);

                            return true;
                        }));

            dlg.host.LayoutInRT(false);
            dlg.options[1].button.targetGraphic.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            FutureInterupt fi = this.App.SetupDialogForTransition(dlg, this.alignOptions, false);
            cancelRetBack.SetInterrupt(fi);
        }
        else
            this.App.OpenDialog();
    }

    public void OnMenu_Append()
    {
        this.App.AppendDialog();
    }

    public void OnMenu_DeleteWiring()
    {
        PxPre.UIL.Dialog dlg =
                this.App.dlgSpawner.CreateDialogTemplate(
                    "Are you sure?",
                    $"Are you sure you want to remove the wiring instrument {this.App.Wirings.Active.GetName()} from the document?",
                    new PxPre.UIL.DlgButtonPair("Cancel", null),
                    new PxPre.UIL.DlgButtonPair(
                        "Delete",
                        (x) =>
                        {
                            this.App.DeleteActiveWiring();
                            return true;
                        }));

        dlg.options[1].button.targetGraphic.color = Color.red;

        dlg.host.LayoutInRT(false);

        this.App.SetupDialogForTransition(dlg, this.alignOptions, false, true);
    }

    /// <summary>
    /// Menu callback for a new wiring document.
    /// </summary>
    public void OnMenu_NewDocument()
    {
        if(this.App.IsDirty() == true)
        { 
            PxPre.UIL.Dialog dlg = 
                this.App.dlgSpawner.CreateDialogTemplate(
                    "Unsaved changes",
                    "There are unsaved changes.\nDo you wish to save before clearing the current wiring document?",
                    new PxPre.UIL.DlgButtonPair("Cancel", null),
                    new PxPre.UIL.DlgButtonPair(
                        "Discard", 
                        (x)=>
                        {
                            this.App.NewDocument(); 
                            return true; 
                        }),
                    new PxPre.UIL.DlgButtonPair(
                        "Save Changes First", 
                        (x)=>
                        {
                            this.App.Save( ()=>{ this.App.NewDocument(); }, this.alignOptions); 
                            return true; 
                        }));

            dlg.options[1].button.targetGraphic.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);

            dlg.host.LayoutInRT(false);

            this.App.SetupDialogForTransition(dlg, this.alignOptions, false);
        }
        else
            this.App.NewDocument();
    }

    static string saveName = "PhonicWiring.phon";
    public void OnButton_DownloadDocument()
    {
        this.App.CreateRenameDialog(
            "Document Name", 
            "Enter save name", 
            "Filename", 
            "Enter Filename",
            saveName,
            "Download",
            (x)=>
            {
                if(string.IsNullOrEmpty(x) == true)
                    return;

                if(x.EndsWith(".phon", System.StringComparison.OrdinalIgnoreCase) == false)
                    x += ".phon";

                saveName = x;
                this.App.DownloadFile(saveName);
                
            });

        this.App.DoVibrateButton();
    }

    public void OnButton_DeleteSelectedNode()
    { 
        // This shouldn't ever happen.
        if(this.selectedNode == null)
            return;

        // NO! This also shouldn't be possible.
        if(this.selectedNode.slave.nodeType == LLDNBase.NodeType.Output)
            return;

        this.App.DeleteWiringNode(this.viewingDocument, this.selectedNode.slave);

        

        this.App.DoVibrateButton();
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      KeyPreviews
    //
    ////////////////////////////////////////////////////////////////////////////////
    
    private void PlayTestKey(PxPre.Phonics.WesternFreqUtils.Key k, int o)
    { }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      Cursor Handlers for GNUIHost
    //
    ////////////////////////////////////////////////////////////////////////////////

    public void UpdateGNUIHostFromDrag(GNUIHost host)
    {
        LLDNBase gnbSlave = host.slave;
        foreach (HermiteLink hl in this.linkCurves)
        {
            if(hl.nodeInput == gnbSlave || hl.nodeOutput == gnbSlave)
                this.UpdateLink(hl);
        }
    }

    Vector2 cachedGNUIDragPos = Vector2.zero;
    public void OnGNUIHost_BeginDrag(GNUIHost host, UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.drag = DragMode.WiringNode;
        this.draggedHost = host;

        // If they started dragging, what they'r dragging is what they've selected (or have selected)
        // so we nix bringing up documentation when we get the signal for the touch release. This is not
        // 100% for multi-touch, but still sane.
        this.potentialFactoryScrollTo = null;

        this.originalGNUIPredragPos = host.rectTransform.anchoredPosition;

        this.UpdateGNUIHostFromDrag(host);

        NodeInfo ni = this.GetNodeInfo(host.slave.nodeType);
        this.wiredDragOutBoundsIcon.sprite = ni.icon;
        this.wiredDragOutBoundsIcon.rectTransform.sizeDelta = ni.icon.rect.size;

        if(host.slave.nodeType != LLDNBase.NodeType.Output)
            this.ShowDeleteButtonDroppable( TrashIcon.DragZoneInactive);

        this.cachedGNUIDragPos = host.slave.cachedUILocation;
    }

    public void OnGNUIHost_Drag(GNUIHost host, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if(this.drag != DragMode.WiringNode)
        {
            this.ClearDragState(eventData);
            return;
        }

        // See comment for same code in onGNUIHost_BeginDrag(). We do it again in here to cover
        // our bases.
        this.potentialFactoryScrollTo = null;

        Vector3 currentPivot =
            host.rectTransform.localToWorldMatrix.MultiplyPoint(GNUIHost.cachedStartDragPos); 

        Vector3 cpos = eventData.position;

        if (host.slave.nodeType != LLDNBase.NodeType.Output)
            this.ShowDeleteButtonDroppable( TrashIcon.DragZoneInactive);

        // If dragged inside of delete region
        if (PointInsideRectTransform(
                cpos,
                this.deleteNoteButton.targetGraphic.rectTransform) == true)
        {
            if (this.originalGNUIPredragPos != host.rectTransform.anchoredPosition)
            {
                host.rectTransform.anchoredPosition = this.originalGNUIPredragPos;
                this.UpdateGNUIHostFromDrag(host);
                this.documentDimDirty = true;
                this.FlagDirty(Dirty.NodeOutputStyle);
            }

            this.wiredDragOutBoundsIcon.gameObject.SetActive(true);
            this.wiredDragOutBoundsIcon.transform.position = cpos;

            if (host.slave.nodeType != LLDNBase.NodeType.Output)
                this.ShowDeleteButtonDroppable( TrashIcon.DragZoneReady);
        }
        else
        {
            // Normal dragging
            // In bounds
            this.wiredDragOutBoundsIcon.gameObject.SetActive(false);

            Vector3 eventPos = cpos;
            host.transform.position += eventPos - currentPivot;

            this.documentDimDirty = true;
            this.UpdateGNUIHostFromDrag(host);
        }
    }

     public void OnGNUIHost_EndDrag(GNUIHost host, UnityEngine.EventSystems.PointerEventData eventData)
    {
        Vector3 curPos = eventData.position;

        bool draggingOverDelete = 
            PointInsideRectTransform(
                curPos, 
                this.deleteNoteButton.targetGraphic.rectTransform);

        if (draggingOverDelete == true)
        { 
            if(host.slave.nodeType != LLDNBase.NodeType.Output)
            {
                this.App.DeleteWiringNode(this.viewingDocument, host.slave);
                this.App.DoVibrateButton();
            }
        }
        else
        {
            Vector3 currentPivot =
                host.rectTransform.localToWorldMatrix.MultiplyPoint(GNUIHost.cachedStartDragPos); 

            Vector3 eventPos = curPos;
            host.transform.position += eventPos - currentPivot;

            // Pre-emptivly create the undo scope so that if ShiftDocument is performed, it will be
            // clustered in the same Undo group as the one generated for MoveWiringNode().
            using( PxPre.Undo.UndoDropSession uds = this.App.CreateAppDropSession("Moved Node"))
            {

                this.App.MoveWiringNode(
                    this.App.Wirings, 
                    this.viewingDocument, 
                    host.slave,
                    host.rectTransform.anchoredPosition);

                Vector2 min;
                Vector2 max;
                this.FindExtents( out min, out max);
                Vector2 offset = 
                    new Vector2(
                        Mathf.Min(min.x, 0.0f), 
                        Mathf.Max(0.0f, max.y));

                if(offset != Vector2.zero)
                {
                    this.App.ShiftDocument(
                        this.App.Wirings,
                        this.viewingDocument,
                        -offset);
                }
            }
        }

        // TODO: Do we ever need to check? Or just always flag
        //
        // this documentDimDirty might have been the system before
        // the unified dirty system.
        if(this.documentDimDirty == true)
            this.FlagDirty(Dirty.DocumentSize);

        this.ClearDragState(eventData);

    }

    void ShowDeleteButtonDroppable(TrashIcon ti)
    { 
        float bottomPad = 0.0f;
        string format = "";
        switch(ti)
        {
            case TrashIcon.DragZoneReady:
                this.deleteNoteButton.targetGraphic.color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                this.deleteNodeImg.sprite = null;
                this.deleteNoteButton.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                this.trashIcon.rectTransform.anchoredPosition = this.trashIconAnchor;

                //format = "Drop to delete selected {0}";
                format = "Drop to delete selected";
                break;
        
            case TrashIcon.DragZoneInactive:
                this.deleteNoteButton.targetGraphic.color = new Color(1.0f, 0.8f, 0.8f, 1.0f);
                this.deleteNodeImg.sprite = null;
                this.deleteNoteButton.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                this.trashIcon.rectTransform.anchoredPosition = this.trashIconAnchor;

                //format = "Drop to delete selected {0}";
                format = "Drop to delete selected";
                break;

            case TrashIcon.Button:
                this.deleteNoteButton.targetGraphic.color = Color.white;
                this.deleteNoteButton.transition    = UnityEngine.UI.Selectable.Transition.SpriteSwap;
                this.deleteNodeImg.sprite           = this.App.uiFactory.buttonStyle.normalSprite;
                this.deleteNodeImg.type             = Type.Sliced;
                this.deleteNoteButton.spriteState   = this.App.uiFactory.buttonStyle.spriteState;
                this.trashIcon.rectTransform.anchoredPosition = this.trashIconAnchor - (Vector2)this.deleteNoteButton.moveOnPress;

                //format = "Press to delete selected {0}";
                format = "Press to delete selected";
                bottomPad = 10.0f;
                break;
        }

        if(this.selectedNode != null && string.IsNullOrEmpty(format) == false)
        {
            NodeInfo ni = this.GetNodeInfo(this.selectedNode.slave.nodeType);
            this.deleteButtonCaption.text = string.Format(format, ni.label);
        }
        else
            this.deleteButtonCaption.text = "";

        RectTransform rtCaption     = this.deleteButtonCaption.rectTransform;
        rtCaption.offsetMin         = new Vector2(75.0f, bottomPad);
        rtCaption.offsetMax         = new Vector2(-10.0f, -10.0f);
    }

    public void OnGNUIHost_PointerDown(GNUIHost host, UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.potentialFactoryScrollTo = host;
        this.SelectNode(host);
    }

    public void OnGNUIHost_Click(GNUIHost host, UnityEngine.EventSystems.PointerEventData eventData)
    { 
        if(this.selectedNode != host || this.potentialFactoryScrollTo != host)
            return;

        if(this.ScrollToPlate(host.slave.nodeType) == true)
            this.App.DoVibrateButton();
    }

    public bool ScrollToPlate(LLDNBase.NodeType type)
    {
        NodePlate np;
        if(this.factoryNodePlates.TryGetValue(type, out np) == false)
            return false;

        float vpHeight = this.dragFactoryRegion.viewport.rect.height;
        float cHeight = this.dragFactoryRegion.content.rect.height;
        float scrollable = cHeight - vpHeight;  // The scrollable amount in pixels

        Vector3 [] oldScrollIfForcing = null;
        float oldVertNorm = this.dragFactoryRegion.verticalNormalizedPosition;

        if(vpHeight < cHeight)
        { 
            Vector2 pPos = np.plate.rectTransform.anchoredPosition; // AnchPos Y value should be how far it's lowered in the scroll view (to get to top)
            Vector2 pSze = np.plate.rectTransform.sizeDelta;        // The size of the plate we're checking in case we need to compare it to the bottom scroll

            float invScroll = 1.0f - oldVertNorm;          // Inverted scroll value, to match with nPlateY (below)

            float vptop = invScroll * scrollable;   // The top of the viewport in pixels (what it's viewing in the canvas)
            float vpbot = vptop + vpHeight;           // The bottom of the viewport in pixels
            float nPlateY = -pPos.y;                // Negated plate position ... because i'd rather worth with positive Y going DOWN in the UI

            const float detectionToleranceInPx = 2.0f;
            if (nPlateY < vptop - detectionToleranceInPx)
            {
                oldScrollIfForcing = new Vector3[4];
                this.dragFactoryRegion.verticalScrollbar.handleRect.GetWorldCorners(oldScrollIfForcing);

                // If it's above the viewport, scroll so it's at the top.
                float scrolldown = (nPlateY - scrollBuffer)  / scrollable;   // How far we need to scroll to get to it in normalize space.
                this.dragFactoryRegion.verticalNormalizedPosition = 1.0f - scrolldown;
            }
            else if(nPlateY + pSze.y > vpbot + detectionToleranceInPx)
            { 
                oldScrollIfForcing = new Vector3[4];
                this.dragFactoryRegion.verticalScrollbar.handleRect.GetWorldCorners(oldScrollIfForcing);

                // If it's below the viewport, scroll so it's at the bottom.
                float pbot = nPlateY + pSze.y;
                float scrolldown = ((pbot + scrollBuffer) - vpHeight) / scrollable;
                this.dragFactoryRegion.verticalNormalizedPosition = 1.0f - scrolldown;
            }
        }

        // And we need to do animation effects.
        this.App.uiFXTweener.SlidingAnchorFade(
            np.text.rectTransform, 
            null, 
            new Vector2(-30.0f, 0.0f), 
            false, 
            true, 
            0.3f, 
            TweenUtil.RestoreMode.RestoreLocal);

        // The scroll isn't actually updated until later, which also means the child won't be,
        // which means the plate isn't in a good state to query the corners for - so we hold off
        // until next frame.
        this.App.StartCoroutine( this.DoPlateReticuleNextFrame(np.plate.rectTransform));

        if (oldScrollIfForcing != null)
        {
            // We can't just get its world corners because changing the normalizedPosition
            // doesn't instantly update it, so we need to figure out where the corners will be ourselves.

            float normDiff = oldVertNorm - this.dragFactoryRegion.verticalNormalizedPosition;

            float pxDiff = normDiff * (scrollable/ cHeight) * vpHeight;
            // And because we're doing this to world coordinates, we need to convert the scale.
            pxDiff *= this.dragFactoryRegion.verticalScrollbar.transform.lossyScale.y;

            Vector3 [] newHandle = 
                new Vector3[4]
                { 
                    new Vector3(oldScrollIfForcing[0].x, oldScrollIfForcing[0].y - pxDiff, oldScrollIfForcing[0].z),
                    new Vector3(oldScrollIfForcing[1].x, oldScrollIfForcing[1].y - pxDiff, oldScrollIfForcing[1].z),
                    new Vector3(oldScrollIfForcing[2].x, oldScrollIfForcing[2].y - pxDiff, oldScrollIfForcing[2].z),
                    new Vector3(oldScrollIfForcing[3].x, oldScrollIfForcing[3].y - pxDiff, oldScrollIfForcing[3].z)
                };

            this.App.StartCoroutine(
                this.App.TransitionReticule(
                    CanvasSingleton.canvas.GetComponent<RectTransform>(),
                    oldScrollIfForcing,
                    newHandle,
                    null));
        }

        return true;
    }

    public IEnumerator DoPlateReticuleNextFrame(RectTransform rt)
    {
        // See comments where it's used in ScrollToPlate() for more 
        // information on its usage.
        yield return new WaitForEndOfFrame();

        Vector3 [] rvrt = new Vector3[4];
        rt.GetWorldCorners(rvrt);

        float horizOffset = 50.0f;
        float vertOffset = 10.0f;

        Vector3 [] rvStart = 
            new Vector3[]
            { 
                new Vector3(rvrt[0].x - horizOffset, rvrt[0].y - vertOffset, rvrt[0].z),
                new Vector3(rvrt[1].x - horizOffset, rvrt[1].y + vertOffset, rvrt[1].z),
                new Vector3(rvrt[2].x + horizOffset, rvrt[2].y + vertOffset, rvrt[2].z),
                new Vector3(rvrt[3].x + horizOffset, rvrt[3].y - vertOffset, rvrt[3].z)
            };

        this.App.StartCoroutine( 
            this.App.TransitionReticule(
                CanvasSingleton.canvas.GetComponent<RectTransform>(),
                rvStart,
                rvrt,
                null));
    }

    public void HandleSocketInput_BeginDrag(SocketUIInput input, UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.previewCurve.gameObject.SetActive(true);

        // The the input connection was plugging in, set the drag operation to
        // edit it. 
        if (input.connection.IsConnected() == true)
        {
            // TODO: Check how relevant this is
            this.previousInputDraggedDC = input.connection;
            this.previousInputDragHost = input.host.slave;

            // Convert this to a socket output drag
            GNUIHost hostOutputter;
            if(
                // Get the output node, this is what we will redirect the dragging handling to.
                this.uiNodes.TryGetValue(input.connection.Reference, out hostOutputter) == true &&  
                // Sanity check to actually make sure it can output to something.
                hostOutputter.outputSocket != null)
            { 
                // Disconnect the connection. If they want to undo it, they can instantly release the
                //drag or re-connect it.

                
                this.dcInputOnDrag = true;
                for(int i = 0; i < this.linkCurves.Count; ++i)
                { 
                    HermiteLink hl = this.linkCurves[i];
                    if(hl.inputParam != input.connection)
                        continue;

                    Color c = hl.uiCurve.color;
                    c.a *= 0.25f;
                    hl.uiCurve.color = c;
                    break;
                }

                // Hand of the behaviour to be an output of what was previously plugging into the input
                eventData.pointerDrag = hostOutputter.outputSocket.gameObject;
                this.HandleSocketOutput_BeginDrag(hostOutputter.outputSocket, eventData);
                return;
            }
            else
            {
                this.previewCurve.gameObject.SetActive(false);
                return;
            }
        }
        else
        { 
            // Go into a mode to make a connection from the output to the input.
            this.drag = DragMode.SocketInput;
            this.draggingInput = input;

            LLDNBase gbinputSel = input.host.slave;
            foreach(LLDNBase gb in this.viewingDocument.Generators)
            { 
                if(gb == gbinputSel)
                    continue;

                if(gb.HierarchyContains(gbinputSel) == true)
                    continue;

                GNUIHost gnui;
                if(this.uiNodes.TryGetValue(gb, out gnui) == true)
                {
                    gnui.DialateOutputArrow();
                    this.dialatedOutputs.Add(gnui);
                }
            }

        }
    }

    public void HandleSocketInput_Drag(SocketUIInput input, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (this.drag != DragMode.SocketInput || this.draggingInput == null)
        { 
            this.ClearDragState(eventData);
            return;
        }

        Vector3 localCursorPt = this.wiringRegion.content.worldToLocalMatrix.MultiplyPoint(eventData.position);

        Vector2 conpos = input.host.GetInputConnection(input.connection).inputPos;
        conpos += input.host.rectTransform.anchoredPosition;

        this.previewCurve.gameObject.SetActive(true);
        this.UpdateLink(this.previewCurve, localCursorPt, conpos);
    }

    public void HandleSocketInput_EndDrag(
        SocketUIInput input, 
        UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.previewCurve.gameObject.SetActive(false);

        if (this.drag == DragMode.SocketInput)
        { 
            if (this.lastOutputHover != null)
            {
                this.App.RewireNodeAudio(
                    this.viewingDocument,
                    this.lastOutputHover.host.slave,
                    input.host.slave,
                    input.connection);
            }
        }
        
        this.ClearDragState(eventData);
    }

    public void HandleSocketInput_PointerEnter(SocketUIInput input, UnityEngine.EventSystems.PointerEventData eventData)
    { 
        if(this.lastInputHover != null)
        {
            this.lastInputHover.targetGraphic.color = Color.white;
            this.ApplyCrosshair(null);
        }

        this.lastInputHover = input;

        if(this.drag == DragMode.SocketOutput)
        {
            this.lastInputHover.targetGraphic.color = Color.green;
            this.ApplyCrosshair(this.lastInputHover.targetGraphic.rectTransform);
        }
    }

    public void HandleSocketInput_PointerExit(SocketUIInput input, UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.lastInputHover = null;
        input.targetGraphic.color = Color.white;
        this.ApplyCrosshair(null);
    }

    public void HandleSocketOutput_Drag(SocketUIOutput output, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (this.drag != DragMode.SocketOutput)
            return;

        if(this.draggingOutput == null)
            return;

        Vector3 localCursorPt = this.wiringRegion.content.worldToLocalMatrix.MultiplyPoint(eventData.position);
        Vector2 socket = this.GetNodeOutputSocketPosition(output.host);
        this.UpdateLink(this.previewCurve, socket, localCursorPt);
    }

    public void HandleSocketOutput_EndDrag(SocketUIOutput output, UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.draggingOutput = null;
        if(this.drag != DragMode.SocketOutput)
            return;

        this.previewCurve.gameObject.SetActive(false);

        do
        {
            if(this.dcInputOnDrag == true)
            { 
                // If we got here by clicking on an entrance, and there's
                // no end input 
                if(
                    this.lastInputHover != null && 
                    this.lastInputHover.connection == previousInputDraggedDC)
                { 
                    // No change to be made!
                    break;
                }

                if(this.lastInputHover == null)
                { 
                    // It was created by dragging the input (instead of an output) and wasn't
                    // dropped on anything. That's the way stuff gets disconnected,
                    this.App.RewireNodeAudio(
                        this.viewingDocument,
                        null,
                        this.previousInputDragHost,
                        this.previousInputDraggedDC);

                    break;
                }
            }

            if(this.lastInputHover != null)
            { 
                if(this.lastInputHover.connection.Reference != output.host.slave)
                {

                    if(this.lastInputHover.connection.IsConnected() == true)
                    { 
                        // Check against cycles
                        if(output.host.slave.HierarchyContains(this.lastInputHover.host.slave) == true)
                            break;
                    }

                    this.App.RewireNodeAudio(
                        this.viewingDocument,
                        output.host.slave,
                        this.lastInputHover.host.slave,
                        this.lastInputHover.connection);

                }
            }
            else if(this.dcInputOnDrag)
            {
                // To get here, we had to disconnect an connection from an input, that
                // was never repaired.
                //this.App.FlagDirty();
            }
        }
        while(false);

        this.FlagDirty(Dirty.NodeOutputStyle);
        this.ClearDragState(eventData);
    }

    public void HandleSocketOutput_BeginDrag(SocketUIOutput output, UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.draggingOutput = output;
        this.drag = DragMode.SocketOutput;

        this.previewCurve.gameObject.SetActive(true);

        Vector3 localCursorPt = this.wiringRegion.content.worldToLocalMatrix.MultiplyPoint(eventData.position);
        Vector2 socket = this.GetNodeOutputSocketPosition(output.host);

        LLDNBase gnOutDrag = this.draggingOutput.host.slave;
        foreach (LLDNBase gb in this.viewingDocument.Generators)
        { 
            if(gb == gnOutDrag)
                continue;

            if(gnOutDrag.HierarchyContains(gb) == true)
                continue;

            GNUIHost gnui;
            if(this.uiNodes.TryGetValue(gb, out gnui) == true)
            {
                gnui.DialateInputArrows();
                this.dialatedInputs.Add(gnui);
            }
        }

        this.UpdateLink(this.previewCurve, socket, localCursorPt);
    }

    public void HandleSocketOutput_PointerEnter(SocketUIOutput output, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if(this.lastOutputHover != null)
        {
            this.lastOutputHover.targetGraphic.color = Color.white;
            this.ApplyCrosshair(null);
        }

        this.lastOutputHover = output;

        if(this.drag == DragMode.SocketInput)
        {
            this.lastOutputHover.targetGraphic.color = Color.green;
            this.ApplyCrosshair(this.lastOutputHover.targetGraphic.rectTransform);
        }
    }

    public void HandleSocketOutput_PointerExit(SocketUIOutput output, UnityEngine.EventSystems.PointerEventData eventData)
    {
        
        this.lastOutputHover = output;
        output.targetGraphic.color = Color.white;
        this.ApplyCrosshair(null);
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      IGNParamUICreator
    //
    ////////////////////////////////////////////////////////////////////////////////

    public struct ParameterPeice
    { 
        public UnityEngine.UI.Button button;
        public UnityEngine.UI.Image plate;

        public UnityEngine.UI.Image thumbPlate;
        public UnityEngine.UI.Text name;
    }

    ParameterPeice GenerateParameterPeice(string name, RectTransform rt, ref float y, LLDNBase genNode)
    {
        ParameterPeice ret = new ParameterPeice();

        GameObject goBtn = new GameObject("ParameterPeiceButton");
        goBtn.transform.SetParent(rt, false);
        UnityEngine.UI.Image imgBtn = goBtn.AddComponent<UnityEngine.UI.Image>();
        imgBtn.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        imgBtn.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        imgBtn.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        imgBtn.rectTransform.offsetMin = new Vector2(5.0f, y - (this.paramPlateSprite.rect.height + 10.0f));
        imgBtn.rectTransform.offsetMax = new Vector2(-5.0f, y);
        imgBtn.sprite = this.paramContainerSprite;
        imgBtn.type = UnityEngine.UI.Image.Type.Sliced;
        UnityEngine.UI.Button btnBtn = goBtn.AddComponent<UnityEngine.UI.Button>();
        btnBtn.targetGraphic = imgBtn;

        GameObject goPlate = new GameObject("Plate");
        goPlate.transform.SetParent(goBtn.transform, false);
        UnityEngine.UI.Image imgPlate = goPlate.AddComponent<UnityEngine.UI.Image>();
        imgPlate.sprite = this.paramPlateSprite;
        imgPlate.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        imgPlate.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        imgPlate.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        imgPlate.rectTransform.sizeDelta = this.paramPlateSprite.rect.size;
        imgPlate.rectTransform.anchoredPosition = new Vector2(5.0f, -5.0f);

        GameObject goText = new GameObject("Text");
        goText.transform.SetParent(goBtn.transform, false);
        UnityEngine.UI.Text txtText = goText.AddComponent<UnityEngine.UI.Text>();
        txtText.text = name;
        txtText.color = Color.black;
        txtText.font = this.dragLabelFont;
        txtText.fontSize = 20;
        txtText.alignment = TextAnchor.MiddleLeft;
        txtText.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtText.verticalOverflow = VerticalWrapMode.Overflow;
        txtText.rectTransform.anchorMin = Vector2.zero;
        txtText.rectTransform.anchorMax = Vector2.one;
        txtText.rectTransform.pivot = new Vector2(0.0f, 0.5f);
        txtText.rectTransform.offsetMax = Vector2.zero;
        txtText.rectTransform.offsetMin = new Vector2(this.paramPlateSprite.rect.width + 10.0f, 0.0f);

        y -= this.paramPlateSprite.rect.height + 10.0f;

        ret.button      = btnBtn;
        ret.plate       = imgBtn;
        ret.thumbPlate  = imgPlate;
        ret.name        = txtText;

        return ret;
    }

    float IGNParamUICreator.AccumulateHeight(ParamBase.Type type, string widgetName, ref float y)
    {
        float plateHeight = this.paramPlateSprite.rect.height;

        switch(type)
        { 
            case ParamBase.Type.WireReference:
                plateHeight += paramReferenceUIHeightExtra;
                break;
        }

        y += plateHeight + 10.0f;
        return y;
    }

    void IGNParamUICreator.PerformAction(RectTransform invoker, string actionname)
    {
        string [] actPrts = 
            actionname.Split(
                new char []{'|' }, 
                System.StringSplitOptions.RemoveEmptyEntries);

        if(actPrts.Length == 0)
            return;

        string cmd = actPrts[0];

        RectTransform reticuleStart = null;
        RectTransform reticuleEnd = null;

        switch(cmd)
        { 
            // Flash reticule on the active wiring bar
            case "hlwiringselector":
                reticuleStart = invoker;
                reticuleEnd = this.alignNamePulldown;
                break;

            // Flash reticule on hamburger button
            case "hlwiringhamburger":
                reticuleStart = invoker;
                reticuleEnd = this.alignOptions;
                break;

            case "hlwiringcanvas":
                reticuleStart = invoker;
                reticuleEnd = this.wiringRegion.viewport;
                break;

            case "hlwiringoutput":
                reticuleStart = invoker;
                break;

            case "hlpreviewpiano":
                reticuleStart = invoker;
                reticuleEnd = this.keyboardTestArea;
                break;

            case "hlself":
                reticuleStart = null;
                reticuleEnd = invoker;
                break;

            case "hldel":
                reticuleStart = invoker;
                reticuleEnd = this.garbageDragRegion.rectTransform;
                break;

            case "hlpallete":
                reticuleStart = invoker;
                reticuleEnd = this.factoryPalletRegion;
                break;

            case "hlestop":
                reticuleStart = invoker;
                reticuleEnd = this.eStop.rectTransform;
                break;

            case "hlundo":
                reticuleStart = invoker;
                reticuleEnd = this.buttonUndo.targetGraphic.rectTransform;
                break;

            case "hlredo":
                reticuleStart = invoker;
                reticuleEnd = this.buttonRedo.targetGraphic.rectTransform;
                break;

            case "loaddocname":
                if(actPrts.Length > 1)
                { 
                    string internalDocname = actPrts[1];
                    // TODO:
                }
                break;

            case "selnode":
                if(actPrts.Length > 1)
                { 
                    string nodeGUID = actPrts[1];
                    foreach(KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
                    {
                        if(kvp.Key.GUID == nodeGUID)
                        {
                            reticuleStart = invoker;
                            reticuleEnd = kvp.Value.rectTransform;
                            break;
                        }
                    }
                }
                break;

            case "gotowire":
                if(actPrts.Length > 1)
                {
                    string wiringGUID = actPrts[1];
                    WiringDocument wd = this.App.Wirings.GetDocument(wiringGUID);
                    if(wd != null)
                        this.App.SetActiveDocument(wd, false);
                }
                break;

            case "gotonext":
                {
                    int idx = this.App.Wirings.GetActiveIndex();
                    if(idx > -1 && idx < this.App.Wirings.Count() - 1)
                    {
                        WiringDocument wd = this.App.Wirings.GetDocument(idx + 1);
                        this.App.SetActiveDocument(wd, false);
                    }
                }
                break;

            case "gotoprev":
                {
                    int idx = this.App.Wirings.GetActiveIndex();
                    if(idx != -1 && idx > 0)
                    {
                        WiringDocument wd = this.App.Wirings.GetDocument(idx - 1);
                        this.App.SetActiveDocument(wd, false);
                    }
                }
                break;

            case "hlkbtab":
                reticuleStart = invoker;
                reticuleEnd = this.App.keyboardTabText.rectTransform;
                break;

            case "url":
                { 
                    if(actPrts.Length > 2)
                    { 
                        string url = actPrts[1];
                        string descr = string.Empty;
                        if(actPrts.Length >= 3)
                            descr = actPrts[2];

                        this.App.OpenLink(url, descr, invoker);
                    }
                }
                break;

            case "playsound":
                if(actPrts.Length > 1)
                { 
                    string audioName = actPrts[1].ToLower();

                    if(audioName == "applause")
                    { 
                        this.App.keyboardPane.PlayAudio_PerfectApplause();
                    }
                }
                break;

        }

        if(reticuleEnd != null)
        { 
            if(reticuleStart != null)
            { 
                this.App.StartCoroutine(
                    this.App.TransitionReticule(
                        CanvasSingleton.canvas.GetComponent<RectTransform>(),
                        reticuleStart,
                        reticuleEnd,
                        null));
            }
            else
            { 
                Vector3 [] vecRetStart = new Vector3[4];
                reticuleEnd.GetWorldCorners(vecRetStart);

                Vector3 [] vecRetEnd = 
                    new Vector3 []
                    { 
                        vecRetStart[0] + new Vector3(-50.0f,    -50.0f,     0.0f),
                        vecRetStart[1] + new Vector3(-50.0f,    50.0f,      0.0f),
                        vecRetStart[2] + new Vector3(50.0f,     50.0f,      0.0f),
                        vecRetStart[3] + new Vector3(50.0f,     -500.0f,    0.0f)
                    };

                this.App.StartCoroutine(
                    this.App.TransitionReticule(
                        CanvasSingleton.canvas.GetComponent<RectTransform>(),
                        reticuleStart,
                        reticuleEnd,
                        null));
            }
        }
    }

    Font IGNParamUICreator.StandardFont => 
        this.App.uiFactory.inputFont.font;

    ParamThumbBase SetupParamEdit(
        WiringCollection collection,
        WiringDocument parentDocument,
        LLDNBase owner,
        ParamBase param, 
        ParameterPeice paramPeice, 
        GameObject thumbPrefab, 
        GameObject editPrefab)
    {
        GameObject go = 
            GameObject.Instantiate<GameObject>(
                thumbPrefab, 
                paramPeice.thumbPlate.rectTransform);

        ParamThumbBase ptb = go.GetComponent<ParamThumbBase>();
        if (ptb != null) 
        {
            ptb.SetRectTransformInParent();

            switch(param.type)
            { 
                case ParamBase.Type.Bool:
                    ParamBool paramBool = param as ParamBool;
                    ptb.SetParamBool(this, paramBool);
                    break;

                case ParamBase.Type.Enum:
                    ParamEnum paramEnum = param as ParamEnum;
                    ptb.SetParamEnum(this, paramEnum);
                    break;

                case ParamBase.Type.Float:
                    ParamFloat paramFloat = param as ParamFloat;
                    ptb.SetParamFloat(this, paramFloat);
                    break;

                case ParamBase.Type.Int:
                    ParamInt paramInt = param as ParamInt;
                    ptb.SetParamInt(this, paramInt);
                    break;

                case ParamBase.Type.TimeLen:
                    ParamTimeLen paramTl = param as ParamTimeLen;
                    ptb.SetParamTimeLen(this, paramTl);
                    break;

                case ParamBase.Type.Nickname:
                    ParamNickname paramN = param as ParamNickname;
                    ptb.SetParamNickname(this, paramN);
                    break;

                case ParamBase.Type.WireReference:
                    ParamWireReference paramWRef = param as ParamWireReference;
                    ptb.SetParamWireReference(this, paramWRef);
                    break;
            }
        }

        paramPeice.button.onClick.AddListener(
            () =>
            {
                this.ClearDragState();

                GameObject goBase = GameObject.Instantiate<GameObject>(this.paramEditRoot, CanvasSingleton.canvas.transform);
                EditParamDlgRoot editRoot = goBase.GetComponent<EditParamDlgRoot>();
                RectTransform rtParamEdit = editRoot.GetComponent<RectTransform>();

                this.App.DoVibrateButton();
                ModalStack.AddToStack(goBase, ()=>{editRoot.ResetToOriginalValue(); });

                editRoot.Initialize(
                    this,
                    collection,
                    parentDocument,
                    this.App.keyboardPane.keyAssets, 
                    this.App.keyboardPane.keyDims, 
                    owner,
                    param, 
                    this.App, 
                    this, ptb, 
                    editPrefab, 
                    paramPeice.plate);

                rtParamEdit.anchorMin = Vector2.zero;
                rtParamEdit.anchorMax = Vector2.one;
                rtParamEdit.offsetMin = Vector2.zero;
                rtParamEdit.offsetMax = Vector2.zero;
                rtParamEdit.pivot = new Vector2(0.0f, 1.0f);

                this.App.RefreshActiveOutput();

                this.App.StartCoroutine(
                    this.App.TransitionReticule(
                        this.canvas.GetComponent<RectTransform>(),
                        paramPeice.plate.rectTransform,
                        rtParamEdit,
                        editRoot.GetComponent<UnityEngine.UI.Graphic>()));

                editRoot.onClose += 
                    ()=>
                    { 
                        this.App.StartCoroutine(
                            this.App.TransitionReticule(
                                this.canvas.GetComponent<RectTransform>(),
                                rtParamEdit,
                                paramPeice.plate.rectTransform, 
                                null));
                    };
            });

        return ptb;
    }

    ParamUIUpdater IGNParamUICreator.CreateFloatRangeSlider(
        RectTransform rt, 
        ref float y, 
        LLDNBase genNode, 
        ParamFloat param)
    {
        ParameterPeice pp = 
            GenerateParameterPeice(
                param.name, 
                rt, 
                ref y, 
                genNode);

        ParamThumbBase ptb = 
            this.SetupParamEdit(
                this.App.Wirings,
                this.viewingDocument,
                genNode,
                param, 
                pp, 
                this.paramThumbSlider, 
                this.paramEditSlider);

        return new ParamEditUIUpdater(pp.plate.rectTransform, ptb);
    }

    Sprite IGNParamUICreator.GetIcon(string name)
    { 
        if(name == "link")
            return this.nodeicoLink;
        else if(name == "video")
            return this.nodeicoVideo;

        return null;
    }

    ParamUIUpdater IGNParamUICreator.CreateIntRangeSlider(
        RectTransform rt, 
        ref float y, 
        LLDNBase genNode, 
        ParamInt param)
    {
        ParameterPeice pp = GenerateParameterPeice(param.name, rt, ref y, genNode);

        ParamThumbBase ptb = 
            this.SetupParamEdit(
                this.App.Wirings,
                this.viewingDocument,
                genNode,
                param, 
                pp, 
                this.paramThumbSlider, 
                this.paramEditSlider);

        return new ParamEditUIUpdater(pp.plate.rectTransform, ptb);
    }

    ParamUIUpdater IGNParamUICreator.CreateTimeLenEdit(
        RectTransform rt, 
        ref float y, 
        LLDNBase genNode, 
        ParamTimeLen param)
    {
        ParameterPeice pp = GenerateParameterPeice(param.name, rt, ref y, genNode);

        ParamThumbBase ptb = 
            this.SetupParamEdit(
                this.App.Wirings,
                this.viewingDocument,
                genNode,
                param, 
                pp, 
                this.paramThumbFreq, 
                this.paramEditSlider);

        return new ParamEditUIUpdater(pp.plate.rectTransform, ptb);
    }

    ParamUIUpdater IGNParamUICreator.CreateFloatTime(
        RectTransform rt, 
        ref float y, 
        LLDNBase genNode, 
        ParamFloat param)
    {
        ParameterPeice pp =
            GenerateParameterPeice(
                param.name,
                rt,
                ref y,
                genNode);

        ParamThumbBase ptb = 
            this.SetupParamEdit(
                this.App.Wirings,
                this.viewingDocument,
                genNode,
                param, 
                pp, 
                this.paramThumbSlider, 
                this.paramEditSlider);

        return new ParamEditUIUpdater(pp.plate.rectTransform, ptb);
    }

    ParamUIUpdater IGNParamUICreator.CreateFloatClampedDial(
        RectTransform rt, 
        ref float y, 
        LLDNBase genNode, 
        ParamFloat param)
    {
        ParameterPeice pp = 
            GenerateParameterPeice(
                param.name,
                rt,
                ref y,
                genNode);

        ParamThumbBase ptb = 
            this.SetupParamEdit(
                this.App.Wirings,
                this.viewingDocument,
                genNode,
                param, 
                pp, 
                this.paramThumbDial, 
                this.paramEditDial);

        return new ParamEditUIUpdater(pp.plate.rectTransform, ptb);
    }

    ParamUIUpdater IGNParamUICreator.CreateEnumPulldown(RectTransform rt, ref float y, LLDNBase genNode, ParamEnum param)
    {
        ParameterPeice pp =
            GenerateParameterPeice(
                param.name,
                rt,
                ref y,
                genNode);

        GameObject goEnumText = new GameObject("EnumText");
        goEnumText.transform.SetParent(pp.thumbPlate.transform, false);
        UnityEngine.UI.Text txt = goEnumText.AddComponent<UnityEngine.UI.Text>();
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;
        txt.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        txt.text = param.GetLabel();
        txt.color = Color.black;
        txt.fontSize = 10;
        txt.font = this.dragLabelFont;
        txt.alignment = TextAnchor.MiddleCenter;

        pp.button.onClick.AddListener(
            ()=>
            { 
                PxPre.DropMenu.Props dropProp = 
                    PxPre.DropMenu.DropMenuSingleton.MenuInst.props;

                Color cs = dropProp.selectedColor;
                Color us = dropProp.unselectedColor;

                int orig = param.value;
                PxPre.DropMenu.StackUtil menuStack = new PxPre.DropMenu.StackUtil(param.name);
                foreach(EnumVals.Entry ee in param.enumVals.entries)
                {
                    Sprite ico = null;
                    if (param.value == ee.val)
                        ico = this.menuSelIcon;


                    EnumVals.Entry cpy = ee;
                    menuStack.AddAction(
                        ico,
                        ico != null ? cs : us,
                        cpy.label, 
                        ()=>
                        { 
                            string originalValue = param.GetStringValue();

                            param.SetValueFromString(cpy.id); 
                            txt.text = param.GetLabel(); 

                            this.App.SetLLDAWParamValue(
                                genNode,
                                param,
                                originalValue,
                                param.GetStringValue());
                        });
                }

                this.wiringRegion.velocity = Vector2.zero;

                PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
                    CanvasSingleton.canvas,
                    menuStack.Root,
                    pp.plate.rectTransform);
            });

        return new EnumUIUpdater(pp.plate.rectTransform, txt, param);
    }

    ParamUIUpdater IGNParamUICreator.CreateBoolSwitch(RectTransform rt, ref float y, LLDNBase genNode, ParamBool param)
    {
        ParameterPeice pp = GenerateParameterPeice(param.name, rt, ref y, genNode);

        GameObject go = GameObject.Instantiate<GameObject>(this.paramThumbToggle, pp.thumbPlate.rectTransform);
        ParamThumbToggle tog = go.GetComponent<ParamThumbToggle>();
        tog.trueText = param.trueString;
        tog.falseText = param.falseString;

        tog.SetRectTransformInParent();

        tog.SetParamBool(this, param);

        pp.button.onClick.AddListener( 
            ()=> 
            { 
                string originalValue = param.GetStringValue();
                tog.SetValue(!param.value); 

                this.App.SetLLDAWParamValue(
                    genNode,
                    param,
                    originalValue,
                    param.GetStringValue());


                this.App.DoVibrateButton();
            });

        return new ToggleUIUpdater(pp.plate.rectTransform, param, tog);
    }

    void UpdateWireReferenceText(UnityEngine.UI.Text text, ParamWireReference param)
    { 
        string refID = param.referenceGUID;

        WiringDocument wd = null;

        if(string.IsNullOrEmpty(refID) == false)
            wd = this.App.Wirings.GetDocument(refID);

        if(wd == null)
            text.text = "--";
        else
        {
            string procName = wd.GetProcessedWiringName(this.App.Wirings);
            text.text = procName;
        }
    }

    ParamUIUpdater IGNParamUICreator.CreateWiringReference(RectTransform rt, ref float y, LLDNBase genNode, ParamWireReference param)
    { 
        // We add 10 because that's what GenerateParameterPeice() does
        float plateHeight = this.paramPlateSprite.rect.height + 10.0f;
        //paramReferenceUIHeightExtra
        WireReferenceEntry wireRefEnt = new WireReferenceEntry();

        GameObject goTitleText = new GameObject("WireRefText");
        goTitleText.transform.SetParent(rt.transform, false);
        UnityEngine.UI.Text txtText = goTitleText.AddComponent<UnityEngine.UI.Text>();
        txtText.font = this.App.uiFactory.inputFont.font;
        txtText.fontSize = 20;
        txtText.color = Color.black;
        txtText.verticalOverflow = VerticalWrapMode.Overflow;
        txtText.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtText.alignment = TextAnchor.LowerLeft;
        txtText.text = param.name;
        RectTransform rtText = txtText.rectTransform; 
        rtText.anchorMin = new Vector2(0.0f, 1.0f);
        rtText.anchorMax = new Vector2(1.0f, 1.0f);
        rtText.pivot = new Vector2(0.0f, 0.0f);
        rtText.offsetMin = new Vector2(5.0f, y - paramReferenceUIHeightExtra);
        rtText.offsetMax = new Vector2(-5.0f, y);
        
        y -= paramReferenceUIHeightExtra;

        GameObject goPulldown = new GameObject("WireRefPulldown");
        goPulldown.transform.SetParent(rt, false);
        UnityEngine.UI.Image imgPulldown = goPulldown.AddComponent<UnityEngine.UI.Image>();
        RectTransform rtImgPulldown = imgPulldown.rectTransform;
        rtImgPulldown.anchorMin = new Vector2(0.0f, 1.0f);
        rtImgPulldown.anchorMax = new Vector2(1.0f, 1.0f);
        rtImgPulldown.pivot = new Vector2(0.0f, 1.0f);
        rtImgPulldown.offsetMin = new Vector2(5.0f, y - plateHeight);
        rtImgPulldown.offsetMax = new Vector2(-5.0f, y);
        imgPulldown.sprite = this.paramContainerSprite;
        imgPulldown.type = Type.Sliced;
        
        GameObject goPulldownArrow = new GameObject("WireRefArrow");
        goPulldownArrow.transform.SetParent(goPulldown.transform, false);
        UnityEngine.UI.Image imgPulldownArrow = goPulldownArrow.AddComponent<UnityEngine.UI.Image>();
        Sprite downSprite = this.App.exerciseAssets.pulldownArrow;
        Vector2 downSpriteSz = downSprite.rect.size;
        imgPulldownArrow.sprite = downSprite;
        float arrowReserved = downSpriteSz.x + 10.0f;
        RectTransform rtPulldownArrow = imgPulldownArrow.rectTransform;
        rtPulldownArrow.anchorMin = new Vector2(1.0f, 0.5f);
        rtPulldownArrow.anchorMax = new Vector2(1.0f, 0.5f);
        rtPulldownArrow.pivot = new Vector2(0.5f, 0.5f);
        rtPulldownArrow.anchoredPosition = new Vector2(-arrowReserved * 0.5f, 0.0f);
        rtPulldownArrow.sizeDelta = downSpriteSz;
        
        GameObject goPulldownText = new GameObject("WireRefText");
        goPulldownText.transform.SetParent(goPulldown.transform, false);
        UnityEngine.UI.Text txtPulldownText = goPulldownText.AddComponent<UnityEngine.UI.Text>();
        RectTransform rtPulldownText = txtPulldownText.rectTransform;
        rtPulldownText.anchorMin = Vector2.zero;
        rtPulldownText.anchorMax = Vector2.one;
        rtPulldownArrow.pivot = new Vector2(0.5f, 0.5f);
        rtPulldownText.offsetMin = new Vector2(0.0f, 0.0f);
        rtPulldownText.offsetMax = new Vector2(-arrowReserved, 0.0f);
        this.App.uiFactory.ApplyTextStyle(txtPulldownText, 14, "--");
        txtPulldownText.alignment = TextAnchor.MiddleCenter;
        
        wireRefEnt.entryText = txtPulldownText;
        wireRefEnt.param = param;
        wireRefEnt.pulldownRect = rtImgPulldown;
        wireRefEnt.node = genNode;

        List<WireReferenceEntry> lstWRE;
        if(this.wireReferencesUIData.TryGetValue(genNode, out lstWRE) == false)
        { 
            lstWRE = new List<WireReferenceEntry>();
            this.wireReferencesUIData.Add(genNode, lstWRE);
        }
        lstWRE.Add(wireRefEnt);

        UnityEngine.UI.Button btnPulldown = goPulldown.AddComponent<UnityEngine.UI.Button>();
        btnPulldown.targetGraphic = imgPulldown;
        btnPulldown.onClick.AddListener(()=>{this.DoPanePulldown(wireRefEnt); });
        this.UpdateWireReferenceText(txtPulldownText, param);
        y -= plateHeight;

        return new WiringRefUIUpdater(rtImgPulldown, param, txtPulldownText);
    }

    void DoPanePulldown(WireReferenceEntry wre)
    { 
        List<WiringDocument> lst = new List<WiringDocument>();
        foreach(WiringDocument wd in this.App.Wirings.Documents)
        { 
            if(wre.param.cannotCycle == true)
            {
                if(wd.ReferencesDocument(this.viewingDocument, true) == true)
                    continue;

                lst.Add(wd);
            }
        }


        PxPre.DropMenu.StackUtil stackUtil = new PxPre.DropMenu.StackUtil();
        foreach(WiringDocument wd in lst)
        { 
            WiringDocument wdCpy = wd;
            Sprite icon;
            this.catIconLookups.TryGetValue(wd.category, out icon);
            stackUtil.AddAction(
                icon,
                wdCpy.GetProcessedWiringName(this.App.Wirings), 
                ()=>
                { 
                    if(wre.param.referenceGUID == wd.guid)
                        return;

                    string originalValue = wre.param.referenceGUID;

                    wre.param.SetValueFromString(wd.guid);
                    this.UpdateWireReferenceText(wre.entryText, wre.param);
                    this.FlagDirty(Dirty.NodeOutputStyle);

                    this.App.SetLLDAWParamValue(
                        wre.node, 
                        wre.param, 
                        originalValue, 
                        wre.param.GetStringValue());

                    // Dunno if this is actullay connected to the output without checking
                    // the hierarchy, just whatever, just blindly refresh it, the overhead
                    // won't be noticeable.
                    if(this.viewingDocument.Output != null)
                        this.App.NotifyActiveOutputUpdated(this.viewingDocument.Output);

                    // TODO: UNDO
                    //this.App.Wirings.SetDirty();
                });
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stackUtil.Root,
            wre.pulldownRect);
    }

    UnityEngine.UI.Image IGNParamUICreator.CreatePCMConnection(RectTransform rt, ref float y, LLDNBase genNode, ParamConnection param)
    { 
        const float widgetSize = 50.0f;

        Vector2 spriteSz = this.socketArrowSprite.rect.size;

        GameObject goArrow = new GameObject("Input");
        goArrow.transform.SetParent(rt);
        goArrow.transform.localRotation = Quaternion.identity;
        goArrow.transform.localScale = Vector3.one;
        UnityEngine.UI.Image img = goArrow.AddComponent<UnityEngine.UI.Image>();
        img.sprite = this.socketArrowSprite;
        img.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        img.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
        img.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        img.rectTransform.sizeDelta = spriteSz;
        img.rectTransform.anchoredPosition = 
            new Vector2(
                5.0f + spriteSz.x * 0.5f, 
                y - (widgetSize - spriteSz.y) * 0.5f - spriteSz.y * 0.5f);
        //img.rectTransform.offsetMin = new Vector2(0.0f, 0.0f);
        //img.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);

        GameObject goName = new GameObject("Name");
        goName.transform.SetParent(rt);
        goName.transform.localRotation = Quaternion.identity;
        goName.transform.localScale = Vector3.one;
        UnityEngine.UI.Text txtName = goName.AddComponent<UnityEngine.UI.Text>();
        txtName.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
        txtName.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        txtName.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        txtName.rectTransform.offsetMin = new Vector2(this.socketArrowSprite.rect.width + 15.0f, y - widgetSize);
        txtName.rectTransform.offsetMax = new Vector2(0.0f, y );
        txtName.alignment = TextAnchor.MiddleLeft;
        txtName.text = param.name;
        txtName.color = Color.black;
        txtName.fontSize = 20;
        txtName.font = this.dragLabelFont;
        txtName.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtName.verticalOverflow = VerticalWrapMode.Overflow;

        SocketUIInput socketInput = goArrow.AddComponent<SocketUIInput>();
        socketInput.parentPane = this;
        socketInput.connection = param;
        socketInput.targetGraphic = img;
        socketInput.host = rt.GetComponent<GNUIHost>(); // This could probbly be done more rigerously

        y -= widgetSize;

        return img;
    }

    ParamUIUpdater IGNParamUICreator.CreateNickname(RectTransform rt, ref float y, LLDNBase genNode, ParamNickname param)
    {
        ParameterPeice pp =
            GenerateParameterPeice(
                param.name,
                rt,
                ref y,
                genNode);

        ParamThumbBase thumb =
            this.SetupParamEdit(
                this.App.Wirings,
                this.viewingDocument,
                genNode,
                param,
                pp,
                this.paramThumbNickname,
                this.paramEditNick);

        return new ParamEditUIUpdater(pp.plate.rectTransform, thumb);
    }

    void IGNParamUICreator.ConnectToOutput(LLDNQuickOut quickOutToConnect)
    { 
        if(this.viewingDocument == null)
            return;

        LLDNOutput output = this.viewingDocument.Output;
        if(output == null)
            return;

        ParamConnection inputOfOutput = output.GetConnectionParam("Input");
        if(inputOfOutput.IsConnected(quickOutToConnect) == true)
            return;

        this.App.RewireNodeAudio(
            this.viewingDocument,
            quickOutToConnect, 
            output, 
            inputOfOutput);
    }

    static bool PointInsideRectTransform(Vector3 pt, RectTransform rt)
    {
        Vector3[] cs = new Vector3[4];
        rt.GetWorldCorners(cs);

        if (pt.x >= cs[0].x &&
            pt.x <= cs[2].x &&
            pt.y >= cs[0].y &&
            pt.y <= cs[2].y)
        { 
            return true;
        }
        return false;
    }
    
    // TODO: Move this to app handling?
    public void SetNodeOutputStyleDirty()
    { 
        if(this.processingOutputStyles != null)
            return;

        this.processingOutputStyles = 
            this.App.StartCoroutine(this.DirtyNodeOutputStyleEnum());
    }

    /// <summary>
    /// Go through the entire shown wiring diagram and figure out what should be 
    /// colored and filled in to visualize to the user what's apart of the network 
    /// connected to the output, and what's orphaned.
    /// </summary>
    /// <returns>Coroutine iterator block.</returns>
    IEnumerator DirtyNodeOutputStyleEnum()
    { 
        yield return new WaitForEndOfFrame();

        this.processingOutputStyles = null;
        if(this.viewingDocument == null)
            yield break;

        const float inactiveHermiteAlpha = 0.8f;
        const float inactiveNodeAlpha = 0.5f;

        HashSet<LLDNBase> involved = new HashSet<LLDNBase>();
        List<HermiteLink> links = new List<HermiteLink>( this.linkCurves);

        LLDNOutput activeOut = this.viewingDocument.Output;
        if (activeOut != null)
        { 
            involved.Add(activeOut);

            while(true)
            { 
                bool changed = false;

                for(int i = links.Count - 1; i >= 0; --i)
                {
                    if(involved.Contains(links[i].nodeInput) == true)
                    { 
                        if(links[i].nodeInput.VerifyConnectionUsed(links[i].inputParam) == false)
                            continue;

                        if (involved.Add(links[i].nodeOutput) == true)
                            changed = true;

                        links[i].uiCurve.color = links[i].nodeOutput.GetEdgeColor();
                        links.RemoveAt(i);
                    }
                }

                if(changed == false)
                    break;
            }
        }

        // The links left after doing the checks above are the wiring connections
        // that weren't apart of the output network.
        foreach (HermiteLink hl in links)
            hl.uiCurve.color = new Color(1.0f, 1.0f, 1.0f, inactiveHermiteAlpha);

        foreach (KeyValuePair< LLDNBase, GNUIHost > kvp in this.uiNodes)
        { 
            GNUIHost gui = kvp.Value;
            if(involved.Contains(kvp.Key) == true)
            {
                gui.icon.color = Color.white;
                gui.title.fontStyle = FontStyle.Bold;
            }
            else
            {
                gui.icon.color = new Color(1.0f, 1.0f, 1.0f, inactiveNodeAlpha);
                gui.title.fontStyle = FontStyle.Normal;
            }
        }
    }

    /// <summary>
    /// Show the trash area that the user can click to delete the selected node, 
    /// or drag node UI hosts over to delete.
    /// 
    /// The area should only be shown when a node is selected or being dragged.
    /// </summary>
    public void ShowTrashArea()
    { 
        this.deleteNodeImg.gameObject.SetActive(true);

        this.deleteNodeImg.rectTransform.offsetMax = new Vector2(0.0f, trashAreaHeight);
        this.factoryPalletRegion.offsetMin = new Vector2(0.0f, trashAreaHeight);
    }

    /// <summary>
    /// Hide the trash region. It should not be shown is the user is not dragging
    /// anything, and isn't dragging anything.
    /// </summary>
    public void HideTrashRegion()
    {
        this.deleteNodeImg.gameObject.SetActive(false);

        this.deleteNodeImg.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);
        this.factoryPalletRegion.offsetMin = new Vector2(0.0f, 0.0f);
    }

    /// <summary>
    /// Create the crosshair (if not created) and move it to be
    /// be visible under a specified parent.
    /// </summary>
    /// <param name="rt">The parent to show the crosshair under.</param>
    void ApplyCrosshair(RectTransform rt)
    { 
        RectTransform rtCS = this.EnsureCrosshair();
        if(rt == null)
        {
            rtCS.SetParent(null);
            rtCS.gameObject.SetActive(false);
        }

        rtCS.SetParent(rt);
        rtCS.gameObject.SetActive(true);
        rtCS.localRotation = Quaternion.identity;
        rtCS.localScale = Vector3.one;
        rtCS.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Ensure the crosshair exists.
    /// </summary>
    /// <returns>The crosshair object, either the current existing crosshair object,
    /// or a newly created one if a previous one didn't exist.</returns>
    RectTransform EnsureCrosshair()
    { 
        if(this.crosshair != null)
            return this.crosshair;

        const float crossZone = 10.0f;
        const float hairHalfWidth = 1.0f;
        const float hairLen = 50.0f;

        // Create the root parent for the 4 ticks creating the crosshair
        GameObject crosshairRoot = new GameObject();
        RectTransform rt = crosshairRoot.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.zero;

        // Create the ticks for the crosshair
        GameObject goTop = new GameObject();
        goTop.transform.SetParent(rt, false);
        UnityEngine.UI.Image imgTop = goTop.AddComponent<UnityEngine.UI.Image>();
        imgTop.color = Color.green;
        RectTransform rtTop = imgTop.rectTransform;
        rtTop.anchorMin = Vector2.zero;
        rtTop.anchorMax = Vector2.zero;
        rtTop.pivot = new Vector2(0.5f, 0.5f);
        rtTop.offsetMin = new Vector2(-hairHalfWidth, crossZone);
        rtTop.offsetMax = new Vector2(hairHalfWidth, crossZone + hairLen);

        GameObject goBot = new GameObject();
        goBot.transform.SetParent(rt, false);
        UnityEngine.UI.Image imgBot = goBot.AddComponent<UnityEngine.UI.Image>();
        imgBot.color = Color.green;
        RectTransform rtBot = imgBot.rectTransform;
        rtBot.anchorMin = Vector2.zero;
        rtBot.anchorMax = Vector2.zero;
        rtBot.pivot = new Vector2(0.5f, 0.5f);
        rtBot.offsetMin = new Vector2(-hairHalfWidth, -crossZone - hairLen);
        rtBot.offsetMax = new Vector2(hairHalfWidth, -crossZone);

        GameObject goLeft = new GameObject();
        goLeft.transform.SetParent(rt, false);
        UnityEngine.UI.Image imgLeft = goLeft.AddComponent<UnityEngine.UI.Image>();
        imgLeft.color = Color.green;
        RectTransform rtLeft = imgLeft.rectTransform;
        rtLeft.anchorMin = Vector2.zero;
        rtLeft.anchorMax = Vector2.zero;
        rtLeft.pivot = new Vector2(0.5f, 0.5f);
        rtLeft.offsetMin = new Vector2(-crossZone - hairLen, -hairHalfWidth);
        rtLeft.offsetMax = new Vector2(-crossZone, hairHalfWidth);

        GameObject goRight = new GameObject();
        goRight.transform.SetParent(rt, false);
        UnityEngine.UI.Image imgRight = goRight.AddComponent<UnityEngine.UI.Image>();
        imgRight.color = Color.green;
        RectTransform rtRight = imgRight.rectTransform;
        rtRight.anchorMin = Vector2.zero;
        rtRight.anchorMax = Vector2.zero;
        rtRight.pivot = new Vector2(0.5f, 0.5f);
        rtRight.offsetMin = new Vector2(crossZone, -hairHalfWidth);
        rtRight.offsetMax = new Vector2(crossZone + hairLen, hairHalfWidth);

        this.crosshair = rt;
        return this.crosshair;
    }

    void ShowNodeDialog(LLDNBase.NodeType nodeType, RectTransform rtInvoker)
    { 
        NodeInfo ni = this.GetNodeInfo(nodeType);

        // CREATE DIALOG
        PxPre.UIL.Dialog dlg = this.App.dlgSpawner.CreateDialogTemplate(new Vector2(650.0f, 0.0f), PxPre.UIL.LFlag.AlignCenter, 1.0f);
        dlg.dialogSizer.border = new PxPre.UIL.PadRect(20.0f);

        // CREATE TITLE
        PxPre.UIL.UILStack uiTitle = new PxPre.UIL.UILStack(this.App.uiFactory, dlg.rootTitle, dlg.titleSizer);
        uiTitle.PushHorizSizer(0.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz);
            uiTitle.AddSpace(10.0f, 0.0f, 0);
            uiTitle.AddImage(ni.icon, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiTitle.AddSpace(0.0f, 1.0f, 0);
            uiTitle.AddText(ni.label, 25, false, 0.0f, PxPre.UIL.LFlag.AlignCenter);
            uiTitle.AddSpace(0.0f, 1.0f, 0);
        uiTitle.Pop();

        // SETUP INFORMATION
        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons( new PxPre.UIL.DlgButtonPair("Close", null));
        LLDNBase gbTmp = this.CreateGenerator(nodeType, false, true);

        // START BODY
        PxPre.UIL.UILStack uiStack = 
            new PxPre.UIL.UILStack(
                this.App.uiFactory, 
                dlg.rootParent, 
                dlg.contentSizer);

        const float sectionSpacing = 40.0f;

        uiStack.PushImage(this.App.exerciseAssets.scrollRectBorder, 1.0f, PxPre.UIL.LFlag.Grow).Chn_SetImgType(Type.Sliced).Chn_SetImgFillCenter(false);
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);

            PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> scrollRgn = 
                uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.Grow);

            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(10.0f, 0.0f, 10.0f, 0.0f);

                uiStack.AddText(
                    "<i>This documentation was brought up by tapping on this node's button from the node list. To create an instance of it, drag-and-drop the button into the wiring region.</i>", 
                    14, 
                    true, 
                    0.0f, 
                    PxPre.UIL.LFlag.GrowHoriz);

                uiStack.AddVertSpace(20.0f, 0.0f, 0);
                
                uiStack.AddText("<b>Description:</b>", 30, false, 0.0f, 0);
                uiStack.AddHorizontalSeparator(0.0f);
                uiStack.AddSpace(5.0f, 0.0f, 0);
                uiStack.AddText(gbTmp.description, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                
                uiStack.AddVertSpace(sectionSpacing, 0.0f, 0);
                
                uiStack.AddText("<b>Parameters:</b>", 30, false, 0.0f, 0);
                uiStack.AddHorizontalSeparator(0.0f);
                uiStack.AddSpace(5.0f, 0.0f, 0);
                uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                    foreach(ParamBase pb in gbTmp.nodeParams)
                    {
                        if(pb.editable == false)
                            continue;

                        uiStack.AddText($"<b>{pb.name}</b>", false, 0.0f, 0);
                
                        uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.Grow);
                            uiStack.AddSpace(10.0f, 0.0f, 0);
                            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                                if(string.IsNullOrEmpty(pb.unit) == false)
                                    uiStack.AddText($"<i><size=16>Unit: {pb.unit}</size></i>", true, 0.0f, PxPre.UIL.LFlag.Grow);    
                                uiStack.AddText(pb.description, true, 1.0f, PxPre.UIL.LFlag.Grow);
                            uiStack.Pop();
                        uiStack.Pop();
                    }
                uiStack.Pop();
                
                uiStack.AddVertSpace(sectionSpacing, 0.0f, 0);
                
                uiStack.AddText("<b>Category:</b>", 30, false, 0.0f, 0);
                uiStack.AddHorizontalSeparator(0.0f);
                uiStack.AddSpace(5.0f, 0.0f, 0);
                uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                    PxPre.UIL.EleText catText = uiStack.AddText($"<b>{GetCategoryName(gbTmp.GetCategory())}</b>", false, 0.0f, 0);
                    uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.Grow);
                        uiStack.AddSpace(10.0f, 0.0f, 0);
                        uiStack.AddText(GetCategoryDescription(gbTmp.GetCategory()), true, 1.0f, PxPre.UIL.LFlag.Grow);
                    uiStack.Pop();
                uiStack.Pop();
            uiStack.AddSpace(0.0f, 1.0f, PxPre.UIL.LFlag.Grow);

            uiStack.Pop();
            uiStack.Pop();
        uiStack.Pop();
        uiStack.Pop();
        uiStack.AddSpace(5.0f, 0.0f, 0);

        Color catCol = LLDNBase.GetCategoryColor(gbTmp.GetCategory());
        catText.text.color = Color.Lerp(catCol, Color.black, 0.5f);

        DoNothing dn = dlg.rootParent.RT.gameObject.AddComponent<DoNothing>();
        dn.StartCoroutine( PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(scrollRgn.ScrollRect, 1.0f));

        dlg.host.LayoutInRTSmartFit();
        this.App.SetupDialogForTransition(dlg, rtInvoker, true, true);
    }

    void ShowCategoryDialog(LLDNBase.Category category, RectTransform rtInvoker)
    { 
        // CREATE DIALOG
        PxPre.UIL.Dialog dlg = this.App.dlgSpawner.CreateDialogTemplate(new Vector2(650.0f, 0.0f), PxPre.UIL.LFlag.AlignCenter, 1.0f);
        dlg.dialogSizer.border = new PxPre.UIL.PadRect(20.0f);

        Color catCol = LLDNBase.GetCategoryColor(category);
        Color titleCol = Color.Lerp(catCol, Color.white, 0.75f);
        Color bodyCol = Color.Lerp(catCol, Color.white, 0.9f);
        UnityEngine.UI.Graphic gTitle = dlg.rootTitle.RT.GetComponent<UnityEngine.UI.Graphic>();
        gTitle.color = titleCol;
        UnityEngine.UI.Graphic gBody = dlg.rootParent.RT.GetComponent<UnityEngine.UI.Graphic>();
        gBody.color = bodyCol;

        // CREATE TITLE
        dlg.AddDialogTemplateTitle(GetCategoryName(category) );

        // SETUP INFORMATION
        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(new PxPre.UIL.DlgButtonPair("Close", null));

        // START BODY
        PxPre.UIL.UILStack uiStack =
            new PxPre.UIL.UILStack(
                this.App.uiFactory,
                dlg.rootParent,
                dlg.contentSizer);

        uiStack.PushImage(this.App.exerciseAssets.scrollRectBorder, 1.0f, PxPre.UIL.LFlag.Grow).Chn_SetImgType(Type.Sliced).Chn_SetImgFillCenter(false);
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);

        PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> scrollRgn = 
            uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.Grow);

        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(10.0f, 0.0f, 10.0f, 0.0f);

        uiStack.AddText(
            "<i>This documentation was brought up by tapping on this category's header from the node list. Drag and drop node icons to instance them.</i>",
            14,
            true,
            0.0f,
            PxPre.UIL.LFlag.GrowHoriz);

        uiStack.AddVertSpace(20.0f, 0.0f, 0);

        uiStack.AddText("<b>Description:</b>", 30, false, 0.0f, 0);
        uiStack.AddHorizontalSeparator(0.0f);
        uiStack.AddSpace(5.0f, 0.0f, 0);

        string categoryDescription = 
            GetCategoryDescription(category);

        uiStack.AddText(categoryDescription, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);

        uiStack.Pop();
        uiStack.Pop();
        uiStack.Pop();
        uiStack.Pop();
        uiStack.AddSpace(5.0f, 0.0f, 0);

        foreach(PxPre.UIL.Dialog.OptionsButton ob in dlg.options)
            ob.button.targetGraphic.color = bodyCol;

        DoNothing dn = dlg.rootParent.RT.gameObject.AddComponent<DoNothing>();
        dn.StartCoroutine(PxPre.UIL.EleVertScrollRgn.SetVertScrollLater(scrollRgn.ScrollRect, 1.0f));

        dlg.host.LayoutInRTSmartFit();
        this.App.SetupDialogForTransition(dlg, rtInvoker, true, true);
    }

    static string GetCategoryName(LLDNBase.Category cat)
    { 
        switch(cat)
        { 
            case LLDNBase.Category.Combines:
                return "Combines";

            case LLDNBase.Category.Envelopes:
                return "Envelopes";

            case LLDNBase.Category.Operations:
                return "Operations";

            case LLDNBase.Category.Special:
                return "Special";

            case LLDNBase.Category.Voices:
                return "Voices";

            case LLDNBase.Category.Wave:
                return "Wave";
        }

        return "GetCategoryName() UNKNOWN";
    }

    static string GetCategoryDescription(LLDNBase.Category cat)
    {
        switch (cat)
        {
            case LLDNBase.Category.Combines:
                return "A category of nodes that combines multiple audio signals";

            case LLDNBase.Category.Envelopes:
                return "A category of nodes that raises or lowers the volume of an audio signal over time to simulate how instruments are played.";

            case LLDNBase.Category.Operations:
                return "A category of nodes that modifies an audio signal.";

            case LLDNBase.Category.Special:
                return "A category of special and misc nodes.";

            case LLDNBase.Category.Voices:
                return "A category of nodes that plays an audio signal multiple times over itself with offset properties.";

            case LLDNBase.Category.Wave:
                return "A category of nodes that generates an audio signal, <i>usually</i> an audible tone.";
        }

        return "GetCategoryDescription() UNHANDLED";
    }

    void IWiringEditorBridge.FlagOutputNetworkDirty()
    { 
        this.FlagDirty( Dirty.NodeOutputStyle);
    }

    public bool ViewedWiringMakesNoise()
    {
        if(this.viewingDocument == null)
            return false;

        return this.viewingDocument.GuessMakesNoise(this.App.Wirings, null);
    }

    public void OnButton_Undo()
    { 
        this.App.Undo();
    }

    public void OnButton_Redo()
    { 
        this.App.Redo();
    }

    public void UpdateUndosButtons(PxPre.Undo.UndoSession undos)
    { 
        this.buttonUndo.interactable = undos.HasUndos();
        this.buttonRedo.interactable = undos.HasRedos();
    }

    protected override void AppSink_OnWiringRenamed(WiringCollection collection, WiringDocument wd)
    {
        if(this.App.IsInUndoSession() == false)
        { 
            this.QueueHighlight(
                ThingToHighlight.CreateHLNodePlate(wd));
        }

        if(wd != this.viewingDocument)
            return;

        this.pulldownDocumentText.text = 
            wd.GetProcessedWiringName(collection);

        // Animation effect
        if(this.gameObject.activeInHierarchy == true)
            this.App.DoDropdownTextUpdate(this.pulldownDocumentText);
    }

    protected override void AppSink_OnWiringParamValueChanged(
        WiringCollection collection, 
        WiringDocument wd, 
        LLDNBase owner, 
        ParamBase param)
    { 
        if(this.App.IsInUndoSession() == false)
        { 
            this.QueueHighlight(
                ThingToHighlight.CreateHLParameter(wd, owner, param));
        }

        GNUIHost uiHost;
        if(this.uiNodes.TryGetValue(owner, out uiHost) == false)
            return;

        ParamUIUpdater updtr;
        if(uiHost.paramUpdaters.TryGetValue(param, out updtr) == false)
            return;

        updtr.Update(wd, collection);
    }

    protected override void AppSink_OnWiringNodeAdded(WiringCollection collection, WiringDocument wd, LLDNBase newNode)
    {
        if(this.App.IsInUndoSession() == false)
        { 
            this.QueueHighlight(
                ThingToHighlight.CreateHLNodePlate(wd, newNode));
        }

        if(wd != this.viewingDocument)
            return;

        //GNUIHost gnuihost = this.CreateHostFromGNGenFromDrop(newNode, newNode.cachedUILocation);
        GNUIHost gnuihost = this.CreateHostFromGNGen(newNode, newNode.cachedUILocation);

         if(newNode.nodeType == LLDNBase.NodeType.GateList)
            this.factoryNodePlates[LLDNBase.NodeType.GateList].button.interactable = false;

        this.uiNodes.Add(newNode, gnuihost);
        this.FlagDirty(Dirty.DocumentSize);
    }

    protected override void AppSink_OnWiringNodeDeleted(WiringCollection collection, WiringDocument wd, LLDNBase delNode)
    { 
        if(this.App.IsInUndoSession() == false)
        { 
            this.QueueHighlight(
                ThingToHighlight.CreateHLNodePlate(wd, delNode));
        }

        if(wd != this.viewingDocument)
            return;

        GNUIHost todel;
        if(this.uiNodes.TryGetValue(delNode, out todel) == false)
            return;

        if(this.selectedNode != null && this.selectedNode.slave == delNode)
            this.DeselectNode();

        switch(todel.slave.nodeType)
        { 
            case LLDNBase.NodeType.GateList:
                { 
                    if(this.viewingDocument.GateList == null)
                        factoryNodePlates[LLDNBase.NodeType.GateList].button.interactable = true;
                }
                break;

            case LLDNBase.NodeType.Reference:
                { 
                    LLDNReference gnRef = todel.slave as LLDNReference;
                    if(gnRef != null)
                        this.wireReferencesUIData.Remove(gnRef);
                }
                break;
        }

        for (int i = this.linkCurves.Count - 1; i >= 0; --i)
        {
            HermiteLink hl = this.linkCurves[i];
            if (hl.nodeOutput == todel.slave)
            {
                // if delTarg is the thing being referenced to,
                // we should get rid of the reference to it.
                hl.inputParam.SetReference(null);
            }
            else if (hl.nodeInput == todel.slave)
            {
                // If it's one of our references, nothing special
                // needs to be done besides make it past the else
                // statement.
            }
            else
                continue;

            GameObject.Destroy(hl.uiCurve.gameObject);
            this.linkCurves.RemoveAt(i);
        }

        this.uiNodes.Remove(delNode);
        GameObject.Destroy(todel.gameObject);

        this.FlagDirty(Dirty.NodeOutputStyle);
        this.FlagDirty(Dirty.DocumentSize);
    }

    protected override void AppSink_OnWiringNodeMoved(
        WiringCollection collection, 
        WiringDocument wd, 
        LLDNBase moved)
    {
        if(this.App.IsInUndoSession() == false)
        { 
            this.QueueHighlight(
                ThingToHighlight.CreateHLNodePlate(wd, moved));
        }

        if(wd != this.viewingDocument)
            return;

        GNUIHost uiHost;
        if(this.uiNodes.TryGetValue(moved, out uiHost) == false)
            return;

        uiHost.rectTransform.anchoredPosition = moved.cachedUILocation;
        this.UpdateGNUIHostFromDrag(uiHost);

        this.RescanDocumentExtents();
    }

    void RescanDocumentExtents()
    { 
        Vector2 min;
        Vector2 max;
        this.FindExtents(out min, out max);
        this.docXMax = max.x;
        this.docYMin = min.y;
        this.wiringRegion.content.sizeDelta = 
            new Vector2(
            this.docXMax + documentPadding, 
            -this.docYMin + documentPadding);
    }

    protected override void AppSink_OnWiringLinkChanged(
        WiringCollection collection, 
        WiringDocument wd, 
        LLDNBase outNode, 
        LLDNBase inNode, 
        ParamConnection inSocket)
    { 

        if(wd != this.viewingDocument)
            return;

        if(outNode == null)
        { 
            for(int i = 0; i < this.linkCurves.Count; ++i)
			{ 
				HermiteLink hl = this.linkCurves[i];
				if(hl.nodeInput == inNode && hl.inputParam == inSocket)
				{ 
					this.linkCurves.RemoveAt(i);
					GameObject.Destroy(hl.uiCurve.gameObject);
					break;
				}
			}
        }
        else
        { 
            this.CreateLink(
	            outNode,
	            inNode,
	            inSocket);
        }

        if(this.App.IsInUndoSession() == false)
        { 
            this.QueueHighlight(
                ThingToHighlight.CreateHLParameter(wd, inNode, inSocket));
        }

        this.FlagDirty(Dirty.NodeOutputStyle);
    }

    public override void AppSink_OnShiftDocument(WiringCollection collection, WiringDocument wd, Vector2 shiftAmount)
    { 
        if(wd != this.viewingDocument)
            return;

        Vector2 min, max;
        this.FindExtents(out min, out max);

        this.docYMin = min.y;
        this.docXMax = max.x;
        //
        this.docXMax += this.documentPadding;
        this.docYMin -= this.documentPadding;
        this.wiringRegion.content.sizeDelta = 
            new Vector2(
                this.docXMax, 
                -this.docYMin);

        foreach(KeyValuePair<LLDNBase, GNUIHost> kvp in this.uiNodes)
            kvp.Value.rectTransform.anchoredPosition = kvp.Key.cachedUILocation;

        foreach(HermiteLink hl in this.linkCurves)
            this.UpdateLink(hl);
        
        this.documentDimDirty = false;
    }

    public override void AppSink_OnUndoRedo(string name, bool undo)
    {
        if(this.isActiveAndEnabled == false)
            return;

        float padding = 5.0f;

        name = (undo ? "Undo " : "Redo ") + name;

        GameObject goUndoPlate = new GameObject("UndoPlate");
        goUndoPlate.transform.SetParent(this.transform, false);
        UnityEngine.UI.Image imgUndoPlate = goUndoPlate.AddComponent<UnityEngine.UI.Image>();
        imgUndoPlate.type = Type.Sliced;
        imgUndoPlate.sprite = this.App.exerciseAssets.plateRounder;
        RectTransform rtUndoPlate = imgUndoPlate.rectTransform;
        rtUndoPlate.pivot = new Vector2(0.0f, 1.0f);
        rtUndoPlate.anchorMin = new Vector2(0.0f, 1.0f);
        rtUndoPlate.anchorMax = new Vector2(0.0f, 1.0f);
        
        GameObject goUndoText = new GameObject("UndoText");
        goUndoText.transform.SetParent(goUndoPlate.transform, false);
        UnityEngine.UI.Text txtUndo = goUndoText.AddComponent<UnityEngine.UI.Text>();
        txtUndo.text = name;
        txtUndo.horizontalOverflow = HorizontalWrapMode.Overflow;
        txtUndo.verticalOverflow = VerticalWrapMode.Overflow;
        txtUndo.alignment = TextAnchor.MiddleCenter;
        this.App.uiFactory.textTextAttrib.Apply(txtUndo);
        //
        TextGenerationSettings tgs = txtUndo.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
        tgs.scaleFactor = 1.0f;
        TextGenerator tg = txtUndo.cachedTextGenerator;
        //
        float txtWidth = tg.GetPreferredWidth(name, tgs);
        float txtHeight = tg.GetPreferredHeight(name, tgs);
        RectTransform rtText = txtUndo.rectTransform;
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.pivot = new Vector2(0.5f,0.5f);
        rtText.anchoredPosition = new Vector2(0.0f, 0.0f);

        rtUndoPlate.sizeDelta = 
            new Vector2(
                txtWidth + padding * 2.0f, 
                txtHeight + padding * 2.0f);

        rtText.offsetMin = Vector2.zero;
        rtText.offsetMax = Vector2.zero;

        CanvasGroup cg = goUndoPlate.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        Vector3 [] rvupmsg = new Vector3[4];
        rtUndoPlate.GetWorldCorners(rvupmsg);
        Vector3 [] rvundob = new Vector3[4];
        buttonUndo.targetGraphic.rectTransform.GetWorldCorners(rvundob);

        rtUndoPlate.transform.position += rvundob[0] - rvupmsg[1];

        const float slideAmt = 50.0f;
        float slideSign = undo ? -1.0f : 1.0f;

        this.App.uiFXTweener.SlidingAnchorFade(
            rtUndoPlate, 
            imgUndoPlate, 
            new Vector2(slideSign * slideAmt, 0.0f), 
            false, 
            false, 
            1.0f, 
            TweenUtil.RestoreMode.Delete);

    }

    IEnumerator HighlightProcessCoroutine()
    { 
        yield return new WaitForEndOfFrame();

        bool highlightedNameArea = false;

        if(this.thingToHighlight != null)
        { 
            if(this.viewingDocument != this.thingToHighlight.doc)
            {
                this.App.SetActiveDocument(this.thingToHighlight.doc, false);
                this.App.TransitionReticule(this.alignNamePulldown);
                highlightedNameArea = true;
            }

            RectTransform toHighlight = null;

            switch(this.thingToHighlight.type)
            { 
                case ThingToHighlight.Type.NodeOutput:
                    { 
                        // For now this is just copy-pasta of ThingToHighlight.Type.NodePlate
                        GNUIHost host;
                        if(this.uiNodes.TryGetValue( this.thingToHighlight.lldaw, out host) == true)
                            toHighlight = host.rectTransform;
                    }
                    break;

                case ThingToHighlight.Type.NodeParam:
                    { 
                        GNUIHost host;
                        if(this.uiNodes.TryGetValue( this.thingToHighlight.lldaw, out host) == true)
                        { 
                            ParamUIUpdater puiu;
                            if(host.paramUpdaters.TryGetValue(this.thingToHighlight.param, out puiu) == true)
                                toHighlight = puiu.rectTransform;
                        }
                            
                    }
                    break;

                case ThingToHighlight.Type.NodePlate:
                    { 
                        GNUIHost host;
                        if(this.uiNodes.TryGetValue( this.thingToHighlight.lldaw, out host) == true)
                            toHighlight = host.rectTransform;
                    }
                    break;

                case ThingToHighlight.Type.WiringDoc:
                    if(highlightedNameArea == false)
                    {
                        toHighlight = this.alignNamePulldown;
                        highlightedNameArea = true; // Not needed, but for consistency
                    }
                    break;
            }

            if(toHighlight != null)
            { 
                Vector2 endScroll = this.wiringRegion.normalizedPosition;

                Vector3 [] highlightWR = new Vector3[4];
                toHighlight.GetWorldCorners(highlightWR);

                Vector3 [] contentWR = new Vector3[4];
                this.wiringRegion.content.GetWorldCorners(contentWR);

                Vector3 [] viewportWR = new Vector3[4];
                this.wiringRegion.viewport.GetWorldCorners(viewportWR);

                float contentWidth = contentWR[2].x - contentWR[0].x;
                float viewportWidth = viewportWR[2].x - viewportWR[0].x;
                if( contentWidth > viewportWidth)
                { 
                    float scrollSpace = contentWidth - viewportWidth;

                    if(highlightWR[0].x < viewportWR[0].x)
                    { 
                        float moveBack = highlightWR[0].x - viewportWR[0].x;
                        endScroll.x += moveBack / scrollSpace;
                    }
                    else if(highlightWR[2].x > viewportWR[2].x)
                    { 
                        float moveFwd = highlightWR[2].x - viewportWR[2].x;
                        endScroll.x += moveFwd/ scrollSpace;
                    }
                }

                float contentHeight = contentWR[1].y - contentWR[0].y;
                float viewportHeight = viewportWR[1].y - viewportWR[0].y;
                if(contentHeight > viewportHeight)
                { 
                    float scrollSpace = contentHeight - viewportHeight;

                    if(highlightWR[0].y < viewportWR[0].y)
                    { 
                        float moveDown = highlightWR[0].y - viewportWR[0].y;
                        endScroll.y += moveDown / scrollSpace;
                    }
                    else if(highlightWR[1].y > viewportWR[1].y)
                    { 
                        float moveUp = highlightWR[1].y - viewportWR[1].y;
                        endScroll.y += moveUp/ scrollSpace;
                    }
                }

                if(endScroll != this.wiringRegion.normalizedPosition)
                    this.wiringRegion.normalizedPosition = endScroll;

                this.App.TransitionReticule(toHighlight);
            }
        }

        this.thingToHighlight = null;
        this.undoHighlightAfterwardsCoroutine = null;
    }

    void QueueHighlight(ThingToHighlight tth)
    { 
        if(this.isActiveAndEnabled == false)
            return;

        this.thingToHighlight = tth;

        if(this.undoHighlightAfterwardsCoroutine == null)
            this.StartCoroutine(this.HighlightProcessCoroutine());
    }
}
