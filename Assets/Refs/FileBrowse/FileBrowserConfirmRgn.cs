using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public class FileBrowserConfirmRgn : FileBrowserArea
        {
            public UnityEngine.UI.Image okImg;
            public UnityEngine.UI.Button okBtn;

            public UnityEngine.UI.Image cancelImg;
            public UnityEngine.UI.Button cancelBtn;

            FileBrowseProp prop;
            FileBrowser browser;

            public void Create(FileBrowseProp prop, FileBrowser browser)
            {
                this.prop = prop;
                this.browser = browser;

                this.okBtn = prop.CreateButton(this.transform, "OK");
                this.okImg = this.okBtn.image;
                this.okBtn.onClick.AddListener(()=>{ this.browser.OK(); });
                RectTransform rtOK = this.okImg.rectTransform;
                rtOK.anchorMin = new Vector2(0.5f, 0.0f);
                rtOK.anchorMax = new Vector2(1.0f, 1.0f);
                rtOK.pivot = new Vector2(0.0f, 1.0f);
                rtOK.offsetMin = Vector2.zero;
                rtOK.offsetMax = Vector2.zero;

                ////////////////////////////////////////////////////////////////////////////////
                
                this.cancelBtn = prop.CreateButton(this.transform, "Cancel");
                this.cancelImg = this.cancelBtn.image;
                this.cancelBtn.onClick.AddListener(()=>{ this.browser.Cancel(); });
                RectTransform rtC = this.cancelImg.rectTransform;
                rtC.anchorMin = new Vector2(0.0f, 0.0f);
                rtC.anchorMax = new Vector2(0.5f, 1.0f);
                rtC.pivot = new Vector2(0.0f, 1.0f);
                rtC.offsetMin = Vector2.zero;
                rtC.offsetMax = Vector2.zero;
                //this.prop.confirmButtons.Apply(this.cancelBtn, this.cancelImg);
            }

            public override void SelectFile(string file, bool append)
            {
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
                return "";
            }

            public override string[] OnOKMulti()
            { 
                return null;
            }
        }
    }
}
