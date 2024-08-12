using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that contains and manages keyboard objects and assets.
/// </summary>
public class KeyCollection
{
    /// <summary>
    /// The lowest octave that can be represented.
    /// </summary>
    public const int minOctave = 1;

    /// <summary>
    /// The highest octave that can be represented.
    /// </summary>
    public const int maxOctave = 8;

    public enum Accidental
    { 
        Sharp,
        Flat
    }

    public enum OctaveHighlighting
    { 
        ROYGBIVM,
        Black
    }

    [System.Serializable]
    public struct KeyColor
    { 
        public Color up;
        public Color down;
    }

    [System.Serializable]
    public struct HighlightColorInfo
    { 
        public KeyColor normal;
        public KeyColor scale;
        public KeyColor root;
    }

    public static PxPre.Phonics.WesternFreqUtils.Key [] keyboardKeyOrder = 
        {
            PxPre.Phonics.WesternFreqUtils.Key.C,
            PxPre.Phonics.WesternFreqUtils.Key.Cs,
            PxPre.Phonics.WesternFreqUtils.Key.D,
            PxPre.Phonics.WesternFreqUtils.Key.Ds,
            PxPre.Phonics.WesternFreqUtils.Key.E,
            PxPre.Phonics.WesternFreqUtils.Key.F,
            PxPre.Phonics.WesternFreqUtils.Key.Fs,
            PxPre.Phonics.WesternFreqUtils.Key.G,
            PxPre.Phonics.WesternFreqUtils.Key.Gs,
            PxPre.Phonics.WesternFreqUtils.Key.A,
            PxPre.Phonics.WesternFreqUtils.Key.As,
            PxPre.Phonics.WesternFreqUtils.Key.B
        };

    public static readonly Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfoSharp =
            new Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr>
            {
                [PxPre.Phonics.WesternFreqUtils.Key.A] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.A, "A", false, 5),
                [PxPre.Phonics.WesternFreqUtils.Key.B] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.B, "B", false, 6),
                [PxPre.Phonics.WesternFreqUtils.Key.C] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.C, "C", false, 0),
                [PxPre.Phonics.WesternFreqUtils.Key.D] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.D, "D", false, 1),
                [PxPre.Phonics.WesternFreqUtils.Key.E] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.E, "E", false, 2),
                [PxPre.Phonics.WesternFreqUtils.Key.F] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.F, "F", false, 3),
                [PxPre.Phonics.WesternFreqUtils.Key.G] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.G, "G", false, 4),

                [PxPre.Phonics.WesternFreqUtils.Key.As] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.As, "A♯", true, 6),
                [PxPre.Phonics.WesternFreqUtils.Key.Cs] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Cs, "C♯", true, 1),
                [PxPre.Phonics.WesternFreqUtils.Key.Ds] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Ds, "D♯", true, 2),
                [PxPre.Phonics.WesternFreqUtils.Key.Fs] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Fs, "F♯", true, 4),
                [PxPre.Phonics.WesternFreqUtils.Key.Gs] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Gs, "G♯", true, 5)
            };

    public static readonly Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfoFlats =
            new Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr>
            {
                [PxPre.Phonics.WesternFreqUtils.Key.A] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.A, "A", false, 5),
                [PxPre.Phonics.WesternFreqUtils.Key.B] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.B, "B", false, 6),
                [PxPre.Phonics.WesternFreqUtils.Key.C] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.C, "C", false, 0),
                [PxPre.Phonics.WesternFreqUtils.Key.D] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.D, "D", false, 1),
                [PxPre.Phonics.WesternFreqUtils.Key.E] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.E, "E", false, 2),
                [PxPre.Phonics.WesternFreqUtils.Key.F] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.F, "F", false, 3),
                [PxPre.Phonics.WesternFreqUtils.Key.G] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.G, "G", false, 4),

                [PxPre.Phonics.WesternFreqUtils.Key.As] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.As, "B♭", true, 6),
                [PxPre.Phonics.WesternFreqUtils.Key.Cs] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Cs, "D♭", true, 1),
                [PxPre.Phonics.WesternFreqUtils.Key.Ds] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Ds, "E♭", true, 2),
                [PxPre.Phonics.WesternFreqUtils.Key.Fs] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Fs, "G♭", true, 4),
                [PxPre.Phonics.WesternFreqUtils.Key.Gs] = new CreateNoteDescr(0, PxPre.Phonics.WesternFreqUtils.Key.Gs, "A♭", true, 5)
            };

    public KeyAssets keyAssets;
    public KeyDimParams keyDims;

    public List<Key> keys = new List<Key>();
    //
    public Dictionary<KeyPair, Key> keyLookup =
        new Dictionary<KeyPair, Key>();

    public float keyboardWidth = 0.0f;

    public HashSet<int> highlights = null;
    public int baseHighlightKey = 0;

    public RectTransform rtHost;
    public RectTransform rtViewport;

    public KeyCollection.HighlightColorInfo whiteKeyColors;
    public KeyCollection.HighlightColorInfo blackKeyColors;

    public Accidental accidental = Accidental.Sharp;
    public OctaveHighlighting octaveBackground = OctaveHighlighting.Black;

    public KeyCollection(
        KeyAssets keyAssets, 
        KeyDimParams keyDims, 
        RectTransform rtHost, 
        RectTransform rtViewport,
        KeyCollection.HighlightColorInfo whiteKeyColors,
        KeyCollection.HighlightColorInfo blackKeyColors)
    {
        this.keyAssets = keyAssets;
        this.keyDims = keyDims;

        this.rtHost = rtHost;
        this.rtViewport = rtViewport;

        this.whiteKeyColors = whiteKeyColors;
        this.blackKeyColors = blackKeyColors;

        this.baseHighlightKey =
            PxPre.Phonics.WesternFreqUtils.GetNote(PxPre.Phonics.WesternFreqUtils.Key.C, 1) % 12;
    }

    public void Clear()
    {
        this.keyLookup.Clear();
        this.keys.Clear();

        this.keyboardWidth = 0.0f;
    }

    public void DestroyAllAndClear()
    {
        foreach (Key k in this.keys)
            GameObject.Destroy(k.gameObject);

        this.Clear();
    }

    public bool TryGetValue(KeyPair kp, out Key k)
    {
        return this.keyLookup.TryGetValue(kp, out k);
    }

    public void AddKey(Key k)
    {
        this.keyLookup.Add(new KeyPair(k.key, k.octave), k);
        this.keys.Add(k);
    }

    public void DisableLabels()
    {
        foreach (Key k in this.keys)
            k.label.gameObject.SetActive(false);
    }

    public void HighlightKeysWithOffset(PxPre.Phonics.WesternFreqUtils.Key key, params int[] offsets)
    {
        this.baseHighlightKey = PxPre.Phonics.WesternFreqUtils.GetNote(key, 1) % 12;
        this.RehighlightKeysWithOffset(offsets);
    }

    public void HighlightKeys(PxPre.Phonics.WesternFreqUtils.Key key)
    {
        this.baseHighlightKey = PxPre.Phonics.WesternFreqUtils.GetNote(key, 1) % 12;
        this.RehighlightKeys();
    }

    public void RehighlightKeysWithOffset(params int[] offsets)
    {
        if(offsets == null || offsets.Length == 0)
            this.highlights = null;
        else
            this.highlights = new HashSet<int>(offsets);

        this.RehighlightKeys();
    }

    static void ApplyColorsToKey(KeyColor kc, Key k)
    {
        k.plate.color = kc.up;

        UnityEngine.UI.ColorBlock cb = k.colors;
        cb.pressedColor = kc.down;
        cb.highlightedColor = kc.up;
        cb.normalColor = kc.up;
        k.colors = cb;
    }

    public void RehighlightKeys()
    {
        foreach (Key k in this.keys)
        {
            int idk = PxPre.Phonics.WesternFreqUtils.GetNote(k.key, k.octave);
            idk = (((idk - this.baseHighlightKey) % 12) + 12) % 12;

            KeyColor kcFin = new KeyColor();
            bool canChangeOpacity = true;

            if (this.highlights == null || this.highlights.Contains(idk) == false)
            {
                if (k.keyShadow == null)
                    kcFin = this.whiteKeyColors.normal;
                    
                else
                    kcFin = this.blackKeyColors.normal;

            }
            else if (idk == 0)
            {
                if(k.keyShadow == null)
                    kcFin = this.whiteKeyColors.root;
                else
                    kcFin = this.blackKeyColors.root;

                canChangeOpacity = false;
            }
            else
            {
                if (k.keyShadow == null)
                    kcFin = this.whiteKeyColors.scale;
                else
                    kcFin = this.blackKeyColors.scale;

                canChangeOpacity = false;
            }

            const float octaveTransForBleedthrough = 0.95f;
            //
            if( canChangeOpacity == true && 
                this.octaveBackground == OctaveHighlighting.ROYGBIVM)
            {
                kcFin.up.a = octaveTransForBleedthrough;
                kcFin.down.a = octaveTransForBleedthrough;

            }

            ApplyColorsToKey(kcFin, k);

        }
    }

    public void UnhighlightKeys()
    {
        this.RehighlightKeysWithOffset(null);
    }

    public void SetKeyLabels(bool showKeys, bool showOctaves, bool whites, bool blacks)
    {
        if (
            (showKeys == false && showOctaves == false) ||
            (whites == false && blacks == false))
        {
            foreach (Key k in this.keys)
                k.label.gameObject.SetActive(false);
        }
        else
        {
            HashSet<PxPre.Phonics.WesternFreqUtils.Key> whiteKeys =
                new HashSet<PxPre.Phonics.WesternFreqUtils.Key>()
                {
                        PxPre.Phonics.WesternFreqUtils.Key.A,
                        PxPre.Phonics.WesternFreqUtils.Key.B,
                        PxPre.Phonics.WesternFreqUtils.Key.C,
                        PxPre.Phonics.WesternFreqUtils.Key.D,
                        PxPre.Phonics.WesternFreqUtils.Key.E,
                        PxPre.Phonics.WesternFreqUtils.Key.F,
                        PxPre.Phonics.WesternFreqUtils.Key.G
                };

            foreach (Key k in this.keys)
            {
                bool isWhite = whiteKeys.Contains(k.key);
                bool isBlack = !isWhite;

                if ((isWhite == true && whites == false) ||
                    (isBlack == true && blacks == false))
                {
                    k.label.gameObject.SetActive(false);
                    continue;
                }
                k.label.gameObject.SetActive(true);
                k.SetLabel(showKeys, showOctaves, this.accidental);
            }
        }
    }

    Key CreateKeyAsset(int octave, CreateNoteDescr cnd, PaneKeyboard keypane)
    {
        Key keyBtn = null;

        if (cnd.black == false)
        {
            // White key
            int o = octave + cnd.octaveOffset;
            GameObject goWK = new GameObject(cnd.noteName + o.ToString());
            goWK.transform.SetParent(this.rtHost);
            goWK.transform.rotation = Quaternion.identity;
            goWK.transform.localScale = Vector3.one;

            keyBtn = goWK.AddComponent<Key>();
            keyBtn.InitializeKey(
                keypane, 
                cnd.key, 
                o, 
                this.keyAssets.spriteWhiteKeyUp,
                this.keyAssets.spriteWhiteKeyDown,
                this.keyAssets.spriteWhiteDisabled,
                this.keyAssets.whiteKeyColors.normal.up, 
                this.keyAssets.whiteKeyColors.normal.down);

            RectTransform rtKey = keyBtn.rectTransform;
            rtKey.pivot = new Vector2(0.0f, 1.0f);
            rtKey.anchorMin = new Vector2(0.0f, 0.0f);
            rtKey.anchorMax = new Vector2(0.0f, 1.0f);
            rtKey.offsetMin = new Vector2(0, 2.0f);
            rtKey.offsetMax = new Vector2(this.keyDims.whiteKeyWidth, 0.0f);

            // A little hack to make sure the white keys are below the blacks, even the
            // white keys of the next octave.
            rtKey.SetAsFirstSibling();
        }
        else
        {
            // Black key
            int o = octave + cnd.octaveOffset;

            // Black Key crevasse Creation
            //////////////////////////////////////////////////
            GameObject goPlate = new GameObject("BlackKeyCrev");
            goPlate.transform.SetParent(this.rtHost);
            goPlate.transform.localRotation = Quaternion.identity;
            goPlate.transform.localScale = Vector3.one;

            UnityEngine.UI.Image imgBKC = goPlate.AddComponent<UnityEngine.UI.Image>();
            imgBKC.sprite = this.keyAssets.spriteBlackCrev;
            imgBKC.type = UnityEngine.UI.Image.Type.Sliced;

            RectTransform rtBKC = imgBKC.rectTransform;
            rtBKC.pivot = new Vector2(0.0f, 1.0f);
            rtBKC.anchorMin = new Vector2(0.0f, 0.0f);
            rtBKC.anchorMax = new Vector2(0.0f, 1.0f);
            rtBKC.offsetMin = new Vector2(0.0f, 0.0f);
            rtBKC.offsetMax = new Vector2(this.keyDims.blackKeyWidth, 0.0f);

            // Black Key Creation
            //////////////////////////////////////////////////
            GameObject goWk = new GameObject(cnd.noteName + o.ToString());
            goWk.transform.SetParent(rtHost);
            goWk.transform.localRotation = Quaternion.identity;
            goWk.transform.localScale = Vector3.one;

            keyBtn = goWk.AddComponent<Key>();
            keyBtn.InitializeKey(
                keypane, 
                cnd.key, 
                o, 
                this.keyAssets.spriteBlackKeyUp, 
                this.keyAssets.spriteBlackKeyDown, 
                this.keyAssets.spriteBlackDisabled,
                this.keyAssets.blackKeyColors.normal.up,
                this.keyAssets.blackKeyColors.normal.down);

            keyBtn.keyShadow = imgBKC;

            RectTransform rtKey = keyBtn.rectTransform;
            rtKey.pivot = new Vector2(0.0f, 1.0f);
            rtKey.anchorMin = new Vector2(0.0f, 0.0f);
            rtKey.anchorMax = new Vector2(0.0f, 1.0f);
            rtKey.offsetMin = new Vector2(0.0f, 0.0f);
            rtKey.offsetMax = new Vector2(this.keyDims.blackKeyWidth, 0.0f);
        }

        if(keyBtn != null)
            this.AddKey(keyBtn);

        return keyBtn;
    }

    public static Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> GetKeyCreationsInfo(Accidental acc)
    { 
        if(acc == Accidental.Flat)
            return keyCreationsInfoFlats;

        return keyCreationsInfoSharp;
    }

    public Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> GetKeyCreationsInfo()
    { 
        return GetKeyCreationsInfo(this.accidental);
    }

    public float AlignKeys()
    { 
        float maxX = float.NegativeInfinity;
        float minX = float.PositiveInfinity;

        float blackKeyHeight =
            CalculateBlackKeyHeight();

        Dictionary < PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr > keyCreationsInfo = 
            this.GetKeyCreationsInfo();

        // Do the alignment without worring where the origin is.
        float octaveLen = (this.keyDims.whiteKeyWidth + this.keyDims.whiteKeyPadding) * 7;
        foreach(Key k in this.keys)
        { 
            float off = k.octave * octaveLen;
            float minHeight = 0.0f;
            float width;
            float botOffset = 0.0f;

            CreateNoteDescr cnd = keyCreationsInfo[k.key];
            if(cnd.black == true)
            {
                off += 
                    (this.keyDims.whiteKeyWidth + this.keyDims.whiteKeyPadding)  * cnd.offset
                    -(this.keyDims.whiteKeyPadding + this.keyDims.blackKeyWidth) * 0.5f;

                minHeight = blackKeyHeight;
                width = this.keyDims.blackKeyWidth;
            }
            else
            {
                off += (this.keyDims.whiteKeyWidth + this.keyDims.whiteKeyPadding) * cnd.offset;
                width = this.keyDims.whiteKeyWidth;

                botOffset = 2.0f;
            }

            k.transform.localRotation = Quaternion.identity;
            k.transform.localPosition = Vector3.zero;
            k.transform.localScale = Vector3.one;

            k.rectTransform.offsetMin =
                new Vector2(
                    off,
                    minHeight + botOffset);

            k.rectTransform.offsetMax =
                new Vector2(
                    off + width,
                    0.0f);

            if (k.keyShadow != null)
            {
                RectTransform rtShad = k.keyShadow.rectTransform;

                rtShad.localPosition = Vector3.zero;
                rtShad.localRotation = Quaternion.identity;
                rtShad.localScale = Vector3.one;

                rtShad.offsetMin = 
                    new Vector2(
                        off - this.keyAssets.blackKeyCrevH, 
                        minHeight - this.keyAssets.blackKeyCrevV);

                rtShad.offsetMax =
                    new Vector2(
                        off + width + this.keyAssets.blackKeyCrevH,
                        0.0f);
            }

            maxX = Mathf.Max(maxX, off + width);
            minX = Mathf.Min(minX, off);
        }

        // Move the entire set to the origin
        foreach (Key k in this.keys)
        { 
            RectTransform rtk = k.rectTransform;

            rtk.offsetMin = new Vector2(rtk.offsetMin.x - minX, rtk.offsetMin.y);
            rtk.offsetMax = new Vector2(rtk.offsetMax.x - minX, rtk.offsetMax.y);

            if (k.keyShadow != null)
            {
                RectTransform rts = k.keyShadow.rectTransform;
                rts.offsetMin = new Vector2(rts.offsetMin.x - minX, rts.offsetMin.y);
                rts.offsetMax = new Vector2(rts.offsetMax.x - minX, rts.offsetMax.y);

                this.SetShadowColor(k);
            }
        }

        if (maxX == float.NegativeInfinity)
            return 0.0f;

        return maxX - minX;
    }

    public void RecolorBackground()
    { 
        foreach(Key k in this.keys)
        { 
            if(k.keyShadow == null)
                continue;

            this.SetShadowColor(k);
        }
    }

    public void SetShadowColor(Key k)
    { 
        if(k.keyShadow == null)
            return;

        if(this.octaveBackground == OctaveHighlighting.Black)
            k.keyShadow.color = Color.black;
        else
            k.keyShadow.color = GetOctaveColor(k.octave);
    }

    public static Color GetOctaveColor(int octave)
    { 
        switch(octave)
        { 
            case 1:
                return new Color(0.94f, 0.27f, 0.27f);

            case 2:
                return new Color(1.0f, 0.78f, 0.133f );

            case 3:
                return new Color(0.91f, 0.941f, 0.0f );

            case 4:
                return new Color(0.22f, 0.96f, 0.25f);

            case 5:
                return new Color(0.0f, 0.5f, 1.0f);

            case 6:
                return new Color(0.76f, 0.45f, 0.96f);

            case 7:
                return new Color(0.89f, 0.37f, 0.88f);

            case 8:
                return new Color(1.0f, 0.0f, 0.5f);
        }

        return Color.black;
    }

    void AlignBlacks(float newPercent)
    { 
        this.keyDims.blackPaddPercent = newPercent;
        this.AlignBlacks();
    }

    static float CalculateBlackKeyHeight(RectTransform viewport, float percent, float botPx, float topPx)
    {
        float keyVPHeight = viewport.rect.height;
        return
            Mathf.Lerp(
                botPx,
                keyVPHeight - topPx,
                percent);
    }

    public float CalculateBlackKeyHeight()
    {
        return
            CalculateBlackKeyHeight(
                this.rtViewport,
                this.keyDims.blackPaddPercent,
                this.keyDims.blackMinTop,
                this.keyDims.blackMinBot);
    }


    public void AlignBlacks()
    {
        float keyHeight = 
            CalculateBlackKeyHeight();

        foreach (Key k in this.keys)
        {
            switch (k.key)
            {
                case PxPre.Phonics.WesternFreqUtils.Key.A:
                case PxPre.Phonics.WesternFreqUtils.Key.B:
                case PxPre.Phonics.WesternFreqUtils.Key.C:
                case PxPre.Phonics.WesternFreqUtils.Key.D:
                case PxPre.Phonics.WesternFreqUtils.Key.E:
                case PxPre.Phonics.WesternFreqUtils.Key.F:
                case PxPre.Phonics.WesternFreqUtils.Key.G:
                    continue;
            }

            k.rectTransform.offsetMin =
                new Vector2(
                    k.rectTransform.offsetMin.x,
                    keyHeight);

            if (k.keyShadow != null)
            {
                RectTransform rtShad = k.keyShadow.rectTransform;
                rtShad.offsetMin =
                    new Vector2(
                        rtShad.offsetMin.x,
                        keyHeight - this.keyAssets.blackKeyCrevV);
            }
        }
    }

    public float CreateKeyboardRange(int lowerOctave, int higherOctave, PaneKeyboard keyboardPane, bool align = true)
    {
        lowerOctave = Mathf.Max(minOctave, lowerOctave);

        higherOctave = Mathf.Min(higherOctave, maxOctave);
        higherOctave = Mathf.Max(higherOctave, lowerOctave);

        Dictionary<KeyPair, Key> existing = 
            new System.Collections.Generic.Dictionary<KeyPair, Key>(this.keyLookup);

        this.keys.Clear();
        this.keyLookup.Clear();

        Dictionary<PxPre.Phonics.WesternFreqUtils.Key, CreateNoteDescr> keyCreationsInfo =
            this.GetKeyCreationsInfo();

        for (int i = lowerOctave; i <= higherOctave; ++i)
        { 
            foreach(PxPre.Phonics.WesternFreqUtils.Key k in keyboardKeyOrder)
            { 
                KeyPair kp = new KeyPair(k, i);
                Key keyCreated;
                if(existing.TryGetValue(kp, out keyCreated) == true)
                { 
                    existing.Remove(kp);

                    this.AddKey(keyCreated);
                }
                else
                {
                    // CreateKeyAsset automatically adds to this.keys and tyhis.keyLookup
                    keyCreated = 
                        CreateKeyAsset( i, keyCreationsInfo[kp.key], keyboardPane);
                }

            }
        }

        foreach(KeyValuePair<KeyPair, Key> kvp in existing)
        { 
            Key kNotUsed = kvp.Value;

            if(kNotUsed.keyShadow != null)
                GameObject.Destroy(kNotUsed.keyShadow.gameObject);

            GameObject.Destroy(kNotUsed.gameObject);
        }

        if (align == true)
            return this.AlignKeys();

        return 0.0f;
    }

    public void ResetKeyPresses()
    { 
        foreach(Key k in this.keys)
            k.ResetTransition();
    }

    public bool SetKeyLabel(int octave, PxPre.Phonics.WesternFreqUtils.Key key, bool labelKey, bool labelOctave)
    {
        Key k;
        if(this.keyLookup.TryGetValue(new KeyPair(key, octave), out k) == false)
            return false;

        k.SetLabel(labelKey, labelOctave, this.accidental);
        return true;
    }

    public bool SetKeyLabel(KeyPair note, bool labelKey, bool labelOctave)
    {
        Key k;
        if (keyLookup.TryGetValue(note, out k) == false)
            return false;

        k.SetLabel(labelKey, labelOctave, this.accidental);
        return true;
    }

    public void EnableAllKeys(bool val = true)
    { 
        foreach(Key k in this.keyLookup.Values)
            k.interactable = val;
    }

    public void DisableAllKeys()
    {
        this.EnableAllKeys(false);
    }
}