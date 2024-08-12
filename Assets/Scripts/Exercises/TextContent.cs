using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TextContent : BaseNoteContent
{
    public abstract string GetContent();

    public TextContent(string title, string description, bool fullscreen)
        : base(title, description, fullscreen)
    { }

    protected override void AddScrollContent(PxPre.UIL.UILStack uiStack)
    { 
        string content = this.GetContent();
        uiStack.AddText(content, true, 1.0f, PxPre.UIL.LFlag.GrowHoriz);
    }
}
