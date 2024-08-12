// <copyright file="BPMPad.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>The logic for the BPM and metronome dialog.</summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The logic for the BPM and metronom dialog.
/// </summary>
public class BPMPad : MonoBehaviour
{
    /// <summary>
    /// The image of the metronom button, used to highligh it when the 
    /// metronome is playing.
    /// </summary>
    public UnityEngine.UI.Image metronomeButtonImg;

    /// <summary>
    /// The input text for the BPM
    /// 
    /// This is both for the typed value via the buttons, and the editable
    /// version via normal keyboard typing.
    /// </summary>
    public UnityEngine.UI.InputField bpmInput;

    /// <summary>
    /// The decimal button
    /// </summary>
    public UnityEngine.UI.Button dotButton;

    /// <summary>
    /// The back button to edit the text of the BPM
    /// </summary>
    public UnityEngine.UI.Button backButton;

    /// <summary>
    ///  The various number buttons
    /// </summary>
    public List<UnityEngine.UI.Button> numberButtons;

    public UnityEngine.UI.Button plusTenButton;     // The button to raise the BPM by +10
    public UnityEngine.UI.Button plusOneButton;     // The button to raise the BPM by +1
    public UnityEngine.UI.Button minusOneButton;    // The button to lower the BPM by -1
    public UnityEngine.UI.Button minusTenButton;    // The button to lower the BPM by -10

    /// <summary>
    /// The cached starting BPM when the dialog was first opened. Used to restore the
    /// BPM if "Cancel" is pressed on the dialog.
    /// </summary>
    float originalBPM = 120.0f;

    /// <summary>
    /// Reference to the main application.
    /// </summary>
    Application app;

    public PulldownInfo pulldownBeat;

    TweenUtil tweenUtil;

    public UnityEngine.UI.Slider exerciseVolumeSlider;

    public void Initialize(Application app)
    { 
        this.app = app;
        this.originalBPM = app.BeatsPerMinute();
        if (this.app.IsMetronomePlaying() == true)
        {
            this.app.SetMetronomeButtonImage(
                this.metronomeButtonImg, 
                true);

        }
        this.SetBPMText(app.GetBPMString(), false);

        this.bpmInput.onValueChanged.AddListener(this.OnInput_EditBPM);
        this.bpmInput.onEndEdit.AddListener(this.OnInput_EndEditBPM);
        this.bpmInput.onValidateInput = this.OnValidateInput;

        this.pulldownBeat.text.text = 
            app.metronomePatterns[app.metronomeIdx].name;

        this.exerciseVolumeSlider.value = 
            this.app.GetMetronomeVolume();

        this.tweenUtil = new TweenUtil(this);
    }

    /// <summary>
    /// The callback for the +10 BPM button.
    /// </summary>
    public void OnButton_BPMPlusTen()
    {
        this.OffsetBPMFromButton(10.0f);

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the +1 BPM button.
    /// </summary>
    public void OnButton_BPMPlusOne()
    {
        this.OffsetBPMFromButton(1.0f);

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the -1 BPM button.
    /// </summary>
    public void OnButton_BPMMinusOne()
    {
        this.OffsetBPMFromButton(-1.0f);

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the -10 BPM button.
    /// </summary>
    public void OnButton_BPMMinusTen()
    { 
        this.OffsetBPMFromButton(-10.0f);

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// Set the BMP to an offset of its current value.
    /// 
    /// This includes updating the UI and claping the final value.
    /// </summary>
    /// <param name="amt">The amount to offset by.</param>
    void OffsetBPMFromButton(float amt)
    {
        this.app.SetBPM(this.app.BeatsPerMinute() + amt, true);
        this.bpmInput.text = this.app.GetBPMString();
        this.ValidateUpDownFromAppBPM();
    }

    /// <summary>
    /// The BPM tab button.
    /// </summary>
    public void OnButton_Tap()
    { 
        bool tapMod = this.app.PushTapMeter(true);
        if(tapMod == true)
            this.SetBPMText(this.app.GetBPMString(), false);

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// Set the BPM text value
    /// </summary>
    /// <param name="str">The text value to set.</param>
    /// <param name="evaluateBPM">
    /// If true, try to evaluate the string to a number value and 
    /// set the BPM to that evaluated value.</param>
    public void SetBPMText(string str, bool evaluateBPM)
    {
        this.bpmInput.text = str;

        this.ValidateButtonsFromText(str);

        if (evaluateBPM == true)
        { 
            float f;
            if(float.TryParse(this.bpmInput.text, out f) == true)
            {
                this.app.SetBPM(f);
            }
        }
        this.ValidateUpDownFromAppBPM();
    }

    /// <summary>
    /// Check which BPM addition and subtraction buttons should
    /// be enabled.
    /// </summary>
    void ValidateUpDownFromAppBPM()
    {
        float bmp = this.app.BeatsPerMinute();
        bool showMin = bmp > Application.minBMP;
        bool showPls = bmp < Application.maxBMP;

        this.plusOneButton.interactable = showPls;
        this.plusTenButton.interactable = showPls;
        this.minusOneButton.interactable = showMin;
        this.minusTenButton.interactable = showMin;
    }

    /// <summary>
    /// Based off the current input text value, decide which 
    /// input text values should be accessible.
    /// </summary>
    /// <param name="str">The input BPM string.</param>
    public void ValidateButtonsFromText(string str)
    {
        int dotIdx = str.IndexOf('.');
        this.dotButton.interactable = dotIdx == -1;

        bool showNums = true;
        if (dotIdx == -1)
        {
            if (str.Length >= 3)
                showNums = false;
        }
        else
        {
            if (str.Length - dotIdx > 3)
                showNums = false;
        }

        foreach (UnityEngine.UI.Button numBtn in this.numberButtons)
        {
            numBtn.interactable = showNums;
        }

        this.backButton.interactable = str.Length > 0;
    }

    /// <summary>
    /// The callback for the back button.
    /// </summary>
    public void OnButton_DialBack()
    {
        string bpmTxt = this.bpmInput.text;
        if(bpmTxt.Length == 0.0f)
            return;

        bpmTxt = bpmTxt.Substring(0, bpmTxt.Length - 1);
        this.SetBPMText(bpmTxt, true);
        this.ValidateUpDownFromAppBPM();

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the metronome button.
    /// </summary>
    public void OnButton_DialMetronome()
    {
        bool metTogState = this.app.ToggleMetronome();

        this.app.SetMetronomeButtonImage(this.metronomeButtonImg, metTogState);

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// Add text to the input BPM string.
    /// </summary>
    /// <param name="str">The string to append.</param>
    public void AppendBPMText(string str)
    {
        string bpmTxt = this.bpmInput.text;
        bpmTxt += str;

        this.SetBPMText(bpmTxt, true);
        this.ValidateUpDownFromAppBPM();
    }

    /// <summary>
    /// The callback for the button that appends "0" to the BPM string.
    /// </summary>
    public void OnButton_Dial0()
    {
        this.AppendBPMText("0");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "1" to the BPM string.
    /// </summary>
    public void OnButton_Dial1()
    {
        this.AppendBPMText("1");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "2" to the BPM string.
    /// </summary>
    public void OnButton_Dial2()
    {
        this.AppendBPMText("2");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "3" to the BPM string.
    /// </summary>
    public void OnButton_Dial3()
    {
        this.AppendBPMText("3");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "4" to the BPM string.
    /// </summary>
    public void OnButton_Dial4()
    {
        this.AppendBPMText("4");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "5" to the BPM string.
    /// </summary>
    public void OnButton_Dial5()
    {
        this.AppendBPMText("5");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "6" to the BPM string.
    /// </summary>
    public void OnButton_Dial6()
    {
        this.AppendBPMText("6");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "7" to the BPM string.
    /// </summary>
    public void OnButton_Dial7()
    {
        this.AppendBPMText("7");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "8" to the BPM string.
    /// </summary>
    public void OnButton_Dial8()
    {
        this.AppendBPMText("8");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "9" to the BPM string.
    /// </summary>
    public void OnButton_Dial9()
    {
        this.AppendBPMText("9");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// The callback for the button that appends "." to the BPM string.
    /// </summary>
    public void OnButton_DialDot()
    {
        this.AppendBPMText(".");

        this.app.DoVibrateButton();
    }

    /// <summary>
    /// Restores the cached BPM value.
    /// </summary>
    public void RestoreOriginalBPM()
    { 
        this.app.SetBPM(this.originalBPM);
        this.ValidateUpDownFromAppBPM();
    }

    /// <summary>
    /// Semaphore value for EditBPM to check if the callback is 
    /// called through recursion - which we want to exit from to
    /// avoid a stack-overflow.
    /// </summary>
    bool overflowGuard = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    public void OnInput_EditBPM(string str)
    {
        string newStr = str;

        int origCaret = this.bpmInput.caretPosition;
        if (this.overflowGuard == false)
        {
            this.overflowGuard = true;

            
            int indexDot = newStr.IndexOf('.');
            if(indexDot != -1)
            {
                if(indexDot > 3)
                {
                    newStr = newStr.Remove(indexDot, 1);
                    newStr = newStr.Insert(3, ".");

                    if(origCaret == 4)
                        ++origCaret;
                }
            }
            else
            { 
                if(newStr.Length > 4)
                    newStr = newStr.Insert(3, ".");
            }
            

            if(newStr.Length > 7)
                newStr = newStr.Substring(0, 7);

            if(str != newStr)
            { 
                this.bpmInput.text = newStr;
            }

            this.SetBPMText(newStr, true);
            this.ValidateUpDownFromAppBPM();
            overflowGuard = false;
        }
        this.bpmInput.caretPosition = origCaret;
    }

    /// <summary>
    /// Callback for the when BPM input field has ended editing.
    /// </summary>
    /// <param name="str">The final string.</param>
    public void OnInput_EndEditBPM(string str)
    {
        this.OnInput_EditBPM(str);
        this.SetBPMText(this.app.GetBPMString(), true);
        this.ValidateUpDownFromAppBPM();
    }

    /// <summary>
    /// Callback for when the BPM input is receiving a new character.
    /// </summary>
    /// <param name="text">The previous text.</param>
    /// <param name="charIndex">The character index being modified.</param>
    /// <param name="addedChar">The character being appended.</param>
    /// <returns></returns>
    public char OnValidateInput(string text, int charIndex, char addedChar)
    {
        if(addedChar != '.' && char.IsNumber(addedChar) == false)
            return '\0';

        if (addedChar == '.')
        { 
            if(text.Contains(".") == true)
                return '\0';

            return addedChar;
        }

        if(this.bpmInput.text.Length >= 7 && charIndex == text.Length - 1)
        {
            this.bpmInput.text = this.bpmInput.text.Substring(0, this.bpmInput.text.Length - 1);
            return addedChar;
        }

        int idx = text.IndexOf('.');
        if(idx != -1)
        { 
            if(charIndex > idx && text.Length - idx > 3 )
            { 
                int origCP = this.bpmInput.caretPosition;
                string newStr = this.bpmInput.text.Insert(charIndex, addedChar.ToString());
                newStr = newStr.Substring(0, idx + 4);
                this.bpmInput.text = newStr;
                this.bpmInput.caretPosition = origCP + 1;
                return '\0';
            }
        }

        return addedChar;
    }

    public void OnButton_PulldownBeat()
    { 
        this.app.DoVibrateButton();

        PxPre.DropMenu.StackUtil stackUtil = new PxPre.DropMenu.StackUtil("Beat");

        PulldownInfo.DoChevyDown(this.tweenUtil, this.pulldownBeat.pulldownChevy);

        for(int i = 0; i < this.app.metronomePatterns.Length; ++i)
        { 
            int iCpy = i;

            stackUtil.AddAction(
                iCpy == this.app.metronomeIdx,
                null,
                this.app.metronomePatterns[iCpy].name,
                ()=>
                { 
                    if(iCpy == this.app.metronomeIdx)
                        return;

                    this.app.SetMetronomeID(iCpy);

                    this.pulldownBeat.text.text = 
                        this.app.metronomePatterns[this.app.metronomeIdx].name;

                    Application.DoDropdownTextUpdate( 
                        this.tweenUtil,
                        this.pulldownBeat.text);
                });
        }

        PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
            CanvasSingleton.canvas,
            stackUtil.Root,
            this.pulldownBeat.rootPlate);
    }

    public void OnSlider_BPMVolume()
    { 
        this.app.SetMetronomeVolume(
            this.exerciseVolumeSlider.value);
    }
}
