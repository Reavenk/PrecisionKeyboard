using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public class FileBrowseFileListRgn : FileBrowserArea
        {
            public struct FileEntry
            { 
                public bool file;
                public UnityEngine.UI.Image plate;
                public UnityEngine.UI.Text text;
                public UnityEngine.UI.Image icon;
                public UnityEngine.UI.Button btn;
                public string path;
            }

            UnityEngine.UI.ScrollRect scrollRect;

            List<FileEntry> fileAssets = new List<FileEntry>();
            Dictionary<string, FileEntry> fileAssetDirectory = new Dictionary<string, FileEntry>();
            HashSet<string> selectedEntries = new HashSet<string>();

            PxPre.FileBrowse.FileBrowseProp properties;

            FileBrowser parentBrowser = null;

            public void Create(PxPre.FileBrowse.FileBrowseProp prop, FileBrowser browser)
            {
                this.parentBrowser = browser;
                this.properties = prop;

                const float scrollHorizHeight = 40.0f;
                const float scrollVertWidth = 40.0f;

                GameObject goBrowseView = new GameObject("BrowseView");
                goBrowseView.transform.SetParent(this.gameObject.transform);
                goBrowseView.transform.localScale = Vector3.one;
                goBrowseView.transform.localRotation = Quaternion.identity;
                RectTransform rtBrowseView = goBrowseView.AddComponent<RectTransform>();
                rtBrowseView.anchorMin = Vector2.zero;
                rtBrowseView.anchorMax = Vector2.one;
                rtBrowseView.pivot = new Vector2(0.0f, 1.0f);
                rtBrowseView.offsetMin = new Vector2(5.0f, 5.0f);
                rtBrowseView.offsetMax = new Vector2(-5.0f, -5.0f);

                GameObject goViewport = new GameObject("Viewport");
                goViewport.transform.SetParent(goBrowseView.transform);
                goViewport.transform.localScale = Vector3.one;
                goViewport.transform.localRotation = Quaternion.identity;
                RectTransform rtVP = goViewport.AddComponent<RectTransform>();
                rtVP.anchorMin = Vector2.zero;
                rtVP.anchorMax = Vector2.one;
                rtVP.pivot = new Vector2(0.0f, 1.0f);
                rtVP.offsetMin = new Vector2(0.0f, scrollHorizHeight);
                rtVP.offsetMax = new Vector2(-scrollVertWidth, 0.0f);
                UnityEngine.UI.Image imgViewport = goViewport.AddComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Mask maskVP = goViewport.AddComponent<UnityEngine.UI.Mask>();
             

                GameObject goContent = new GameObject("Content");
                goContent.transform.SetParent(goViewport.transform);
                goContent.transform.localScale = Vector3.one;
                goContent.transform.localRotation = Quaternion.identity;
                RectTransform rtContent = goContent.AddComponent<RectTransform>();
                rtContent.anchorMin = new Vector2(0.0f, 1.0f);
                rtContent.anchorMax = new Vector2(0.0f, 1.0f);
                rtContent.pivot = new Vector2(0.0f, 1.0f);
                rtContent.offsetMin = new Vector2(0.0f, -10.0f);
                rtContent.offsetMax = new Vector2(10.0f, 0.0f);

                //      VERT SCROLL
                //
                ////////////////////////////////////////////////////////////////////////////////
                ///
                GameObject goVertScroll = new GameObject("VertScroll");
                goVertScroll.transform.SetParent(goBrowseView.transform);
                goVertScroll.transform.localScale = Vector3.one;
                goVertScroll.transform.localRotation = Quaternion.identity;
                RectTransform rtVertScr = goVertScroll.AddComponent<RectTransform>();
                rtVertScr.anchorMin = new Vector2(1.0f, 0.0f);
                rtVertScr.anchorMax = new Vector2(1.0f, 1.0f);
                rtVertScr.pivot = new Vector2(0.0f, 1.0f);
                rtVertScr.offsetMin = new Vector2(-scrollVertWidth, scrollHorizHeight);
                rtVertScr.offsetMax = new Vector2(0.0f, 0.0f);
                UnityEngine.UI.Image imgVertScr = goVertScroll.AddComponent<UnityEngine.UI.Image>();
                imgVertScr.sprite = this.properties.sliderSprite;
                imgVertScr.type = UnityEngine.UI.Image.Type.Sliced;


                GameObject goVertThumb = new GameObject("VertThumb");
                goVertThumb.transform.SetParent(goVertScroll.transform);
                goVertThumb.transform.localScale = Vector3.one;
                goVertThumb.transform.localRotation = Quaternion.identity;
                RectTransform rtVertThumb = goVertThumb.AddComponent<RectTransform>();
                rtVertThumb.pivot = new Vector2(0.0f, 1.0f);
                rtVertThumb.offsetMin = Vector2.zero;
                rtVertThumb.offsetMax = Vector2.zero;
                UnityEngine.UI.Image imgVertThumb = goVertThumb.AddComponent<UnityEngine.UI.Image>();

                UnityEngine.UI.Scrollbar sbVert = goVertScroll.AddComponent<UnityEngine.UI.Scrollbar>();
                sbVert.handleRect = rtVertThumb;
                sbVert.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
                sbVert.targetGraphic = imgVertThumb;

                this.properties.sliderThumb.Apply(sbVert, imgVertThumb);

                //      HORIZ SCROLL
                //
                ////////////////////////////////////////////////////////////////////////////////
                ///
                GameObject goHorizScroll = new GameObject("HorizScroll");
                goHorizScroll.transform.SetParent(goBrowseView.transform);
                goHorizScroll.transform.localScale = Vector3.one;
                goHorizScroll.transform.localRotation = Quaternion.identity;
                RectTransform rtHorizScr = goHorizScroll.AddComponent<RectTransform>();
                rtHorizScr.anchorMin = new Vector2(0.0f, 0.0f);
                rtHorizScr.anchorMax = new Vector2(1.0f, 0.0f);
                rtHorizScr.pivot = new Vector2(0.0f, 1.0f);
                rtHorizScr.offsetMin = Vector2.zero;
                rtHorizScr.offsetMax = new Vector2(-scrollVertWidth, scrollHorizHeight);
                UnityEngine.UI.Image imgHorzScr = goHorizScroll.AddComponent<UnityEngine.UI.Image>();
                imgHorzScr.sprite = this.properties.sliderSprite;
                imgHorzScr.type =  UnityEngine.UI.Image.Type.Sliced;

                GameObject goHorizThumb = new GameObject("HorizThumb");
                goHorizThumb.transform.SetParent(rtHorizScr);
                goHorizThumb.transform.localScale = Vector3.one;
                goHorizThumb.transform.localRotation = Quaternion.identity;
                RectTransform rtHorizThumb = goHorizThumb.AddComponent<RectTransform>();
                rtHorizThumb.pivot = new Vector2(0.0f, 1.0f);
                rtHorizThumb.offsetMin = Vector2.zero;
                rtHorizThumb.offsetMax = Vector2.zero;
                UnityEngine.UI.Image imgHorizThumb = goHorizThumb.AddComponent<UnityEngine.UI.Image>();

                UnityEngine.UI.Scrollbar sbHoriz = goHorizScroll.AddComponent<UnityEngine.UI.Scrollbar>();
                sbHoriz.handleRect = rtHorizThumb;
                sbHoriz.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;

                this.properties.sliderThumb.Apply(sbHoriz, imgHorizThumb);

                //      RECT
                //
                ////////////////////////////////////////////////////////////////////////////////

                this.scrollRect = goBrowseView.AddComponent<UnityEngine.UI.ScrollRect>();
                this.scrollRect.viewport = rtVP;
                this.scrollRect.content = rtContent;
                this.scrollRect.horizontalScrollbar = sbHoriz;
                this.scrollRect.verticalScrollbar = sbVert;
                this.scrollRect.scrollSensitivity = this.properties.fileScrollSensitivity;

                //      FIN
                //
                ////////////////////////////////////////////////////////////////////////////////

                // Make dirty
                this.scrollRect.gameObject.SetActive(false);
                this.scrollRect.gameObject.SetActive(true);
            }

            public void ClearFiles()
            {
                foreach (FileEntry fe in this.fileAssets)
                    GameObject.Destroy(fe.plate.gameObject);

                this.fileAssets.Clear();
                this.selectedEntries.Clear();
                this.fileAssetDirectory.Clear();

                this.scrollRect.verticalScrollbar.value = 1.0f;
                this.scrollRect.horizontalScrollbar.value = 0.0f;
                this.scrollRect.content.sizeDelta = Vector2.zero;

                foreach(Transform t in this.scrollRect.content)
                    GameObject.Destroy(t.gameObject);
            }

            public override void ViewDirectory(string directory, FileFilter filter)
            { 
                this.ClearFiles();

                try
                {
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(directory);
                    if(dir.Exists == false)
                    {
                        this.PlaceMessageInFileArea("Folder does not exist.");
                        return;
                    }

                    foreach (System.IO.DirectoryInfo di in dir.GetDirectories())
                    {
                        FileEntry fe = this.GenerateFileEntry(di, string.Empty, false);
                        this.fileAssets.Add(fe);
                        this.DeselectFileEntry(fe);
                        this.fileAssetDirectory.Add(di.FullName, fe);
                    }

                    System.IO.FileInfo [] files = dir.GetFiles(filter.filterString);

                    foreach (System.IO.FileInfo fi in files)
                    {
                        FileEntry fe = this.GenerateFileEntry(fi, fi.Extension, true);
                        this.fileAssets.Add(fe);
                        this.DeselectFileEntry(fe);
                        this.fileAssetDirectory.Add(fi.FullName, fe);
                    }
                    this.LayoutFileEntries();
                }
                catch(System.Exception ex)
                {
                    this.PlaceMessageInFileArea($"Issue viewing folder:\n{ex.Message}.");
                }

            }

            void PlaceMessageInFileArea(string message)
            {
                this.scrollRect.content.sizeDelta =
                    new Vector2(0.0f, 0.0f);

                GameObject goText = new GameObject("FileAreaMsg");
                goText.transform.SetParent(this.scrollRect.content, false);
                UnityEngine.UI.Text txt = goText.AddComponent<UnityEngine.UI.Text>();

                txt.text = message;
                txt.rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
                txt.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
                txt.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                txt.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                txt.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);

                txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.verticalOverflow = VerticalWrapMode.Overflow;

                this.parentBrowser.properties.inputText.Apply(txt);
            }

            void LayoutFileEntries()
            { 
                float outerPaddingHoriz = 5.0f;
                float outerPaddingVert = 5.0f;

                float maxIconWidth = 0.0f;
                float maxTextWidth = 0.0f;

                const float icoTextPadding = 5.0f;
                const float padding = 5.0f;

                foreach(FileEntry fe in this.fileAssets)
                { 
                    if(fe.icon != null && fe.icon.sprite != null)
                        maxIconWidth = Mathf.Max(maxIconWidth, fe.icon.sprite.rect.width);

                    TextGenerationSettings tgs = 
                        fe.text.GetGenerationSettings(
                            new Vector2(
                                float.PositiveInfinity, 
                                float.PositiveInfinity));

                    tgs.scaleFactor = 1.0f;

                    TextGenerator tg = fe.text.cachedTextGenerator;

                    float textWidth = tg.GetPreferredWidth(fe.text.text, tgs);
                    maxTextWidth = Mathf.Max(maxTextWidth, textWidth);
                }

                float fY = -outerPaddingVert;
                float textPos = padding + maxIconWidth + icoTextPadding;
                float totalWidth = textPos + maxTextWidth + padding;

                float entryVertPadding = 10.0f;

                bool atLeastOne = false;
                foreach (FileEntry fe in this.fileAssets)
                { 
                    if(atLeastOne == false)
                        atLeastOne = true;
                    else
                        fY -= entryVertPadding;

                    fe.plate.rectTransform.anchoredPosition = 
                        new Vector2(outerPaddingHoriz, fY);

                    float heightIco = 0.0f;
                    if(fe.icon != null && fe.icon.sprite != null)
                        heightIco = fe.icon.sprite.rect.height;

                    TextGenerationSettings tgs = 
                        fe.text.GetGenerationSettings(
                            new Vector2(
                                float.PositiveInfinity, 
                                float.PositiveInfinity));

                    tgs.scaleFactor = 1.0f;

                    TextGenerator tg = fe.text.cachedTextGenerator;
                    float heightText = tg.GetPreferredHeight(fe.text.text, tgs);
                    heightText = Mathf.Ceil(heightText + 1.0f);

                    float maxh = Mathf.Max(heightIco, heightText);
                    float totalheight = maxh + padding + padding;

                    fe.plate.rectTransform.sizeDelta = 
                        new Vector2(
                            totalWidth,
                            totalheight);

                    if(fe.icon != null && fe.icon.sprite != null)
                    { 
                        Vector2 spriteSz = fe.icon.sprite.rect.size;
                        fe.icon.rectTransform.sizeDelta = spriteSz;
                        fe.icon.rectTransform.anchoredPosition = 
                            new Vector2(
                                (maxIconWidth - spriteSz.x) * 0.5f + padding,
                                -padding - (maxh - spriteSz.y) * 0.5f);
                    }

                    fe.text.rectTransform.sizeDelta = new Vector2(maxTextWidth, heightText);
                    fe.text.rectTransform.anchoredPosition = new Vector2(textPos, -padding - (maxh - heightText) * 0.5f);

                    fY -= totalheight;
                }

                this.scrollRect.content.sizeDelta = 
                    new Vector2(totalWidth + outerPaddingHoriz + outerPaddingHoriz, 
                    -fY + outerPaddingVert);
            }

            FileEntry GenerateFileEntry(System.IO.FileSystemInfo fsi, string ext, bool isfile)
            { 
                FileEntry fi = new FileEntry();

                PxPre.FileBrowse.FileBrowseProp.IconGroup icoGr = 
                    new PxPre.FileBrowse.FileBrowseProp.IconGroup();
        
                if (isfile == false)
                    icoGr = this.properties.iconFolder;
                else
                    icoGr = properties.GetExtIcon(ext);

                GameObject goPlate = new GameObject("Plate");
                goPlate.transform.SetParent(this.scrollRect.content);
                goPlate.transform.localRotation = Quaternion.identity;
                goPlate.transform.localScale = Vector3.one;
                RectTransform rtPlate = goPlate.AddComponent<RectTransform>();
                rtPlate.anchorMin = new Vector2(0.0f, 1.0f);
                rtPlate.anchorMax = new Vector2(0.0f, 1.0f);
                rtPlate.pivot = new Vector2(0.0f, 1.0f);
                UnityEngine.UI.Image imgPlate = goPlate.AddComponent<UnityEngine.UI.Image>();
                imgPlate.type = UnityEngine.UI.Image.Type.Sliced;
                UnityEngine.UI.Button btn = goPlate.AddComponent<UnityEngine.UI.Button>();
                btn.onClick.AddListener(
                    ()=>
                    { 
                        this.parentBrowser.SelectFile(fsi.FullName, true);
                    });

                GameObject goIco = new GameObject("Ico");
                goIco.transform.SetParent(rtPlate, false);
                goIco.transform.localRotation = Quaternion.identity;
                goIco.transform.localScale = Vector3.one;
                RectTransform rtIcon = goIco.AddComponent<RectTransform>();
                rtIcon.anchorMin = new Vector2(0.0f, 1.0f);
                rtIcon.anchorMax = new Vector2(0.0f, 1.0f);
                rtIcon.pivot = new Vector2(0.0f, 1.0f);
                UnityEngine.UI.Image imgIco = goIco.AddComponent<UnityEngine.UI.Image>();
                imgIco.sprite = icoGr.iconSmall;

                GameObject goText = new GameObject("Text");
                goText.transform.SetParent(rtPlate);
                goText.transform.localRotation = Quaternion.identity;
                goText.transform.localScale = Vector3.one;
                RectTransform rtText = goText.AddComponent<RectTransform>();
                rtText.anchorMin = new Vector2(0.0f, 1.0f);
                rtText.anchorMax = new Vector2(0.0f, 1.0f);
                rtText.pivot = new Vector2(0.0f, 1.0f);
                UnityEngine.UI.Text txt = goText.AddComponent<UnityEngine.UI.Text>();
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                this.properties.filenameFont.Apply(txt);
                txt.text = fsi.Name;

                fi.file = isfile;
                fi.icon = imgIco;
                fi.path = fsi.FullName;
                fi.text = txt;
                fi.plate = imgPlate;
                return fi;
            }

            public override void SelectFile(string file, bool append)
            {
                if (append == false)
                    this.DeselectFiles();

                FileEntry fe;
                if(this.fileAssetDirectory.TryGetValue(file, out fe) == false)
                    return;

                if(this.selectedEntries.Add(file) == false)
                    return;

                this.SelectFileEntry(fe);
            }

            public override void SelectFiles(string[] files, bool append)
            {
                if (append == false)
                    this.DeselectFiles();

                foreach(string f in files)
                    this.SelectFile(f, true);
            }

            public override void DeselectFiles()
            {
                foreach (string str in this.selectedEntries)
                {
                    FileEntry fe;
                    if(this.fileAssetDirectory.TryGetValue(str, out fe) == false)
                        continue;

                    this.DeselectFileEntry(fe);
                }

                this.selectedEntries.Clear();
            }

            void SelectFileEntry(FileEntry fe)
            {
                fe.plate.color = this.properties.fileplateSelectedColor;
                fe.plate.sprite = this.properties.fileplateSpriteSelected;
            }

            void DeselectFileEntry(FileEntry fe)
            {
                fe.plate.color = this.properties.fileplateDeselectedColor;
                fe.plate.sprite = this.properties.fileplateSprite;
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