using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteContentHelpdoc<ty> : 
    BaseNoteContent
    where ty : BaseHelpdoc, new()
{
    // Should only be used to pull image resources.
    Application app;

    public NoteContentHelpdoc(Application app, string title, string description, bool fullscreen)
        : base(title, description, fullscreen)
    { 
        this.app = app;
    }

    protected override void AddScrollContent(PxPre.UIL.UILStack uiStack)
    { 
        BaseHelpdoc hd = new ty();

        hd.FillDocumentContent(uiStack,
            this.app,
            this.app.keyboardPane,
            this.app.wiringPane,
            this.app.optionsPane);
    }
}
