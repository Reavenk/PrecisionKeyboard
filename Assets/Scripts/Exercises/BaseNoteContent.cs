using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseNoteContent : BaseContent
{
    public readonly string contentTitle;
    public readonly string contentDescription;

    public override string title => this.contentTitle;
    public override string description => this.contentDescription;
    public override string longDescription => this.contentDescription;

    bool fullscreen = false;

    public BaseNoteContent(string title, string description, bool fullscreen)
    { 
        this.contentTitle = title;
        this.contentDescription = description;
        this.fullscreen = fullscreen;
    }

    public override ContentType GetExerciseType()
    {
        return ContentType.Document;
    }

    public override PxPre.UIL.Dialog CreateDialog(ExerciseAssets assets)
    {
        PxPre.UIL.Dialog dlg = null;

        if(this.fullscreen == true)
        {
            dlg = 
                this.app.dlgSpawner.CreateDialogTemplate(
                    new Vector2(-1, 0.0f), 
                    PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.Grow, 
                    0.0f);
        }
        else
        { 
            dlg = 
                this.app.dlgSpawner.CreateDialogTemplate(
                    new Vector2(650, 0.0f), 
                    PxPre.UIL.LFlag.AlignCenter, 
                    0.0f);
        }

        dlg.dialogSizer.border = new PxPre.UIL.PadRect(20.0f);
        dlg.AddDialogTemplateTitle(this.title);

        PxPre.UIL.UILStack uiStack = new PxPre.UIL.UILStack(this.app.uiFactory, dlg.rootParent, dlg.contentSizer);
        uiStack.PushImage(this.app.exerciseAssets.scrollRectBorder, 1.0f, PxPre.UIL.LFlag.Grow).Chn_SetImgSliced();
        uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);

            PxPre.UIL.EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, PushdownScrollbar> vertScr = 
            uiStack.PushVertScrollRect<UnityEngine.UI.ScrollRect, PushdownScrollbar>(1.0f, PxPre.UIL.LFlag.Grow);

            PxPre.UIL.EleBoxSizer sizer = uiStack.PushVertSizer();
                sizer.border = new PxPre.UIL.PadRect(10.0f);

                PxPre.UIL.UILStack uiStackInner = 
                    new PxPre.UIL.UILStack(
                        this.app.uiFactory, 
                        vertScr, 
                        sizer);

                this.AddScrollContent(uiStackInner);

            uiStack.Pop();
            uiStack.Pop();

        uiStack.Pop();
        uiStack.Pop();

        dlg.AddDialogTemplateSeparator();
        dlg.AddDialogTemplateButtons(new PxPre.UIL.DlgButtonPair("Close", null));

        dlg.host.LayoutInRTSmartFit();

        DoNothing dn = vertScr.RT.gameObject.AddComponent<DoNothing>();
        dn.StartCoroutine( vertScr.SetVertScrollLater(1.0f));

        return dlg;
    }

    protected abstract void AddScrollContent(PxPre.UIL.UILStack uiStack);
}
