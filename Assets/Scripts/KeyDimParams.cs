using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyDimParams
{
    public float whiteKeyWidth = 50.0f;
    public float blackKeyWidth = 40.0f;
    public float whiteKeyPadding = 2.0f;
    public float blackMinTop = 70.0f;
    public float blackMinBot = 70.0f;
    public float blackPaddPercent = 0.35f;
    
    public float labelUpOffset = 50.0f;

    public KeyDimParams Clone()
    { 
        KeyDimParams kdp        = new KeyDimParams();

        kdp.whiteKeyPadding     = this.whiteKeyPadding;
        kdp.blackKeyWidth       = this.blackKeyWidth;
        kdp.whiteKeyPadding     = this.whiteKeyPadding;
        kdp.blackMinTop         = this.blackMinTop;
        kdp.blackMinBot         = this.blackMinBot;
        kdp.blackPaddPercent    = this.blackPaddPercent;

        return kdp;
    }


}
