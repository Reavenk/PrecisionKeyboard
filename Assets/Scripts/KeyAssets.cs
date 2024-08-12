using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyAssets
{
    public Sprite spriteWhiteKeyUp;
    public Sprite spriteWhiteKeyDown;
    public Sprite spriteWhiteDisabled;
    public Sprite spriteBlackKeyUp;
    public Sprite spriteBlackKeyDown;
    public Sprite spriteBlackDisabled;

    public float blackKeyCrevH = 3.0f;
    public float blackKeyCrevV = 3.0f;

    public Sprite spriteBlackCrev;

    public KeyCollection.HighlightColorInfo blackKeyColors;
    public KeyCollection.HighlightColorInfo whiteKeyColors;

    public Font keyLabelFont;
    public Color keyLabelColor;
    public int labelFontSize = 20;
}
