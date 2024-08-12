// <copyright file="GNUIHost.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary></summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GNUIHost : 
    UnityEngine.EventSystems.EventTrigger
{
    public static bool HonorEncasements = true;

    const float paramPadding = 5.0f;
    const float connectOutButtonHeight = 50.0f;
    const float encaseAlpha = 0.2f;
    
    public LLDNBase slave;

    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Text title;

    private RectTransform cachedRectTransform = null;
    internal static Vector3 cachedStartDragPos = Vector3.zero;

    private UnityEngine.UI.Image plate = null;
    public UnityEngine.UI.Image Plate 
    {get{return this.plate; } }

    public RectTransform rectTransform 
    { get{return this.cachedRectTransform; } }

    // TODO: Encapsulate
    public Vector2 outputPos;

    public PaneWiring parentWindow = null;

    public struct InputConnectionPos
    { 
        public ParamConnection connectionParam;
        public UnityEngine.UI.Image dropLocation;
        public Vector2 inputPos;
    }

    // Encapsulate
    public List<InputConnectionPos> inputConnections = 
        new List<InputConnectionPos>();

    public SocketUIOutput outputSocket = null;

    public Dictionary<ParamBase, ParamUIUpdater> paramUpdaters = 
        new Dictionary<ParamBase, ParamUIUpdater>();

    public void InitCaches()
    {
        this.cachedRectTransform = this.GetComponent<RectTransform>();
        this.plate = this.GetComponent<UnityEngine.UI.Image>();
    }
    public void Awake()
    {
        this.InitCaches();
    }

    public InputConnectionPos GetInputConnection(ParamConnection connection)
    { 
        foreach(InputConnectionPos icp in this.inputConnections)
        { 
            if(icp.connectionParam == connection)
                return icp;
        }
        return new InputConnectionPos();
    }

    public void UpdateBasePosition()
    { 
        this.slave.cachedUILocation = 
            this.cachedRectTransform.anchoredPosition;
    }

    public void UpdateFromBasePosition()
    {
        this.cachedRectTransform.anchoredPosition = 
            this.slave.cachedUILocation;
    }

    public void GenerateUI(IGNParamUICreator uic, Application app)
    { 
        if(this.slave.GetCategory() == LLDNBase.Category.Special)
        {
            LLDNComment gnCom = this.slave as LLDNComment;
            if(gnCom != null)
            {
                this._GenerateCommentUI(gnCom, uic, app);
                return;
            }

            LLDNGateList gnGList = this.slave as LLDNGateList;
            if(gnGList != null)
            { 
                this._GenerateGateList(gnCom, uic, app);
                return;
            }

            LLDNHighlight gnGL = this.slave as LLDNHighlight;
            if(gnGL != null)
            { 
                this._GenerateHighlight(gnGL, uic, app);
                return;
            }

            LLDNQuickOut gnQuick = this.slave as LLDNQuickOut;
            if(gnQuick != null)
            { 
                this._GenerateQuickOut(gnQuick, uic, app);
                return;
            }

            LLDNReference gnRef = this.slave as LLDNReference;
            if(gnRef != null)
            { 
                this._GenerateReference(gnRef, uic, app);
                return;
            }

            LLDNWindow gnWindow = this.slave as LLDNWindow;
            if(gnWindow != null)
            { 
                this._GenerateWindow(gnWindow, uic, app);
                return;
            }
        }

        this.GenerateStdUI(uic);
    }

    public void _GenerateCommentUI(LLDNComment gnCom, IGNParamUICreator uic, Application app)
    {
        float width = LLDNComment.defaultWidth;
        float height = LLDNComment.defaultHeight;
        height = Mathf.Max(height, gnCom.secrHeight.value);
        width = Mathf.Max(width, gnCom.secrWidth.value);

        this.cachedRectTransform.sizeDelta = 
            new Vector2(
                width, 
                topRegionHeight + height + 5.0f);

        GameObject goComBtn = new GameObject("CommentRgn");
        goComBtn.transform.SetParent(this.transform, false);
        UnityEngine.UI.Image img = goComBtn.AddComponent<UnityEngine.UI.Image>();
        img.type = UnityEngine.UI.Image.Type.Sliced;
        img.sprite = app.exerciseAssets.plateRounder;
        UnityEngine.UI.Button btn = goComBtn.AddComponent<UnityEngine.UI.Button>();
        RectTransform rtPl = img.rectTransform;
        rtPl.anchorMin = new Vector2(0.0f, 1.0f);
        rtPl.anchorMax = new Vector2(1.0f, 1.0f);
        rtPl.pivot = new Vector2(0.0f, 1.0f);
        rtPl.offsetMin = new Vector2(5.0f, -topRegionHeight - height);
        rtPl.offsetMax = new Vector2(-5.0f, -50.0f);

        GameObject goComTxt = new GameObject("Text");
        goComTxt.transform.SetParent(goComBtn.transform, false);
        UnityEngine.UI.Text txtCom = goComTxt.AddComponent<UnityEngine.UI.Text>();
        txtCom.color = Color.black;
        txtCom.font = uic.StandardFont;
        txtCom.fontSize = (int)(14 * gnCom.fontScale.value);
        txtCom.text = gnCom.GetComment();
        txtCom.verticalOverflow = VerticalWrapMode.Overflow;
        txtCom.horizontalOverflow = HorizontalWrapMode.Wrap;
        txtCom.supportRichText = false;
        txtCom.alignment = TextAnchor.UpperLeft;
        txtCom.verticalOverflow = VerticalWrapMode.Truncate;
        txtCom.horizontalOverflow = HorizontalWrapMode.Wrap;
        RectTransform rtTxt = txtCom.rectTransform;

        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = new Vector2(5.0f, 5.0f);
        rtTxt.offsetMax = new Vector2(-5.0f, -5);

        this.paramUpdaters.Add(
            gnCom.GetParam("text"), 
            new CommentUIUpdater(
                this,
                gnCom,
                txtCom,
                rtPl));

        if(gnCom.encase.value == true && HonorEncasements == true)
        { 
            Color c = img.color;
            c.a = encaseAlpha;
            img.color = c;
            this.icon.gameObject.SetActive(false);

            Color cp = this.plate.color;
            cp.a = encaseAlpha;
            this.plate.color = cp;
            this.enabled = false;

            btn.enabled = false;
            // Push to back so stuff can be dragged on top of it
            this.rectTransform.SetAsFirstSibling();

            // Something still blocks multitouch
            CanvasGroup cgText = txtCom.gameObject.AddComponent<CanvasGroup>();
            cgText.interactable = false;
        }

        btn.onClick.AddListener(
           () =>
           {
               app.DoVibrateButton();

               PxPre.UIL.Dialog dlg = app.dlgSpawner.CreateDialogTemplate(false, true);
               dlg.AddDialogTemplateTitle("Comment");

               PxPre.UIL.EleInput input = app.uiFactory.CreateInput(dlg.rootParent);
               dlg.contentSizer.Add(input, 1.0f, PxPre.UIL.LFlag.Grow);
               input.input.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;
               input.input.text = gnCom.GetComment();
               input.text.alignment = TextAnchor.UpperLeft;

               dlg.AddDialogTemplateSeparator();

               PxPre.UIL.Dialog.OptionsButton[] dlgButtons = 
                    dlg.AddDialogTemplateButtons(
                       new PxPre.UIL.DlgButtonPair[]
                       {
                            new PxPre.UIL.DlgButtonPair("Close", null),
                            new PxPre.UIL.DlgButtonPair("Save Changes (Unmodified)",
                            (x)=>
                            {
                                string oldValue = gnCom.GetComment();
                                string newValue = input.input.text;

                                gnCom.SetComment(newValue);

                                app.SetLLDAWParamValue(
                                    gnCom,
                                    gnCom.GetParamText("text"),
                                    oldValue,
                                    newValue);

                                //this.UpdateDimensionsFromComment(txtCom);
                                return true;
                            })
                       });

               PxPre.UIL.Dialog.OptionsButton okButton = dlgButtons[1];
               okButton.button.interactable = false;

               input.input.onValueChanged.AddListener(
                   (x)=>
                   { 
                       okButton.button.interactable = true;
                       okButton.text.text = "Save Changes";
                       dlgButtons[0].text.text = "Discard";
                    });

               dlg.host.LayoutInRT(false);
               app.SetupDialogForTransition(dlg, img.rectTransform, true, true);
           });
    }

    public void UpdateDimensionsFromComment(UnityEngine.UI.Text commentText, RectTransform textContainer, LLDNComment comment)
    { 
        commentText.text = comment.GetComment();
        commentText.supportRichText = false;
        //gnCom.SetComment(input.input.text);

        TextGenerationSettings tgs = 
            commentText.GetGenerationSettings(
                new Vector2(
                    commentText.rectTransform.rect.width, 
                    float.PositiveInfinity));

        TextGenerator tg = commentText.cachedTextGenerator;
        float prefHt = tg.GetPreferredHeight(commentText.text, tgs) / commentText.transform.lossyScale.y;
        prefHt = Mathf.Ceil(prefHt) + 1.0f/this.transform.lossyScale.y;
        prefHt = Mathf.Max(prefHt, LLDNComment.defaultHeight);

        textContainer.sizeDelta = 
            new Vector2(
                textContainer.sizeDelta.x, 
                prefHt + 10.0f);

        this.rectTransform.sizeDelta = 
            new Vector2(
                this.rectTransform.sizeDelta.x,
                prefHt + 15.0f + topRegionHeight);

        comment.secrHeight.value = prefHt + 10.0f;
    }

    public void _GenerateGateList(LLDNComment gnGL, IGNParamUICreator uic, Application app)
    { 
        this.GenerateStdUI(uic);
    }

    public void _GenerateHighlight(LLDNHighlight gnHL, IGNParamUICreator uic, Application app)
    { 
        float itStop = this.GenerateStdUI(uic);

        GameObject goButton = new GameObject("HighlightBtn");
        goButton.transform.SetParent(this.transform, false);
        UnityEngine.UI.Image imgBtn = goButton.AddComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Button btnBtn = goButton.AddComponent<UnityEngine.UI.Button>();
        RectTransform rtBtn = imgBtn.rectTransform;

        btnBtn.onClick.AddListener(
            ()=>
            { 
                if(gnHL.actionString == null || string.IsNullOrEmpty(gnHL.actionString.value) == true)
                    return;

                uic.PerformAction( rtBtn, gnHL.actionString.value);
            });

        rtBtn.RTQ().
            TopLeftPivot().
            AnchMin(0.0f, 1.0f).
            AnchMax(1.0f, 1.0f).
            OffsetMin(5.0f, itStop - connectOutButtonHeight * gnHL.fontScale.value).
            OffsetMax(-5.0f, itStop);

        GameObject goText = new GameObject("QuickButtonComText");
        goText.transform.SetParent(goButton.transform, false);
        UnityEngine.UI.Text txt = goText.AddComponent<UnityEngine.UI.Text>();
        RectTransform rtTxt = txt.rectTransform;
        rtTxt.RTQ().
            TopLeftPivot().
            ExpandAnchors().
            OffsetMin(0.0f, 5.0f).
            OffsetMax(0.0f, 0.0f);

        string buttonText = "Highlight";
        ParamText pt = gnHL.GetParamText("Button");
        if(pt != null)
            buttonText = pt.value;

        txt.text = buttonText;

        // Arguably setting this up should be handled elsewhere.
        PushdownButton pushB = btnBtn as PushdownButton;
        if(pushB != null)
            pushB.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);

        txt.alignment = TextAnchor.MiddleCenter;

        app.uiFactory.ApplyButtonStyle(btnBtn, imgBtn, null, true);
        app.uiFactory.ApplyTextStyle(txt);
        txt.fontSize = (int)(txt.fontSize * gnHL.fontScale.value);

        Vector2 szD = this.rectTransform.sizeDelta;
        szD.x = Mathf.Max(szD.x, gnHL.secrWidth.value);

        this.rectTransform.sizeDelta = new Vector2(szD.x, szD.y + connectOutButtonHeight * gnHL.fontScale.value);

        string iconP = gnHL.icon.value;
        if(string.IsNullOrEmpty(iconP) == false)
        { 
            Sprite sp = uic.GetIcon(iconP);
            if(sp != null)
                this.icon.sprite = sp;
        }

        if(gnHL.encase.value == true && HonorEncasements == true)
        { 
            this.enabled = false;

            Color pc = this.plate.color;
            pc.a = encaseAlpha;
            this.plate.color = pc;

            Color ic = this.icon.color;
            ic.a = encaseAlpha;
            this.icon.color = ic;
        }
    }

    public void _GenerateQuickOut(LLDNQuickOut gnQuick, IGNParamUICreator uic, Application app)
    { 
        float itStop = this.GenerateStdUI(uic);

        GameObject goButton = new GameObject("QuickOutBtn");
        goButton.transform.SetParent(this.transform, false);
        UnityEngine.UI.Image imgBtn = goButton.AddComponent<UnityEngine.UI.Image>();
        UnityEngine.UI.Button btnBtn = goButton.AddComponent<UnityEngine.UI.Button>();
        RectTransform rtBtn = imgBtn.rectTransform;

        btnBtn.onClick.AddListener(()=>{ uic.ConnectToOutput(gnQuick); });

        rtBtn.RTQ().
            TopLeftPivot().
            AnchMin(0.0f, 1.0f).
            AnchMax(1.0f, 1.0f).
            OffsetMin(5.0f, itStop - connectOutButtonHeight).
            OffsetMax(-5.0f, itStop);

        GameObject goText = new GameObject("QuickButtonComText");
        goText.transform.SetParent(goButton.transform, false);
        UnityEngine.UI.Text txt = goText.AddComponent<UnityEngine.UI.Text>();
        RectTransform rtTxt = txt.rectTransform;
        rtTxt.RTQ().
            TopLeftPivot().
            ExpandAnchors().
            OffsetMin(0.0f, 5.0f).
            OffsetMax(0.0f, 0.0f);

        string buttonText = "Connect Out";
        ParamText pt = gnQuick.GetParamText("Button");
        if(pt != null)
            buttonText = pt.value;

        txt.text = buttonText;

        // Arguably setting this up should be handled elsewhere.
        PushdownButton pushB = btnBtn as PushdownButton;
        if(pushB != null)
            pushB.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);

        txt.alignment = TextAnchor.MiddleCenter;

        app.uiFactory.ApplyButtonStyle(btnBtn, imgBtn, null, true);
        app.uiFactory.ApplyTextStyle(txt);

        Vector2 szD = this.rectTransform.sizeDelta;
        this.rectTransform.sizeDelta = new Vector2(szD.x, szD.y + connectOutButtonHeight);

    }

    public void _GenerateReference(LLDNReference gnRef, IGNParamUICreator uic, Application app)
    {
        this.GenerateStdUI(uic);
    }

    public void _GenerateWindow(LLDNWindow gnWin, IGNParamUICreator uic, Application app)
    { 
        this.GenerateStdUI(uic);
    }

    const float topRegionHeight = 50.0f;

    protected float GenerateStdUI(IGNParamUICreator uic)
    { 
        float fy = -topRegionHeight;

        bool atleastone = false;
        foreach(ParamBase pb in this.slave.nodeParams)
        { 
            if(pb.editable == false)
                continue;

            if(atleastone == false)
                atleastone = true;
            else
                fy -= paramPadding;

            if (pb.type == ParamBase.Type.PCMInput)
            { 
                ParamConnection pcon = pb as ParamConnection;
                UnityEngine.UI.Image inputDrop = 
                    uic.CreatePCMConnection(this.cachedRectTransform, ref fy, this.slave, pcon);

                // TODO: A more rigerous implementation
                InputConnectionPos input = new InputConnectionPos();
                input.connectionParam = pcon;
                input.inputPos = new Vector2(5.0f, fy + 25.0f); // Including this being more rigerous

                if (inputDrop != null)
                    input.dropLocation = inputDrop;

                this.inputConnections.Add(input);

                this.paramUpdaters.Add(pcon, new SocketUIUpdater(inputDrop.rectTransform));
            }
            else if(pb.type == ParamBase.Type.Bool)
            { 
                ParamBool pbool = pb as ParamBool;
                ParamUIUpdater puiuptr = 
                    uic.CreateBoolSwitch(this.cachedRectTransform, ref fy, this.slave, pbool);

                this.paramUpdaters.Add(pbool, puiuptr);
            }
            else if(pb.type == ParamBase.Type.Enum)
            { 
                ParamEnum penum = pb as ParamEnum;
                ParamUIUpdater puiuptr = 
                    uic.CreateEnumPulldown(this.cachedRectTransform, ref fy, this.slave, penum);

                this.paramUpdaters.Add(penum, puiuptr);
            }
            else if(pb.type == ParamBase.Type.Float)
            { 
                ParamFloat pfloat = pb as ParamFloat;
                if(pb.widgetType == "time")
                {
                    ParamUIUpdater puiuptr = 
                        uic.CreateFloatTime(this.cachedRectTransform, ref fy, this.slave, pfloat);

                    this.paramUpdaters.Add(pfloat, puiuptr);
                }
                else if(pb.widgetType == "clampeddial")
                {
                    ParamUIUpdater puiuptr = 
                        uic.CreateFloatClampedDial(this.cachedRectTransform, ref fy, this.slave, pfloat);

                    this.paramUpdaters.Add(pfloat, puiuptr);
                }
                else
                {
                    ParamUIUpdater puiuptr = 
                        uic.CreateFloatRangeSlider(this.cachedRectTransform, ref fy, this.slave, pfloat);

                    this.paramUpdaters.Add(pfloat, puiuptr);
                }
            }
            else if(pb.type == ParamBase.Type.Int)
            { 
                ParamInt pint = pb as ParamInt;

                if(pb.widgetType == "input")
                { 
                }
                else
                {
                    ParamUIUpdater puiuptr = 
                        uic.CreateIntRangeSlider(this.cachedRectTransform, ref fy, this.slave, pint);

                    this.paramUpdaters.Add(pint, puiuptr);
                }
            }
            else if(pb.type == ParamBase.Type.TimeLen)
            { 
                ParamTimeLen ptl = pb as ParamTimeLen;
                ParamUIUpdater puiuptr = 
                    uic.CreateTimeLenEdit(this.cachedRectTransform, ref fy, this.slave, ptl);

                this.paramUpdaters.Add(ptl, puiuptr);
            }
            else if(pb.type == ParamBase.Type.WireReference)
            { 
                ParamWireReference pwr = pb as ParamWireReference;
                ParamUIUpdater puiuptr = 
                    uic.CreateWiringReference(this.cachedRectTransform, ref fy, this.slave, pwr);

                this.paramUpdaters.Add(pwr, puiuptr);
            }
            else if(pb.type == ParamBase.Type.Nickname)
            {
                ParamNickname pn = pb  as ParamNickname;
                ParamUIUpdater puiuptr = 
                    uic.CreateNickname(this.cachedRectTransform, ref fy, this.slave, pn);

                this.paramUpdaters.Add(pn, puiuptr);
            }
        }

        float szdX = this.cachedRectTransform.sizeDelta.x;

        this.cachedRectTransform.sizeDelta = new Vector2(szdX, -fy + 5.0f);

        this.outputPos = new Vector2(szdX + 10.0f, -25.0f);

        return fy;
    }

    public static Vector2 EstimateHeight( LLDNBase.NodeType type, IEnumerable<ParamBase> paramToEst, IGNParamUICreator uic)
    {
        // NOTE: Since we're passing the node type and its params, at this point 
        // we should probably just consider passing in a copy of the node itself.

        float fy = 50.0f;
        switch(type)
        {
            // For things that completly override the height, just spit it out.
            case LLDNBase.NodeType.Comment:
                return new Vector2(LLDNComment.defaultWidth, fy + LLDNComment.defaultHeight);
        }

        bool atleastone = false;
        foreach (ParamBase pb in paramToEst)
        {
            if(pb.editable == false)
                continue;

            if (atleastone == false)
                atleastone = true;
            else
                fy += paramPadding;

            
            uic.AccumulateHeight(pb.type, pb.widgetType, ref fy);
        }

        fy += 5.0f;

        switch(type)
        { 
            // For things that append to the end, add it in after
            // the standard parameters have been accounted for.
            case LLDNBase.NodeType.QuickOut:
            case LLDNBase.NodeType.Highlight:
                fy += connectOutButtonHeight;
                break;
        }

        // We actually don't know the width, it's set from the outside.
        return new Vector2( 0.0f, fy);
    }

    
    public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        cachedStartDragPos = 
            this.cachedRectTransform.InverseTransformPoint(eventData.position);

        this.parentWindow.OnGNUIHost_BeginDrag(this, eventData);
    }

    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    { 
        this.parentWindow.OnGNUIHost_Drag(this, eventData);
    }

    public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        this.parentWindow.OnGNUIHost_EndDrag(this, eventData);
    }

    public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        this.cachedRectTransform.SetAsLastSibling();
        this.parentWindow.OnGNUIHost_PointerDown(this, eventData);
    }

    public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        this.parentWindow.OnGNUIHost_Click(this, eventData);
    }

    public void DialateInputArrows()
    {
        foreach (InputConnectionPos icp in inputConnections)
            icp.dropLocation.transform.localScale = new Vector3(3.0f, 2.0f, 2.0f);
    }

    public void DialateOutputArrow()
    { 
        if(this.outputSocket == null)
            return;

        this.outputSocket.transform.localScale = new Vector3(3.0f, 2.0f, 2.0f);
    }

    public void UndialateInputArrows()
    {
        foreach(InputConnectionPos icp in inputConnections)
            icp.dropLocation.transform.localScale = Vector3.one;
    }

    public void UndialateOutputArrow()
    {
        if(this.outputSocket == null)
            return;

        this.outputSocket.transform.localScale = Vector3.one;
    }
}
