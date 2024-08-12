using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace FileBrowse
    {
        [CreateAssetMenuAttribute(fileName = "FileBrowseProp", menuName = "FileBrowse Properties")]
        public class FileBrowseProp : ScriptableObject
        {
            [System.Serializable]
            public struct ContentPadding
            { 
                public float left;
                public float top;
                public float right;
                public float bottom;
            }

            [System.Serializable]
            public struct IconGroup
            {
                public Sprite iconSmall;
                public Sprite iconMed;
                public Sprite iconLarge;
            }

            [System.Serializable]
            public struct IconExt
            { 
                public IconGroup icon;
                public List<string> extensions;
            }

            [System.Serializable]
            public class SelectableStyle
            { 
                public UnityEngine.UI.Selectable.Transition transition = 
                    UnityEngine.UI.Selectable.Transition.ColorTint;

                public UnityEngine.UI.ColorBlock colors = 
                    UnityEngine.UI.ColorBlock.defaultColorBlock;

                public Color color = Color.white;
                public Sprite sprite;
                public UnityEngine.UI.SpriteState spriteState;

                public void ApplySel(UnityEngine.UI.Selectable selectable)
                { 
                    selectable.spriteState  = this.spriteState;
                    selectable.transition = this.transition;
                    selectable.colors = this.colors;
                }

                public void ApplyG(UnityEngine.UI.Graphic graphic)
                { 
                    graphic.color = this.color;
                }

                public void Apply(
                    UnityEngine.UI.Selectable sel, 
                    UnityEngine.UI.Image img, 
                    bool assignTG = true,
                    UnityEngine.UI.Image.Type ? imgTy = UnityEngine.UI.Image.Type.Sliced)
                { 
                    this.ApplySel(sel);
                    this.ApplyG(img);
                    img.sprite = this.sprite;

                    if(assignTG == true)
                        sel.targetGraphic = img;

                    if(imgTy.HasValue == true)
                        img.type = imgTy.Value;
                }

                    public GameObject CreateButton(
                    Transform parent, 
                    string objName, 
                    out UnityEngine.UI.Image img,
                    out UnityEngine.UI.Button btn,
                    params System.Type [] btnCmp)
                {
                    GameObject go;
                    if (btnCmp != null)
                        go = new GameObject(objName, btnCmp);
                    else
                        go = new GameObject(objName);
                
                    go.transform.SetParent(parent);
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                
                    img = go.GetComponent<UnityEngine.UI.Image>();
                    if(img == null)
                        img = go.AddComponent<UnityEngine.UI.Image>();
                
                    btn = go.AddComponent<UnityEngine.UI.Button>();
                    if(btn == null)
                        btn = go.AddComponent<UnityEngine.UI.Button>();
                
                    this.Apply(btn, img, true, UnityEngine.UI.Image.Type.Sliced);
                
                    return go;
                }
            }

            [System.Serializable]
            public class TextStyle
            { 
                public Font font;
                public Color color = Color.black;
                public int size = 14;

                public void Apply(UnityEngine.UI.Text txt)
                { 
                    txt.font = this.font;
                    txt.color = this.color;
                    txt.fontSize = this.size;
                }

                public GameObject Create(Transform parent, out UnityEngine.UI.Text txt, string str, string objName)
                { 
                    GameObject go = new GameObject(objName);
                    go.transform.SetParent(parent);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    txt = go.AddComponent<UnityEngine.UI.Text>();
                    txt.text = str;
                    this.Apply(txt);

                    return go;
                }
            }

            public static UnityEngine.UI.Image CreateCenteredImage(Sprite s, Transform parent, Vector2 anchor, string name = "")
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
                img.sprite = s;
                RectTransform rtImg = img.rectTransform;
                rtImg.anchorMin = new Vector2(0.5f, 0.5f);
                rtImg.anchorMax = new Vector2(0.5f, 0.5f);
                rtImg.pivot = new Vector2(0.5f, 0.5f);
                rtImg.anchoredPosition = anchor;
                rtImg.sizeDelta = s.rect.size;

                return img;
            }

            public static UnityEngine.UI.Image CreateCenteredImage(Sprite s, Transform parent, string name = "")
            {
                UnityEngine.UI.Image img = CreateCenteredImage(s, parent, Vector2.zero, name);
                return img;
            }

            public IconExt [] iconDeclarations;

            public IconGroup iconFolder;
            public IconGroup iconDefault;
            public Dictionary<string, IconGroup> icons = null;

            public IconGroup GetExtIcon(string ext)
            { 
                if(this.icons == null)
                {
                    this.icons = new Dictionary<string, IconGroup>();

                    foreach (IconExt icoe in this.iconDeclarations)
                    {
                        foreach(string str in icoe.extensions)
                            this.icons.Add(str.ToLower(), icoe.icon);
                    }
                }
                IconGroup ret;
                if(this.icons.TryGetValue(ext, out ret) == true)
                    return ret;

                return this.iconDefault;
            }


            //
            //      FILENAME LISTINGS
            ////////////////////////////////////////////////////////////////////////////////
            ///
            public TextStyle filenameFont;

            //
            //      FILE SLIDER
            ////////////////////////////////////////////////////////////////////////////////
            public Sprite sliderSprite;
            public SelectableStyle sliderThumb;
            public float fileScrollSensitivity = 50.0f;

            //
            //      FILE PLATES
            ////////////////////////////////////////////////////////////////////////////////
            public Color fileplateSelectedColor = Color.green;
            public Color fileplateDeselectedColor = Color.white;
            public Sprite fileplateSprite;
            public Sprite fileplateSpriteSelected;

            //
            //      FILE INPUT INFO
            ////////////////////////////////////////////////////////////////////////////////
            public TextStyle inputLabel;
            public TextStyle inputText;
            
            public float inputLabelInputPadding = 10.0f;
            public float inputPadding = 10.0f;
            public Sprite inputPlateSprite;

            //
            //      CONFIRM BUTTONS
            ////////////////////////////////////////////////////////////////////////////////
            [UnityEngine.SerializeField]
            private SelectableStyle confirmButtons;

            public ContentPadding buttonPadding;

            //
            //      CRUMB BUTTONS
            ////////////////////////////////////////////////////////////////////////////////
            public SelectableStyle crumbButtons;

            public SelectableStyle navButtons;
            public Sprite navBackSprite;
            public Sprite navForwardSprite;
            public Sprite navUpSprite;
            public TextStyle navCrumbFont;
            public float navCrumbPadding = 10.0f;

            ////////////////////////////////////////////////////////////////////////////////

            public virtual UnityEngine.UI.Button CreateButton(Transform parent, string text, params System.Type [] btnCmp)
            { 
                return this.CreateTypedButton<UnityEngine.UI.Button>(parent, text);
            }

            protected BtnTy CreateTypedButton<BtnTy>(Transform parent, string text, params System.Type [] btnCmp) where BtnTy : UnityEngine.UI.Button
            {
                GameObject goBtn;
                
                if(btnCmp != null)
                    goBtn = new GameObject("FileBrowseBtn_" + text, btnCmp);
                else
                    goBtn = new GameObject("FileBrowseBtn_" + text);

                goBtn.transform.SetParent(parent, false);
                UnityEngine.UI.Image btnImg = goBtn.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtBtn = btnImg.rectTransform;
                BtnTy btn = goBtn.AddComponent<BtnTy>();
                btn.targetGraphic = btnImg;

                this.confirmButtons.Apply(btn, btnImg);

                if(string.IsNullOrEmpty(text) == false)
                { 
                    GameObject goTxt = new GameObject("OKText");
                    goTxt.transform.SetParent(rtBtn.transform);
                    goTxt.transform.localScale = Vector3.one;
                    goTxt.transform.localRotation = Quaternion.identity;
                    UnityEngine.UI.Text txtOK = goTxt.AddComponent<UnityEngine.UI.Text>();
                    txtOK.text = text;
                    this.inputLabel.Apply(txtOK);
                    txtOK.alignment = TextAnchor.MiddleCenter;
                    RectTransform rtOKTxt = txtOK.rectTransform;
                    rtOKTxt.anchorMin = Vector2.zero;
                    rtOKTxt.anchorMax = Vector2.zero;
                    rtOKTxt.pivot = new Vector2(0.5f, 0.5f);
                    rtOKTxt.offsetMin = new Vector2(buttonPadding.left, buttonPadding.bottom);
                    rtOKTxt.offsetMax = new Vector2(-buttonPadding.top, -buttonPadding.right);
                }

                return btn;
            }
        }
    }
}