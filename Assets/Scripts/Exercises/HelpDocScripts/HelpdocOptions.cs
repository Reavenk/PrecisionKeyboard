using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpdocOptions : BaseHelpdoc
{
    public override void FillDocumentContent(
        PxPre.UIL.UILStack contentStack, 
        Application app, 
        PaneKeyboard paneKB,
        PaneWiring paneW,
        PaneOptions paneO)
    { 
        // Shorten the name a bit
        PxPre.UIL.UILStack uistk = contentStack;

        AddReadingText(uistk, "The options pane has controls to modify the application, as well as a list of extra content not related to playing on the keyboard or Wiring.", false);

        AddHeader(uistk, "Content");
        AddReadingText(uistk, "On the left side of the <b>Options</b> tab is the content list. It contains documents and exercises.");
        AddReadingText(uistk, "At the top of the content list is a pulldown filter.");
        AddIndentedTextList(
            uistk,
            new IconTextPair( paneO.quizMicroWildcard,  "Show All", "Show every type of content availiable."),
            new IconTextPair( paneO.quizMicroWildcard,  "Show All Exercises", "Filter the contents by only showing exercises."),
            new IconTextPair( paneO.quizMicroIdentify,  "Ear Training Exercises", "Filter the contents by only showing ear training exercises. These are exercises that involve identifying musical sounds."),
            new IconTextPair( paneO.quizMicroInput,     "Keyboard Exercises", "Filter the contents by only showing keyboard exercises. These are exercises that involve playing on the keyboard."),
            new IconTextPair( paneO.quizMicroNotes,     "Notes", "Filter the contents by only showing documents."));

        AddHeader(uistk, "Controls");

        AddReadingText(uistk, "On the left side of the <b>Options</b> tab is the content list. It contains documents and exercises.");
        AddReadingText(uistk, "For more information on the various controls, see the <b>Explanation of Options</b> document. It is available at the bottom of the controls scroll region - but a shortcut has been provided below.");

        PxPre.UIL.EleGenButton<PushdownButton> clearPrefBtn = uistk.PushButton<PushdownButton>("", 0.0f, 0);
        clearPrefBtn.border = new PxPre.UIL.PadRect(10.0f, 10.0f, 10.0f, 15.0f);
        clearPrefBtn.minSize = new Vector2(500.0f, 0.0f);
        clearPrefBtn.Button.moveOnPress = new Vector3(0.0f, -5.0f, 0.0f);
        uistk.PushHorizSizer(1, PxPre.UIL.LFlag.Grow);
            uistk.AddImage(paneO.iconExplanation, 0.0f, 0);
            uistk.AddHorizSpace(10.0f, 0, 0);
            uistk.AddText("Explanation of Options", false, 1.0f, PxPre.UIL.LFlag.AlignCenter);
        uistk.Pop();
        uistk.Pop();
        clearPrefBtn.Button.onClick.AddListener(()=>{ paneO.ShowExplainOptionsButton(clearPrefBtn.RT); });
    }
}
