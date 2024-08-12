using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class Dialog
        {
            public struct OptionsButton
            { 
                public UnityEngine.UI.Button button;
                public UnityEngine.UI.Text text;
            }

            public Factory uifactory;

            /// <summary>
            /// The host for the screen-sized root of the entire dialog system
            /// </summary>
            public PxPre.UIL.EleHost host;

            /// <summary>
            /// The vertical box sizer for the dialog. This includes both the title, and the body.
            /// </summary>
            public PxPre.UIL.EleBoxSizer dialogSizer;

            /// <summary>
            /// The RectTransform element for the title.
            /// </summary>
            public PxPre.UIL.EleBaseRect rootTitle;

            /// <summary>
            /// The RectTransform element for all root content.
            /// </summary>
            public PxPre.UIL.EleBaseRect rootParent;

            /// <summary>
            /// The sizer for the titelbar
            /// </summary>
            public PxPre.UIL.EleBaseSizer titleSizer;

            /// <summary>
            /// The sizer for custom UI content should go, exluding 
            /// progression button.
            /// </summary>
            public PxPre.UIL.EleBaseSizer contentSizer;

            /// <summary>
            /// The sizer for custom UI content should go, including
            /// progression button.
            /// </summary>
            public PxPre.UIL.EleBaseSizer bodySizer;

            public List<OptionsButton> options = new List<OptionsButton>();
            private PxPre.UIL.EleBoxSizer buttonRow = null;

            public System.Action onClose;

            public void DestroyDialog()
            {
                onClose?.Invoke();
                GameObject.Destroy(this.host.RT.gameObject);
            }

            public OptionsButton[] AddDialogTemplateButtons(float minDlgButtonWidth, params DlgButtonPair [] bpair)
            {
                List<OptionsButton> rets = new List<OptionsButton>();

                if(this.buttonRow == null)
                {
                    this.buttonRow = 
                        this.uifactory.HorizontalSizer(
                            this.bodySizer,
                            0.0f,
                            PxPre.UIL.LFlag.GrowHoriz);
                }

                for (int i = 0; i < bpair.Length; ++i)
                {
                    DlgButtonPair bp = bpair[i];

                    float prop = 0.0f;
                    if (i == bpair.Length - 1)
                        prop = 1.0f;

                    PxPre.UIL.EleGenButton<PushdownButton> eleBtn =
                        this.uifactory.CreateButton<PushdownButton>(
                            this.rootParent,
                            bp.label);

                    if (minDlgButtonWidth > 0.0f)
                        eleBtn.minSize = new Vector2(minDlgButtonWidth, eleBtn.minSize.y);

                    eleBtn.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
                    eleBtn.Button.onClick.AddListener(
                        () =>
                        {
                            bool close = true;
                            if (bp.action != null)
                                close = bp.action(eleBtn.Button);

                            if (close == true)
                                this.DestroyDialog();
                        });
                    this.buttonRow.Add(eleBtn, prop, PxPre.UIL.LFlag.GrowHoriz);

                    OptionsButton ob = new OptionsButton();
                    ob.button = eleBtn.Button;
                    ob.text = eleBtn.text;
                    //
                    this.options.Add(ob);
                    rets.Add(ob);
                }

                return rets.ToArray();
            }

            public void AddDialogTemplateTitle(string title, Sprite icon = null)
            {

                EleBaseSizer sizerTitle = this.titleSizer;
                PxPre.UIL.EleText eleTitleTxt = 
                    uifactory.CreateText(
                        this.rootTitle, 
                        title, 
                        false);

                if(icon != null)
                {
                    PxPre.UIL.EleBoxSizer ebs = uifactory.HorizontalSizer(this.titleSizer, 1.0f, LFlag.Grow);
                    sizerTitle = ebs;

                    PxPre.UIL.EleImg imgIcon = 
                        uifactory.CreateImage(
                            this.rootTitle,
                            icon,
                            "");

                    imgIcon.minSize = icon.rect.size;

                    ebs.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
                    ebs.Add(imgIcon, 0.0f, LFlag.AlignVertCenter);
                    ebs.Add(new PxPre.UIL.EleSpace(10.0f), 0.0f, 0);
                }

                eleTitleTxt.text.fontSize = 25;
                sizerTitle.Add(eleTitleTxt, 1.0f, PxPre.UIL.LFlag.AlignCenter);
            }

            public void AddDialogTemplateSeparator()
            {
                PxPre.UIL.EleSeparator sep = uifactory.CreateHorizontalSeparator(this.rootParent);
                this.bodySizer.Add(sep, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
            }

            public OptionsButton[]  AddDialogTemplateButtons(params DlgButtonPair [] bpair)
            {
                return this.AddDialogTemplateButtons(100.0f, bpair);
            }

            public PxPre.UIL.UILStack CreateContentStack(PxPre.UIL.Factory f)
            { 
                return new PxPre.UIL.UILStack(f, this.rootParent, this.contentSizer);
            }
        }

        public struct DlgButtonPair
        {
            public string label;
            public System.Func<UnityEngine.UI.Button, bool> action;

            public DlgButtonPair(string label, System.Func<UnityEngine.UI.Button, bool> action)
            {
                this.label = label;
                this.action = action;
            }
        }

        [System.Serializable]
        public class DialogSpawner
        {
            [UnityEngine.SerializeField]
            Factory factory;

            public Factory UIFactory {get{return this.factory; } }

            public float minDialogWidth = 100.0f;
            public Sprite titleSprite;
            public Sprite bodySprite;

            public System.Action<Dialog> onCreateDialog;

            public DialogSpawner(Factory factory)
            { 
                this.factory = factory;
            }

            public Dialog CreateDialogTemplate(Vector2 bodySize, PxPre.UIL.LFlag flags, float proportion)
            {
                // The backplate
                UnityEngine.UI.Image bplate;
                RTQuick rtqBP = RTQuick.CreateGameObjectWithImage(CanvasSingleton.canvas.transform, "Backplate", out bplate);
                // The UILayout host for the backplate
                PxPre.UIL.EleHost host = new PxPre.UIL.EleHost(bplate.rectTransform, true);
                bplate.Color(0.0f, 0.0f, 0.0f, 0.8f).RTQ().ExpandParentFlush().CenterPivot();
                // The sizer for the internal content.
                PxPre.UIL.EleBoxSizer bsMain = this.factory.VerticalSizer(host);

                // Vertical sizer containing the entire dialog
                PxPre.UIL.EleBoxSizer bsDlg = new PxPre.UIL.EleBoxSizer(bsMain, PxPre.UIL.Direction.Vert, proportion, flags|LFlag.GrowVertOnCollapse);

                PxPre.UIL.EleImg eleTitle = new PxPre.UIL.EleImg(host, this.titleSprite, new Vector2(500.0f, 50.0f));
                eleTitle.Img.ImageType(UnityEngine.UI.Image.Type.Sliced).Color(0.8f, 0.8f, 0.8f);
                bsDlg.Add(eleTitle, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                PxPre.UIL.EleBoxSizer bsTitleSzr = this.factory.HorizontalSizer(eleTitle);

                PxPre.UIL.EleImg eleDlg = this.factory.CreateImage(host, this.bodySprite, bodySize, "");

                //
                bsDlg.Add(eleDlg, 1.0f, PxPre.UIL.LFlag.Grow);
                PxPre.UIL.EleBoxSizer bsBody = this.factory.VerticalSizer(eleDlg);
                bsBody.padding = 5.0f;
                bsBody.border = new PxPre.UIL.PadRect(10.0f);
                eleDlg.Img.type = UnityEngine.UI.Image.Type.Sliced;

                PxPre.UIL.EleBoxSizer bsContent = this.factory.VerticalSizer(bsBody, 1.0f, PxPre.UIL.LFlag.Grow);

                Dialog ret = new Dialog();
                ret.uifactory = this.factory;
                ret.host = host;
                ret.dialogSizer = bsDlg;
                ret.rootTitle = eleTitle;
                ret.titleSizer = bsTitleSzr;
                ret.bodySizer = bsBody;
                ret.contentSizer = bsContent;
                ret.rootParent = eleDlg;

                this.onCreateDialog?.Invoke(ret);

                return ret;
            }

            public Dialog CreateDialogTemplate(bool constrained = true, bool fullScreen = false)
            {
                Vector2 bodySize = new Vector2(500.0f, 400.0f);
                if (constrained == false)
                    bodySize = new Vector2(-1.0f, -1.0f);

                float dlgProp = 0.0f;
                PxPre.UIL.LFlag dlgFlag = PxPre.UIL.LFlag.AlignCenter;
                if (fullScreen == true)
                {
                    dlgProp = 1.0f;
                    dlgFlag |= PxPre.UIL.LFlag.Grow;
                }

                if (constrained == false)
                    dlgFlag |= PxPre.UIL.LFlag.Grow;

                return CreateDialogTemplate(bodySize, dlgFlag, dlgProp);
            }

            public Dialog CreateDialogTemplate(
                string title,
                string message,
                params DlgButtonPair[] bpair)
            {
                Dialog dlg = this.CreateDialogTemplate();

                // Insert title text
                dlg.AddDialogTemplateTitle(title);

                if (string.IsNullOrEmpty(message) == false)
                {
                    PxPre.UIL.EleText exeTxt = this.factory.CreateText(dlg.rootParent, title, 20, false);
                    exeTxt.text.text = message;
                    dlg.contentSizer.Add(exeTxt, 0.0f, PxPre.UIL.LFlag.AlignCenter);
                }

                // Add separator
                if (bpair.Length > 0)
                {
                    dlg.AddDialogTemplateSeparator();
                    dlg.AddDialogTemplateButtons(bpair);
                }

                return dlg;
            }

            

        }
    }
}
