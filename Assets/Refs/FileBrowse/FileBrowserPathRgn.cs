using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public class FileBrowserPathRgn : FileBrowserArea
        {
            UnityEngine.UI.Text label;
            UnityEngine.UI.InputField input;

            FileBrowseProp prop;
            FileBrowser browser;

            public void Create(PxPre.FileBrowse.FileBrowseProp prop, FileBrowser browser)
            { 
                this.prop = prop;
                this.browser = browser;

                GameObject goLabel = new GameObject("FontLabel");
                goLabel.transform.SetParent(this.transform);
                goLabel.transform.localScale = Vector3.one;
                goLabel.transform.localRotation = Quaternion.identity;
                UnityEngine.UI.Text txtLabel = goLabel.AddComponent<UnityEngine.UI.Text>();
                this.prop.inputLabel.Apply(txtLabel);
                txtLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                txtLabel.verticalOverflow = VerticalWrapMode.Overflow;
                txtLabel.alignment = TextAnchor.MiddleLeft;
                txtLabel.text = "Filename";
                RectTransform rtLabel = txtLabel.rectTransform;
                TextGenerationSettings tgsLabel = txtLabel.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                tgsLabel.scaleFactor = 1.0f;
                TextGenerator tgLabel = txtLabel.cachedTextGenerator;
                float labelWidth = tgLabel.GetPreferredWidth(txtLabel.text, tgsLabel);
                rtLabel.pivot = new Vector2(0.0f, 0.5f);
                rtLabel.anchorMin = new Vector2(0.0f, 0.0f);
                rtLabel.anchorMax = new Vector2(0.0f, 1.0f);
                rtLabel.offsetMin = new Vector2(0.0f, 0.0f);
                rtLabel.offsetMax = new Vector2(labelWidth, 0.0f);

                GameObject goInput = new GameObject("Input");
                goInput.transform.SetParent(this.transform);
                goInput.transform.localScale = Vector3.one;
                goInput.transform.localRotation = Quaternion.identity;
                UnityEngine.UI.Image imgInput = goInput.AddComponent<UnityEngine.UI.Image>();
                imgInput.type = UnityEngine.UI.Image.Type.Sliced;
                imgInput.sprite = this.prop.inputPlateSprite;
                RectTransform rtInput = imgInput.rectTransform;
                rtInput.anchorMin = Vector2.zero;
                rtInput.anchorMax = Vector2.one;
                rtInput.offsetMin = new Vector2(labelWidth + this.prop.inputLabelInputPadding, 0.0f);
                rtInput.offsetMax = Vector2.zero;

                GameObject goInputText = new GameObject("InputText");
                goInputText.transform.SetParent(goInput.transform);
                goInputText.transform.localScale = Vector3.one;
                goInputText.transform.localRotation = Quaternion.identity;
                UnityEngine.UI.Text textInput = goInputText.AddComponent<UnityEngine.UI.Text>();
                this.prop.inputText.Apply(textInput);
                textInput.alignment = TextAnchor.MiddleLeft;
                textInput.verticalOverflow = VerticalWrapMode.Overflow;
                RectTransform rtText = textInput.rectTransform;
                rtText.anchorMin = Vector2.zero;
                rtText.anchorMax = Vector2.one;
                rtText.pivot = new Vector2(0.0f, 0.5f);
                rtText.offsetMin = new Vector2(this.prop.inputPadding, 0.0f);
                rtText.offsetMax = new Vector2(-this.prop.inputPadding, 0.0f);

                this.input = goInput.AddComponent<UnityEngine.UI.InputField>();
                this.input.textComponent = textInput;
            }

            public override void SelectFile(string file, bool append)
            {
                if(string.IsNullOrEmpty(file) == true)
                    this.input.text = string.Empty;
                else
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    this.input.text = fi.Name;
                }
            }

            public override void SelectFiles(string[] files, bool append)
            {
            }

            public override void DeselectFiles()
            {
            }

            public override void ViewDirectory(string directory, FileFilter filter)
            {
            }

            public override string OnOK()
            {
                string inptxt = this.input.text;

                if(string.IsNullOrEmpty(inptxt) == true)
                    return string.Empty;

                if(System.IO.Path.IsPathRooted(inptxt) == true)
                    return inptxt;

                return System.IO.Path.Combine( this.browser.CurDirectory, inptxt);
            }

            public override string[] OnOKMulti()
            {
                return null;
            }
        }
    }
}

