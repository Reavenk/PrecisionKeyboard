using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Application : MonoBehaviour
{
    int vibrateButtonMS = 5;
    int vibrateKeysMS = 10;

    public const int MaxHapticAmount = 40;

    public void DoVibrateKeyboard()
    {
        if(this.vibrateKeysMS == 0)
            return;

        Vibrator.Vibrate(this.vibrateKeysMS);
    }

    public void DoVibrateButton()
    {
        if(this.vibrateButtonMS == 0)
            return;

        Vibrator.Vibrate(this.vibrateButtonMS);
    }

    public void DoVibrateBlipCancel()
    {
        if(this.vibrateButtonMS == 0) 
            return;

        Vibrator.Vibrate(1);
    }

    public void DoVibrateSlider()
    {
        this.DoVibrateButton();
    }

    public int GetVibrateKeysMS()
    {
        return this.vibrateKeysMS;
    }

    public int GetVibrateButtonsMS()
    {
        return this.vibrateButtonMS;
    }

    public bool SetVibrateKeysMS(int newVal)
    {
        newVal = Mathf.Clamp(newVal, 0, MaxHapticAmount + 1);

        if (newVal == this.vibrateKeysMS)
            return false;

        this.vibrateKeysMS = newVal;
        this.optionsPane.SetHapticSliderKeys(newVal);
        return true;
    }

    public bool SetVibrateButtonsMS(int newVal)
    {
        newVal = Mathf.Clamp(newVal, 0, MaxHapticAmount + 1);

        if (newVal == this.vibrateButtonMS)
            return false;

        this.vibrateButtonMS = newVal;
        this.optionsPane.SetHapticSliderButtons(newVal);
        return true;
    }
}
