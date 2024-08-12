using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseHelpdoc
{
    public const float IndentAmt = 30.0f;

    public struct IconTextPair
    {
        public Sprite icon;
        public string title;
        public string text;

        public IconTextPair(Sprite icon, string title, string text)
        { 
            this.icon = icon;
            this.title = title;
            this.text = text;
        }
    }

    public abstract void FillDocumentContent(
        PxPre.UIL.UILStack contentStack, 
        Application app, 
        PaneKeyboard paneKB,
        PaneWiring paneW,
        PaneOptions paneO);

    protected static void AddReadingText(PxPre.UIL.UILStack uiStack, string text, bool addPrespace = true)
    {
        if(addPrespace == true)
            AddParagraphSpace(uiStack);

        uiStack.AddText(text, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
    }

    protected static void AddSubSectionText(PxPre.UIL.UILStack uiStack, string text, bool addPrespace = true)
    {
        if(addPrespace == true)
            AddParagraphSpace(uiStack);

        uiStack.AddText($"<b>{text}</b>", 22, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
    }

    protected static void AddCommentText(PxPre.UIL.UILStack uiStack, string text, bool addPrespace = true)
    {
        if(addPrespace == true)
            AddParagraphSpace(uiStack);

        uiStack.AddText($"<i>{text}</i>", 14, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
    }

    protected static void AddParagraphSpace(PxPre.UIL.UILStack uiStack)
    { 
        uiStack.AddVertSpace(10.0f, 0.0f, 0);
    }

    protected static void AddPreSectionSpace(PxPre.UIL.UILStack uiStack)
    { 
        uiStack.AddVertSpace(30.0f, 0.0f, 0);
    }

    protected static void AddPreHeaderSpace(PxPre.UIL.UILStack uiStack)
    { 
        uiStack.AddVertSpace(70.0f, 0.0f, 0);
    }

    protected static void AddHeader(PxPre.UIL.UILStack uiStack, string text, bool addPrespace = true)
    { 
        if(addPrespace == true)
            AddPreHeaderSpace(uiStack);

        uiStack.AddText($"<b>\t{text}</b>", 40, true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
        uiStack.AddHorizontalSeparator();
    }

    protected static void AddImagedSection( PxPre.UIL.UILStack uiStack, Application assetSrc, Sprite sprite, string text, bool addPrespace = true)
    { 
        if(addPrespace == true)
            AddPreSectionSpace(uiStack);

        uiStack.PushImage(assetSrc.exerciseAssets.plateRounder, 0.0f, PxPre.UIL.LFlag.GrowHoriz).Chn_SetImgSliced();
        uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.Grow).Chn_Border(5.0f);

            uiStack.AddImage(sprite, 0.0f, PxPre.UIL.LFlag.AlignCenter|PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddHorizSpace(10.0f, 0.0f, 0);
            uiStack.AddText($"<b>{text}</b>", 30, true, 1.0f, PxPre.UIL.LFlag.AlignBot|PxPre.UIL.LFlag.GrowHoriz);

        uiStack.Pop();
        uiStack.Pop();
    }

    protected static void AddIndentedTextList(PxPre.UIL.UILStack uiStack, params IconTextPair [] items)
    { 
        const float itemSpacing = 10.0f;
        const float iconSpace = 10.0f;

        uiStack.PushHorizSizer(0.0f, PxPre.UIL.LFlag.GrowHoriz);
            uiStack.AddHorizSpace(IndentAmt, 0.0f, 0);
            uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.GrowHoriz);

            for(int i = 0; i < items.Length; ++i)
            { 
                if(i != 0)
                    uiStack.AddVertSpace(itemSpacing, 0.0f, 0);

                uiStack.PushHorizSizer(1.0f, PxPre.UIL.LFlag.GrowHoriz);
                    uiStack.AddImage(items[i].icon, 0.0f, 0);
                    uiStack.AddHorizSpace(iconSpace, 0.0f, 0);
                    uiStack.PushVertSizer(1.0f, PxPre.UIL.LFlag.Grow);
                        uiStack.AddText( $"<b>• {items[i].title}</b>", true, 0.0f, PxPre.UIL.LFlag.GrowHoriz);
                        uiStack.AddText( items[i].text, true, 1.0f, PxPre.UIL.LFlag.GrowHoriz);
                    uiStack.Pop();
                uiStack.Pop();
            }

            uiStack.Pop();
        uiStack.Pop();
    }
}
