using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpdocWiring : BaseHelpdoc
{
    public override void FillDocumentContent(
        PxPre.UIL.UILStack uistk, 
        Application app, 
        PaneKeyboard paneKB,
        PaneWiring paneW,
        PaneOptions paneO)
    { 
        HelpAssets ha = app.helpAssets; // Name shortening

        AddReadingText(uistk, "The Wiring system is how synthesizers are represented in this application. A document can have many of them, which are shown in the Instrument pulldown.", false);
        AddReadingText(uistk, "For a more thorough explanation from scratch, including some tutorials on audio synthesis, see <b>Included Examples/Wiring Tutorial</b> under the Hamburger.");
        AddReadingText(uistk, "Every time the application is started, the <b>Starting Instruments</b> document is opened. It is an <b>Included Examples</b> document that cannot be changed and this document cannot be edited and replaced - although edited copies of it can be saved. Feel free and bold to play with <b>Included Examples</b> documents because they cannot be ruined and can always be reloaded.");

        //////////////////////////////////////////////////
        AddHeader(uistk, "Instrument");
        //////////
        AddReadingText(uistk, "A pulldown that lists the Wiring instruments in the current document. This is similar to the Instruments pulldown in the <b>Keyboard</b> tab except it has additional editing features.");
        AddReadingText(uistk, "The Wiring name shown is called the \"active Wiring\" and is the synth that will be played when keys are pressed, and the target of certain menu actions.");

        AddIndentedTextList(
            uistk,
            new IconTextPair(paneW.pulldownRename,              "Rename",                           "Change the name of the active wiring."),
            new IconTextPair(paneW.pulldownClone,               "Duplicate",                        "Duplicate the active wiring and make the clone the new active wiring."),
            new IconTextPair(paneW.icoOrganizeMoveUp,           "Organize/Move Up",                 "Move the active wiring up on the list of Wirings in the document. This option will only be available if it is not already the top Wiring."),
            new IconTextPair(paneW.icoOrganizeMoveDown,         "Organize/Move Down",               "Move the active wiring down on the list of Wirings in the document. This option will only be available if it is not already the bottom Wiring"),
            new IconTextPair(paneW.icoOrganizeMoveTop,          "Organize/Move To Top",             "Move the active Wiring to the top of the list of Wirings in the document. This option will only be available if it is not already the top Wiring."),
            new IconTextPair(paneW.icoOrganizeMoveBottom,       "Organize/Move To Bottom",          "Move the active Wiring to the bottom of the list of Wirings in the document. This option will only be available if it is not already the bottom Wiring."),
            new IconTextPair(paneW.icoOrganizeName,             "Organize/Organize All By Name",    "Sort all the Wirings in the document in alphabetical order."),
            new IconTextPair(paneW.icoOrganizeIcon,             "Organize/Organize All By Icon",    "Sort all the Wirings in the document by alphabetical order of their icon name - Wirings sharing the same icon will be sorted by alphabetical order within their icon group."),
            new IconTextPair(paneW.GetCategoryIcon(WiringDocument.Category.Unlabled), "Organize/Set Icon *", "Assign the Wiring an icon next to its name when shown in the menus. The icon does not do anything functional and is for organization purposes only."));

        //////////////////////////////////////////////////
        AddHeader(uistk, "Preview Keyboard");
        //////////

        AddReadingText(uistk, "On the bottom is a preview keyboard that plays the 4th octave. It is not designed to be an effective performing keyboard and is only provided for quick testing. On the very right is an E-Stop button.");


        //////////////////////////////////////////////////
        AddHeader(uistk, "Undo/Redo");
        //////////

        AddReadingText(uistk, "To the left of the Instrument pulldown are the undo and redo buttons. This can be used to undo editing operations.");
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.btnUndo, "Undo", "Undo a previous operation. This button will be disabled if there is nothing to undo."),
            new IconTextPair(ha.btnRedo, "Redo", "Redo a previously undo operation. This button will be disabled if there is nothing to redo."));


        //////////////////////////////////////////////////
        AddHeader(uistk, "Nodes");

        AddReadingText(uistk, "The list of nodes that can be used to author a Wiring.");

        AddReadingText(uistk, "The nodes are organized into categories. The names of the various categories in the Nodes list can be tapped to open a dialog with more information about them.");
        AddReadingText(uistk, "The individual nodes can be dragged and dropped into the canvas to place them in the Wiring. The nodes can also be tapped to bring up documentation about them.");
        AddReadingText(uistk, "For more information on editing, and how to start using the nodes, check out the Wiring tutorial.");
        AddReadingText(uistk, "To the right of the Nodes section is a sash that can be dragged to redistribute the space between the Notes section and the Canvas.");

        //////////////////////////////////////////////////
        AddHeader(uistk, "Canvas");
        //////////
        AddReadingText(uistk, "The contents of the active wiring.");
        AddReadingText(uistk, "Empty space can be dragged to scroll. Multitouch pinch gestures on empty space can be used to zoom in and out.");
        AddReadingText(uistk, "Node parameters can be tapped to edit");
        AddReadingText(uistk, "Empty space on nodes (where the parameters are not present) can be dragged to move nodes.");
        AddReadingText(uistk, "Nodes can be connected by dragging a node's output (on its right) to another node's input (on a node's left).");

        //////////////////////////////////////////////////
        AddHeader(uistk, "Hamburger (☰)");
        //////////
        AddReadingText(uistk, "A list of various document options including saving and loading the document.");
        AddIndentedTextList(
            uistk,
            new IconTextPair(paneW.icoAddNewWiring,     "Add New Wiring",      "Create a new wiring in the document."),
            new IconTextPair(paneW.icoSave,             "Save",                "Save the document."),
            new IconTextPair(paneW.icoSaveAs,           "Save As",             "Save the document under a name selected from a file dialog."),
            new IconTextPair(paneW.icoOpen,             "Open",                "Discard the current document and open a new document from file."),
            new IconTextPair(paneW.icoAppend,           "Append",              "Add the contents of a selected document file to the current document."),
            new IconTextPair(paneW.icoOpenInternalDoc,  "Included Examples/*", "Discard the current document and open an example document that comes with the program."),
            new IconTextPair(paneW.icoDelCur,           "Delete Wiring",       "Delete the active Wiring from the document."),
            new IconTextPair(paneW.icoNewDoc,           "New Document",        "Discard the current document and create a new empty document."));
        
    }
}
