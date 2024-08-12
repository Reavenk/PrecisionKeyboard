using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public class FileBrowserCrumbRgn : FileBrowserArea
        {
            public struct CrumbAssets
            { 
                public UnityEngine.UI.Image plate;
                public UnityEngine.UI.Button btn;
                public UnityEngine.UI.Text txt;
                public float cachedWidth;
                public string path;
            }

            UnityEngine.UI.InputField editInput;
            UnityEngine.UI.Text textInput;

            UnityEngine.UI.Button backBtn;
            UnityEngine.UI.Button fwdBtn;
            UnityEngine.UI.Button upBtn;

            FileBrowseProp prop;
            FileBrowser browser;
            List<CrumbAssets> crumbAssets = new List<CrumbAssets>();

            bool created = false;

            public void Create(FileBrowseProp prop, FileBrowser browser)
            { 
                this.prop = prop;
                this.browser = browser;

                //
                //      NAV BUTTONS
                ////////////////////////////////////////////////////////////////////////////////
                this.backBtn = this.prop.CreateButton(this.transform, "");
                if(this.prop.navBackSprite != null)
                    FileBrowseProp.CreateCenteredImage(this.prop.navBackSprite, backBtn.transform, Vector2.zero);

                this.backBtn.onClick.AddListener(()=>{ this.browser.HistoryGoBack(); });

                this.fwdBtn = this.prop.CreateButton(this.transform, "");
                if(this.prop.navForwardSprite != null)
                    FileBrowseProp.CreateCenteredImage(this.prop.navForwardSprite, fwdBtn.transform, Vector2.zero);

                this.fwdBtn.onClick.AddListener(()=>{this.browser.HistoryGoForward(); });

                this.upBtn = this.prop.CreateButton(this.transform, "UpButton");
                if(this.prop.navUpSprite != null)
                    FileBrowseProp.CreateCenteredImage(this.prop.navUpSprite, upBtn.transform, Vector2.zero);

                this.upBtn.onClick.AddListener(()=>{this.browser.NavigateParent(); });

                //
                //      Edit Location
                ////////////////////////////////////////////////////////////////////////////////

                GameObject goLoc = new GameObject("EditLoc");
                goLoc.transform.SetParent(this.transform);
                goLoc.transform.localRotation = Quaternion.identity;
                goLoc.transform.localScale = Vector3.one;
                UnityEngine.UI.Image editPlate = goLoc.AddComponent<UnityEngine.UI.Image>();
                editPlate.sprite = this.prop.inputPlateSprite;
                editPlate.type = UnityEngine.UI.Image.Type.Sliced;
                RectTransform rtEditPlate = editPlate.rectTransform;
                rtEditPlate.anchorMin = Vector2.zero;
                rtEditPlate.anchorMax = Vector2.one;
                rtEditPlate.pivot = new Vector2(0.0f, 1.0f);
                rtEditPlate.offsetMin = Vector2.zero;
                rtEditPlate.offsetMax = Vector2.zero;

                this.prop.inputText.Create(goLoc.transform, out this.textInput, "", "InputText");
                this.textInput.alignment = TextAnchor.MiddleLeft;
                RectTransform rtInputTxt = this.textInput.rectTransform;
                rtInputTxt.anchorMin = Vector2.zero;
                rtInputTxt.anchorMax = Vector2.one;
                rtInputTxt.pivot = new Vector2(0.0f, 1.0f);
                rtInputTxt.offsetMin = new Vector2(this.prop.inputPadding, 0.0f);
                rtInputTxt.offsetMax = new Vector2(-this.prop.inputPadding, 0.0f);

                this.editInput = goLoc.AddComponent<UnityEngine.UI.InputField>();
                this.editInput.targetGraphic = editPlate;
                this.editInput.textComponent = this.textInput;
                this.editInput.onEndEdit.AddListener(
                    (x)=>
                    {
                        this.OnInput_PathEndEdit(x);
                    });
                this.editInput.lineType = UnityEngine.UI.InputField.LineType.MultiLineSubmit;

                this.DisableInput();

                UnityEngine.EventSystems.EventTrigger et = goLoc.AddComponent< UnityEngine.EventSystems.EventTrigger>();
                et.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry>();
                UnityEngine.EventSystems.EventTrigger.Entry evtMDown = new UnityEngine.EventSystems.EventTrigger.Entry();
                evtMDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                evtMDown.callback.AddListener(
                    (x)=>
                    { 
                        this.OnButton_PathClick((UnityEngine.EventSystems.PointerEventData)x); 
                    });
                et.triggers.Add(evtMDown);

                this.created = true;

                this.RefreshNavigationButtonsInteractivity();
            }

            void RefreshNavigationButtonsInteractivity()
            {
                this.backBtn.interactable = 
                    (this.browser.directoryHistory.Count > 0);

                this.fwdBtn.interactable = 
                    (this.browser.directoryRedoHistory.Count > 0);

                try
                {
                    System.IO.DirectoryInfo curDir = new System.IO.DirectoryInfo(this.browser.CurDirectory);

                    if(curDir.Exists == false || curDir.Parent == null || curDir.Parent.Exists == false)
                        this.upBtn.interactable = false;
                    else
                        this.upBtn.interactable = true;
                }
                catch(System.Exception)
                {
                    this.upBtn.interactable = false;
                }
            }

            public void OnButton_PathClick(UnityEngine.EventSystems.PointerEventData evt)
            { 
                if(this.editInput.enabled == false)
                    this.EnableInput();

                this.editInput.text = this.browser.CurDirectory;
                this.Layout();
                this.editInput.OnPointerClick(evt);
            }

            public void OnInput_PathEndEdit(string val)
            { 
                // We move the accept condition into a bool, outside of the if statement
                // in case we need to processor branch it later.
                bool acceptText = 
                    val != this.browser.CurDirectory;

                if (acceptText == true)
                {
                    //Debug.LogError("INSIDE");
                    // Canonlicalize?
                    System.IO.DirectoryInfo dir;
                    try
                    {
                         dir = new System.IO.DirectoryInfo(this.textInput.text);
                    }
                    catch(System.Exception ex)
                    {
                        Debug.LogError("INVALID! : " + ex.Message);
                        return;
                    }

                    if(this.browser.ViewDirectory(dir.FullName, FileBrowser.NavigationType.AddressBar, true) == true)
                        return;
                }
                this.DisableInput();
                this.Layout();
            }

            public void Layout()
            {
                if(this.created == false)
                    return;

                RectTransform rtThis = this.GetComponent<RectTransform>();
                Vector2 sz = rtThis.rect.size;

                const float navBtnWidth = 50.0f;
                const float buffer = 50.0f;

                RectTransform rtBackBtn = this.backBtn.targetGraphic.rectTransform;
                rtBackBtn.anchorMin = new Vector2(0.0f, 0.0f);
                rtBackBtn.anchorMax = new Vector2(0.0f, 1.0f);
                rtBackBtn.offsetMin = new Vector2(navBtnWidth * 0.0f, 0.0f);
                rtBackBtn.offsetMax = new Vector2(navBtnWidth * 1.0f, 0.0f);

                RectTransform rtFwdBtn = this.fwdBtn.targetGraphic.rectTransform;
                rtFwdBtn.anchorMin = new Vector2(0.0f, 0.0f);
                rtFwdBtn.anchorMax = new Vector2(0.0f, 1.0f);
                rtFwdBtn.offsetMin = new Vector2(navBtnWidth * 1.0f, 0.0f);
                rtFwdBtn.offsetMax = new Vector2(navBtnWidth * 2.0f, 0.0f);

                RectTransform rtUpBtn = this.upBtn.targetGraphic.rectTransform;
                rtUpBtn.anchorMin = new Vector2(0.0f, 0.0f);
                rtUpBtn.anchorMax = new Vector2(0.0f, 1.0f);
                rtUpBtn.offsetMin = new Vector2(navBtnWidth * 2.0f, 0.0f);
                rtUpBtn.offsetMax = new Vector2(navBtnWidth * 3.0f, 0.0f);

                this.backBtn.interactable = this.browser.BackwardsHistoryCount() > 1;
                this.fwdBtn.interactable = this.browser.ForwardsHistoryCount() != 0;

                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo( this.browser.CurDirectory);
                this.upBtn.interactable = dir.Parent != null;


                float fCrumbIt = navBtnWidth * 3.0f;

                RectTransform rtEdit = this.editInput.targetGraphic.rectTransform;
                rtEdit.offsetMin = new Vector2(fCrumbIt, 0.0f);

                if(this.editInput.enabled == true)
                { 
                    foreach(CrumbAssets ca in this.crumbAssets)
                        ca.plate.gameObject.SetActive(false);

                    return;
                }

                float fCrumbEnd = sz.x - buffer;
                //
                int items = crumbAssets.Count;
                for(items = crumbAssets.Count - 1; items >= 0; --items)
                { 
                    float w = this.crumbAssets[items].cachedWidth;
                    if(fCrumbIt + w > fCrumbEnd)
                        break;

                    fCrumbIt += w;
                }
                // Items will be one step lower than where it should be
                items++;

                fCrumbIt = navBtnWidth * 3.0f;
                for(int i = 0; i < items; ++i)
                    this.crumbAssets[i].plate.gameObject.SetActive(false);

                for(int i = items; i < this.crumbAssets.Count; ++i)
                {
                    this.crumbAssets[i].plate.gameObject.SetActive(true);

                    RectTransform rt = this.crumbAssets[i].plate.rectTransform;
                    Rect r = rt.rect;

                    rt.anchorMin = new Vector2(0.0f, 0.0f);
                    rt.anchorMax = new Vector2(0.0f, 1.0f);

                    rt.offsetMin = new Vector2(fCrumbIt, 0.0f);
                    rt.offsetMax = new Vector2(fCrumbIt + this.crumbAssets[i].cachedWidth, 0.0f);

                    fCrumbIt += this.crumbAssets[i].cachedWidth;
                }
            }

            protected override void OnRectTransformDimensionsChange()
            {
                base.OnRectTransformDimensionsChange();

                this.Layout();
            }

            void ClearCrumbs()
            {
                if (this.created == false)
                    return;

                foreach (CrumbAssets ca in this.crumbAssets)
                    GameObject.Destroy(ca.plate.gameObject);

                this.crumbAssets.Clear();
            }

            void CreateCrumbs(System.IO.DirectoryInfo di)
            {
                this.ClearCrumbs();

                List<System.IO.DirectoryInfo> paths = new List<System.IO.DirectoryInfo>();
                for(; di != null; di = di.Parent)
                    paths.Add(di);

                paths.Reverse();

                foreach(System.IO.DirectoryInfo dtocr in paths)
                {
                    UnityEngine.UI.Image imgCr;
                    UnityEngine.UI.Button btnCr;
                    this.prop.crumbButtons.CreateButton( this.transform,"Crumb", out imgCr, out btnCr);

                    string sectionName = dtocr.FullName;

                    CrumbAssets cras = new CrumbAssets();
                    cras.btn = btnCr;
                    cras.plate = imgCr;
                    cras.path = sectionName;
                    this.prop.navCrumbFont.Create(imgCr.transform, out cras.txt, dtocr.Name, "CrumbLabel");

                    cras.txt.alignment = TextAnchor.MiddleCenter;

                    TextGenerationSettings tgs = cras.txt.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                    tgs.scaleFactor = 1.0f;
                    TextGenerator tg = cras.txt.cachedTextGenerator;
                    float textWidth = tg.GetPreferredWidth(dtocr.Name, tgs);

                    cras.cachedWidth = this.prop.navCrumbPadding * 2.0f + textWidth;

                    RectTransform rtTxt = cras.txt.rectTransform;
                    rtTxt.anchorMin = Vector2.zero;
                    rtTxt.anchorMax = Vector2.one;
                    rtTxt.pivot = new Vector2(0.5f, 0.5f);
                    rtTxt.offsetMin = Vector2.zero;
                    rtTxt.offsetMax = Vector2.zero;

                    this.crumbAssets.Add(cras);

                    if(dtocr != di)
                    { 
                        cras.btn.onClick.AddListener(
                            ()=>
                            { 
                                this.browser.ViewDirectory(sectionName, FileBrowser.NavigationType.Breadcrumb); 
                            });
                    }
                }
            }

            public override void SelectFile(string file, bool append)
            { }

            public override void SelectFiles(string[] files, bool append)
            { }

            public override void DeselectFiles()
            { }

            void DisableInput()
            {
                this.editInput.enabled = false;
                this.textInput.gameObject.SetActive(false);
            }

            void EnableInput()
            {
                this.editInput.enabled = true;
                this.textInput.gameObject.SetActive(true);
            }


            public override void ViewDirectory(string directory, FileFilter filter)
            {
                this.DisableInput();

                System.IO.DirectoryInfo dinfo = 
                    new System.IO.DirectoryInfo(directory);

                this.ClearCrumbs();
                this.CreateCrumbs(dinfo);
                this.Layout();

                this.RefreshNavigationButtonsInteractivity();
            }

            public override string OnOK()
            {
                return "";
            }

            public override string[] OnOKMulti()
            {
                return null;
            }
        }
    }
}