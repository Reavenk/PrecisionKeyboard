using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpdocKeyboard : BaseHelpdoc
{
    public override void FillDocumentContent(
        PxPre.UIL.UILStack uistk, 
        Application app, 
        PaneKeyboard paneKB,
        PaneWiring paneW,
        PaneOptions paneO)
    { 

        HelpAssets ha = app.helpAssets; // Name shortening

        AddReadingText(
            uistk, 
            "The <b>keyboard</b> tab has a keyboard that can be played with, but is also the tab where exercises happen - although exercises are selected from the <b>Options</b> tab.",
            false);

        AddHeader(uistk, "Control Panels");
        AddReadingText( uistk, "It has 5 control panels that can be switched from a pulldown on the top left of the tab.");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddImagedSection(uistk, app, ha.icoKeyDispHome, "Home");
        ////////////////////
        AddReadingText(uistk, "The panel has a master volume slider - as well as metronome and BPM controls. A larger dialog to control the BPM and metronome can be accessed by tapping on the BPM value.");

        AddSubSectionText(uistk, "Master");
        AddReadingText(uistk, "The Master slider controls the volume of a key pressed. It should be set so that instruments are not too quiet, but also so that multiple notes can be played simultaneously without clipping.", false);

        AddSubSectionText(uistk, "Instrument");
        AddReadingText(uistk, "The list of currently loaded instruments. This is the same list available from the <b>Wiring</b> tab. To save/load or modify the instruments, go to the <b>Wiring</b> tab.", false);

        AddSubSectionText(uistk, "Metronome");
        AddReadingText(uistk, "Can be pressed to toggle the metronome.", false);

        AddSubSectionText(uistk, "Tap Bar");
        AddReadingText(uistk, "Can be pressed repeatedly to a beat to try to match the BPM to the rate the Tap Bar is being pressed.", false);

        AddSubSectionText(uistk, "BPM Meter");
        AddReadingText(uistk, "Shows the current BPM value of the metronome. Tap on it to open a dialog with more metronome options.", false);

        AddCommentText(uistk, "<i>Some features will be removed during exercises.</i>");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddImagedSection(uistk, app, ha.icoKeyDispExercise, "Exercise");
        //////////
        AddReadingText(uistk, "The panel is the same as the <b>Home</b> panel, except with a different icon while exercises are running.");

         ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddImagedSection(uistk, app, ha.icoKeyDispKeyLabel, "Key Label & Highlighting");
        //////////
        AddReadingText(uistk, "The keyboard has labeling features, both to label the individual key names, and to highlight the keys for various scales.");

        AddSubSectionText(uistk, "Scale");
        AddReadingText(uistk, "Scales can be highlighted on the keyboard. This a pulldown of scales that can be chosen to be highlighted.", false);

        AddSubSectionText(uistk, "Key");
        AddReadingText(uistk, "If a scale is highlighted, this pulldown can be used to select the key to highlight the scale in.", false);

        AddSubSectionText(uistk, "Labels");
        AddReadingText(uistk, "Keys can be labeled. At least one <b><color=#904747ff>rust</color></b> colored option and one <b><color=#479090ff>teal</color></b> option must be selected to see any labeling.", false);

        AddSubSectionText(uistk, "Accidentals");
        AddReadingText(uistk, "When labeling the black keys, specify whether they should be labeled as sharps (♯) or flats (♭). This option may also affect some quizzes.", false);
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.icoPullAcciSharp, "Sharps", "Label black keys as sharps."),
            new IconTextPair(ha.icoPullAcciFlat, "Flats", "Label black keys as flats."));

        AddSubSectionText(uistk, "ROYGBIVM Coloring");
        AddReadingText(uistk, "Select whether the background of the keyboard should be black, or match the coloring of the BEV scrollbar.", false);
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.icoPullRoyBlack, "Black Background", "Give the keyboard a black background."),
            new IconTextPair(ha.icoPullRoyColor, "ROYGBIVM Background", "Color code the keyboard's background to match the BEV scrollbar."));

        AddCommentText(uistk, "<i>Some extra items are provided that are not available outside of exercises, such as showing the exercise score.</i>");

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        AddImagedSection(uistk, app, ha.icoKeyKeyDim, "Key Label & Highlighting");
        //////////
        AddReadingText(uistk, "The physical dimensions of the keyboard can be modified by using the controls in this panel. They keys can be widened and the length of the black keys can be modified. The number of octaves can be changed, and the height of the horizontal scrollbar can be raised.");

        AddReadingText(uistk, "Two sliders that control the dimensions of the keys.", false);
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.icoSliderKeyWidth, "Key Width", "The width of the keys on the keyboard."),
            new IconTextPair(ha.icoSliderKeyBlackHeight, "Accidentals Length", "The length of the black keys."));

        AddSubSectionText(uistk, "Octave Spinners");
        AddReadingText(uistk, "Two spin fields are provided that can be used to change the lower and upper octave of the keyboard. The top spinner adjusts the lower octave; the bottom spinner adjusts the top octave.", false);

        AddSubSectionText(uistk, "BEV Height");
        AddReadingText(uistk, "Adjust the height of the BEV scrollbar.", false);
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.icoPullBevNormal, "Normal", "Use the standard dimension of every other scrollbar in the application."),
            new IconTextPair(ha.icoPullBevDouble, "Double", "Use double the standard dimension. This is the default."));

        AddSubSectionText(uistk, "BEV Position");
        AddReadingText(uistk, "Specify the location of the BEV scrollbar.", false);
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.icoPullBevTop, "Top", "Place the BEV scrollbar at the top of the keyboard."),
            new IconTextPair(ha.icoPullBevBottom, "Bottom", "Place the BEV scrollbar at the bottom of the keyboard."));

        AddCommentText(uistk, "<i>Some features will be removed during exercises because the keyboard may not be relevant, or would contradict difficulty settings.</i>");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddImagedSection(uistk, app, ha.icoKeyDispDataQuality, "Data & Quality");
        //////////
        AddReadingText(uistk, "The panel has controls for affecting data parameters and audio quality. Typically increasing performance will involve reducing quality so these controls allow changing those settings to whatever suits your needs and preferences.");        

        AddSubSectionText(uistk, "Samples/Sec");
        AddReadingText(uistk, "The sample rate. This can be lowered to reduce the quality and highest frequency produceable, in order to make audio synthesis less computationally expensive. It it advised to leave this alone at the default of 44100 unless you understand this control.", false);

        AddSubSectionText(uistk, "Priority");
        AddReadingText(uistk, "Controls the size of the audio buffer used to generate proceurally generated streaming audio data. This allow a tradeoff between latency (responsivness) and how safe the synthesizer is from buffer underrun.", false);
        AddIndentedTextList(
            uistk,
            new IconTextPair(ha.icoPullPriorityExperimentalPlus, "Experimental+", "Incredibly low buffer size. Best latency but a very high chance of buffer underrunning."),
            new IconTextPair(ha.icoPullPriorityExperimental, "Experimental", "Incredibly lower buffer size. Very high chance of buffer underrunning."),
            new IconTextPair(ha.icoPullPriorityResponsive, "Responsiveness", "Low buffer size. Default value."),
            new IconTextPair(ha.icoPullPriorityBalance, "Balance", "Medium buffer size. Low risk of buffer underrun, but moderate latency."),
            new IconTextPair(ha.icoPullPriorityQuality, "Quality", "Largest buffer size. Low risk of buffer underrun, but highest latency."));

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddImagedSection(uistk, app, ha.icoKeyDispMIDI, "MIDI");
        //////////
        AddReadingText(uistk, "The panel has controls for connecting to input and output MIDI ports.");

        AddSubSectionText(uistk, "Input");
        AddReadingText(uistk, "Connection to a MIDI input device. If connected, the other device can control the application to play notes using the active Wiring.", false);

        AddSubSectionText(uistk, "Output");
        AddReadingText(uistk, "Connection to a MIDI output device. If connected, the connected music software can be controled with this app's virtual keyboard.", false);

        AddSubSectionText(uistk, "Output Channel");
        AddReadingText(uistk, "The channel for output MIDI message. The purpose of the channel depends on the settings of the software receiving the MIDI message.", false);
        AddCommentText(uistk, "While not a strict rule, MIDI channel 8 is often reserved for percussion instruments.");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddHeader( uistk, "Other Controls");

        AddReadingText(uistk, "There are some other controls to the very right that are always available no matter what keyboard panel you are in.");

        AddSubSectionText(uistk, "E-Stop");
        AddReadingText(uistk, "The meter is at the top right of the keyboard tab. When pressed, it will stop ALL notes being played by the synthesizer - as well as the metronome. It will also stop any MIDI notes being played.");

        AddSubSectionText(uistk, "Meter");
        AddReadingText(uistk, "The meters is a colored vertical bar directly left of the E-Stop. This shows the volume level comming out from playing the wiring instrument. It does not take into account other audio such as exercise sounds or the metronome.");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddHeader( uistk, "BEV Scrollbar");

        AddReadingText(uistk, "BEV stands for <i>Bird's Eye View</i>. It is essentially the \"minimap\" of the entire keyboard and color coded with the ROYGBIV color scheme often used to describe the ordering of colors on a rainbow (red, orange, yellow, green, blue, indigo, violet) - with magenta added at the end. For that reason, the color scheme is referred to as ROYGBIVM");
        AddReadingText(uistk, "The most important thing to remember is that octave 4 is green since it contains Middle C (C4) and the tunning standard note A4. If you think of this as the \"Goldilocks\" octave, just remember <i>Goldilocks</i> starts with the same letter as <i>green</i>.");
        AddReadingText(uistk, "Besides being able to scroll the entire keyboard with it, the keyboard with can be scaled by using pinch multitouch gestures on it.");

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        AddHeader( uistk, "Keyboard");
        AddReadingText(uistk, "The keyboard is... the keyboard... I'm sure you can figure this one out without documentation.");
        
    }

}
