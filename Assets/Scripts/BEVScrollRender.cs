using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public class BEVScrollRender : UnityEngine.EventSystems.UIBehaviour
{
    public Application app;

    public Shader unlitRTTShader;

    protected RenderTexture rtKeys = null;
    protected RenderTexture rtOlys = null;
    public UnityEngine.UI.RawImage imgDst = null;

    bool bevOctaveDirty = false;
    bool bevHighlightsDirty = false;

    protected Material renderMat = null;

    Coroutine dirtyCoroutine = null;
    public UnityEngine.UI.RawImage imgThumb;
    public UnityEngine.UI.MyScrollbar scrollbar;

    // A specific material with a specific shader is used for the BEV. It's referenced
    // so we can change it as needed to update it properly.
    public Material bevScrollbarMaterial;

    public struct KeyOffsetInfo
    { 
        public float normX;
        public bool black;

        public KeyOffsetInfo(float normX, bool black)
        { 
            this.normX = normX;
            this.black = black;
        }
    }

    protected override void Awake()
    {
        this.renderMat = new Material(Shader.Find("Hidden/Internal-Colored"));

        this.imgDst = this.gameObject.GetComponent<UnityEngine.UI.RawImage>();
    }

    protected override void Start()
    {
        base.Start();

        // Do this in start instead of awake or else we might need to do 
        // this after some other initialization.
        this.SetDirty(true, true);
    }

    public void SetDirty(bool octavesize, bool highlights)
    { 
        if(octavesize == false && highlights == false)
            return;

        if(octavesize == true)
        {
            this.bevOctaveDirty = true;
            this.bevHighlightsDirty = true;
        }
        else
            this.bevHighlightsDirty = true;

        if (highlights == true)


        if(this.dirtyCoroutine != null)
            return;

        if(this.app != null)    // This will be triggered and will fail during app shutdown
        {
            this.dirtyCoroutine = 
                this.app.StartCoroutine(this.DirtyCoroutine());
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        this.SetDirty(true, true);
    }

    IEnumerator DirtyCoroutine()
    {
        yield return new WaitForFixedUpdate();

        int octaves = this.app.keyboardPane.upperOctave - this.app.keyboardPane.lowerOctave + 1;
        Rect r = this.imgDst.rectTransform.rect;

        if(this.bevOctaveDirty == true)
        {
            // bevOctaveDirty dirty flag cleared in called function
            this.RenderBEVOctave(
                (int)(r.width / octaves), 
                (int)r.height);

            this.imgDst.uvRect =
                new Rect(
                    (float)(this.app.keyboardPane.lowerOctave - 1),
                    0.0f,
                    octaves,
                    1.0f);
        }

        if(this.bevHighlightsDirty == true)
        {
            // bevHighlightsDirty dirty flag cleared in called function
            this.RenderBEVHighlights(
                (int)(r.width / octaves),
                (int)r.height,
                (int)this.app.keyboardPane.HighlightedRoot(),
                this.app.keyboardPane.HighlightedIntervals());

            // If we change the texture, it won't update it's materials to match.
            // There's most definitly some caching and dirty flagging system I
            // should probably dig up, but hopefully this does the job (at low
            // overhead).
            this.imgThumb.enabled = false;
            this.imgThumb.enabled = true;
        }


        this.dirtyCoroutine = null;
    }

    void RenderBEVHighlights(int width, int height, int offset, int[] chords)
    { 
        if(this.rtOlys != null)
            this.rtOlys.Release();

        // We don't scale this for extra quality, because it's just some
        // overlayed squares.
        this.rtOlys = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        this.rtOlys.wrapModeU = TextureWrapMode.Repeat;
        this.rtOlys.useMipMap = false;
        this.rtOlys.antiAliasing = 4;

        RenderTexture.active = this.rtOlys;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0.0f, (float)width, (float)height, 0.0f);

        this.renderMat.SetPass(0);
        GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        // These values copy-pasta'd from RenderBEVOctave()
        float fhgt = (float)height;
        float fwdt = (float)width;
        //
        float blackRatio = this.app.keyboardPane.keyDims.blackKeyWidth / this.app.keyboardPane.keyDims.whiteKeyWidth;
        float blackWidth = 1.0f / 7.0f * blackRatio * fwdt;
        //float halfBW = blackWidth * 0.5f;
        //float hlHeight = 8.0f;
        //float botHlHgt = 2.0f + hlHeight;

        // Remember that we only care about the highlight regions in the BEV
        GL.Begin(GL.TRIANGLES);

        // If we have chords being highlighted, also draw the highlighted 
        // colors on the BEV keys.
        if (chords != null && chords.Length > 0)
        {
            offset += 12 - (int)PxPre.Phonics.WesternFreqUtils.Key.C;

            // Define where the normalized horizontal position of all the keys are, and also
            // pair them with if they're a black or non-black key.
            KeyOffsetInfo[] rkoi =
                new KeyOffsetInfo[]
                { 
                    /*[00]*/ new KeyOffsetInfo(  1.0f / 14.0f, false),
                    /*[01]*/ new KeyOffsetInfo(  2.0f / 14.0f, true),
                    /*[02]*/ new KeyOffsetInfo(  3.0f / 14.0f, false),
                    /*[03]*/ new KeyOffsetInfo(  4.0f / 14.0f, true),
                    /*[04]*/ new KeyOffsetInfo(  5.0f / 14.0f, false),
                    /*[05]*/ new KeyOffsetInfo(  7.0f / 14.0f, false),
                    /*[06]*/ new KeyOffsetInfo(  8.0f / 14.0f, true),
                    /*[07]*/ new KeyOffsetInfo(  9.0f / 14.0f, false),
                    /*[08]*/ new KeyOffsetInfo( 10.0f / 14.0f, true),
                    /*[09]*/ new KeyOffsetInfo( 11.0f / 14.0f, false),
                    /*[10]*/ new KeyOffsetInfo( 12.0f / 14.0f, true),
                    /*[11]*/ new KeyOffsetInfo( 13.0f / 14.0f, false),
                };

            float halfHeight = fhgt * 0.5f;
            float wrad = fwdt / 14.0f;
            float sub = 0.0f;
            for (int i = 3; i >= 0; --i)
            {
                // Dynamic resizing. If the radius is too small, shrink back the padding
                sub = i * 2.0f;
                if (wrad - sub < 4.0f)
                    continue;

                break;
            }
            float brad = wrad * blackRatio;
            wrad -= sub;
            brad -= sub;

            for (int i = 0; i < chords.Length; ++i)
            {
                // This used to not be a full 1.0f, but since colored BEV highlighting
                // was added, the background color could leak-in and taint the highlight
                // color if it wasn't set to full opacity.
                const float highlightAlpha = 1.0f;

                if (i == 0)
                    GL.Color(new Color(1.0f, 0.5f, 0.5f, highlightAlpha));
                else if (i == 1)
                    GL.Color(new Color(0.5f, 1.0f, 0.5f, highlightAlpha));

                int idx = (offset + chords[i]) % rkoi.Length;

                float x = fwdt * rkoi[idx].normX;
                float y = fhgt - 10.0f;

                float rad = wrad;
                if (rkoi[idx].black == true)
                {
                    y -= halfHeight;
                    rad = brad;
                }

                GL.Vertex3(x + rad, y + 5.0f, 0.0f);
                GL.Vertex3(x - rad, y + 5.0f, 0.0f);
                GL.Vertex3(x - rad, y - 5.0f, 0.0f);
                //
                GL.Vertex3(x - rad, y - 5.0f, 0.0f);
                GL.Vertex3(x + rad, y - 5.0f, 0.0f);
                GL.Vertex3(x + rad, y + 5.0f, 0.0f);
            }
        }

        GL.End();
        GL.PopMatrix();

        RenderTexture.active = null;
        this.bevScrollbarMaterial.SetTexture("_HighlightOverlay", this.rtOlys);
        this.bevHighlightsDirty = false;
    }

    // When the octave level changes, we re-render it to make sure it fits near-pixelperfect
    // when wrapping in the BEV.
    void RenderBEVOctave(int width, int height)
    { 
        this.imgDst.texture = null;
        this.imgThumb.texture = null;

        if (this.rtKeys != null)
            this.rtKeys.Release();

        // We may want to render the actually image larger
        // than what we need so it looks better at smaller scales.
        const int resFactor = 2;

        this.rtKeys = new RenderTexture(width * resFactor, height * resFactor, 0, RenderTextureFormat.ARGB32);
        this.rtKeys.wrapModeU = TextureWrapMode.Repeat;
        this.rtKeys.useMipMap = false;
        this.rtKeys.antiAliasing = 4;

        RenderTexture.active = this.rtKeys;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0.0f, (float)width, (float)height, 0.0f);

        this.renderMat.SetPass(0);
        GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        float fhgt = (float)height;
        float fwdt = (float)width;

        float blackRatio = this.app.keyboardPane.keyDims.blackKeyWidth / this.app.keyboardPane.keyDims.whiteKeyWidth;
        float blackWidth = 1.0f / 7.0f * blackRatio * fwdt;
        float halfBW = blackWidth * 0.5f;
        float hlHeight = 8.0f;
        float botHlHgt = 2.0f + hlHeight;

        GL.Begin(GL.TRIANGLES);
            
            // The left bar of the octave
            GL.Color(Color.white);
            GL.Vertex3(0.0f, fhgt, 0.0f);
            GL.Vertex3(fwdt, fhgt, 0.0f);
            GL.Vertex3(fwdt, fhgt-botHlHgt, 0.0f);
            GL.Vertex3(0.0f, fhgt, 0.0f);
            GL.Vertex3(fwdt, fhgt-botHlHgt, 0.0f);
            GL.Vertex3(0.0f, fhgt-botHlHgt, 0.0f);
            //
            float wFillTop = 10.0f;
            GL.Color(new Color(0.8f, 0.8f, 0.8f, 1.0f));
            GL.Vertex3(0.0f, fhgt-botHlHgt, 0.0f);
            GL.Vertex3(fwdt, fhgt-botHlHgt, 0.0f);
            GL.Vertex3(fwdt, wFillTop, 0.0f);
            GL.Vertex3(0.0f, fhgt-botHlHgt, 0.0f);
            GL.Vertex3(fwdt, wFillTop, 0.0f);
            GL.Vertex3(0.0f, wFillTop, 0.0f);
            //
            GL.Color(new Color(0.8f, 0.8f, 0.8f, 1.0f));
            GL.Vertex3(0.0f, wFillTop, 0.0f);
            GL.Vertex3(fwdt, wFillTop, 0.0f);
            GL.Color(new Color(0.8f, 0.8f, 0.8f, 0.0f));
            GL.Vertex3(fwdt, 0.0f, 0.0f);
            GL.Color(new Color(0.8f, 0.8f, 0.8f, 1.0f));
            GL.Vertex3(0.0f, wFillTop, 0.0f);
            GL.Color(new Color(0.8f, 0.8f, 0.8f, 0.0f));
            GL.Vertex3(fwdt, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);


        GL.Color(Color.black);
            // Notch one outlines at the edge of octave
            //GL.Vertex3(10.0f, 10.0f, 0.0f);
            //GL.Vertex3( 0.0f, 10.0f, 0.0f);
            //GL.Vertex3(10.0f, 0.0f, 0.0f);
            //GL.Vertex3(0.0f, 10.0f, 0.0f);
            //GL.Vertex3(0.0f, 0.0f, 0.0f);
            //GL.Vertex3(10.0f, 0.0f, 0.0f);
            //
            GL.Vertex3(2.0f,            fhgt - 2.0f, 0.0f);
            GL.Vertex3(2.0f,            fhgt - 0.0f, 0.0f);
            GL.Vertex3(0.0f,            fhgt - 2.0f, 0.0f);
            //
            GL.Vertex3(0.0f,            0.0f, 0.0f);
            GL.Vertex3(2.0f,            0.0f, 0.0f);
            GL.Vertex3(0.0f,            fhgt - 2.0f, 0.0f);

            GL.Vertex3(0.0f,            fhgt - 2.0f, 0.0f);
            GL.Vertex3(2.0f, 10.0f,     0.0f);
            GL.Vertex3(2.0f,            fhgt - 2.0f, 0.0f);

            // The right bar of the octave
            // GL.Vertex3(fwdt,            10.0f, 0.0f);
            // GL.Vertex3(fwdt - 10.0f,    10.0f, 0.0f);
            // GL.Vertex3(fwdt - 10.0f,    0.0f, 0.0f);
            // GL.Vertex3(fwdt, 10.0f, 0.0f);
            // GL.Vertex3(fwdt - 10.0f, 0.0f, 0.0f);
            // GL.Vertex3(fwdt, 0.0f, 0.0f);
            //
            GL.Vertex3(fwdt - 2.0f,     fhgt - 0.0f, 0.0f);
            GL.Vertex3(fwdt - 2.0f,     fhgt - 2.0f, 0.0f);
            GL.Vertex3(fwdt,            fhgt - 2.0f, 0.0f);
            //
            GL.Vertex3(fwdt - 0.0f,     0.0f, 0.0f);
            GL.Vertex3(fwdt - 2.0f,     0.0f, 0.0f);
            GL.Vertex3(fwdt - 0.0f,     fhgt - 2.0f, 0.0f);

            GL.Vertex3(fwdt - 0.0f,     fhgt - 2.0f, 0.0f);
            GL.Vertex3(fwdt - 2.0f,     0.0f, 0.0f);
            GL.Vertex3(fwdt - 2.0f,     fhgt - 2.0f, 0.0f);

            // The bridge between
            GL.Vertex3(2.0f,            fhgt - 2.0f, 0.0f);
            GL.Vertex3(2.0f,            fhgt, 0.0f);
            GL.Vertex3(fwdt - 2.0f,     fhgt, 0.0f);

            GL.Vertex3(fwdt - 2.0f,     fhgt, 0.0f);
            GL.Vertex3(fwdt - 2.0f,     fhgt - 2.0f, 0.0f);
            GL.Vertex3(2.0f,            fhgt - 2.0f, 0.0f);

            Color blackKeyLt = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            Color blackKeyOp = new Color(0.43f, 0.43f, 0.43f, 1.0f);
            Color blackKeyTr = new Color(0.43f, 0.43f, 0.43f, 0.0f);
            float midPt = (float)fhgt / 2.0f;
            for(int i = 0; i < 6; ++i)
            { 
                float ox = (float)((float)i + 1.0f) / 7.0f * (float)width;
                float x = ox - 1.0f;

                float t = midPt;
                if(i == 2)
                    t = 00.0f;

                GL.Vertex3(x, t, 0.0f);
                GL.Vertex3(x + 2.0f, t, 0.0f);
                GL.Vertex3(x + 2.0f, fhgt, 0.0f);

                GL.Vertex3(x, t, 0.0f);
                GL.Vertex3(x + 2.0f, fhgt, 0.0f);
                GL.Vertex3(x, fhgt, 0.0f);

                // Draw the black keys
                if(i != 2)
                { 
                    float cbl = ox - halfBW;
                    float cbr = ox + halfBW;
                    float bFillTop = wFillTop;

                    GL.Color(blackKeyLt);
                    GL.Vertex3(cbl, midPt, 0.0f);
                    GL.Vertex3(cbl, midPt - hlHeight, 0.0f);
                    GL.Vertex3(cbr, midPt- hlHeight, 0.0f);
                    GL.Vertex3(cbl, midPt, 0.0f);
                    GL.Vertex3(cbr, midPt - hlHeight, 0.0f);
                    GL.Vertex3(cbr, midPt, 0.0f);

                    GL.Color(blackKeyOp);
                    GL.Vertex3(cbl, midPt - hlHeight, 0.0f);
                    GL.Vertex3(cbl, bFillTop, 0.0f);
                    GL.Vertex3(cbr, bFillTop, 0.0f);
                    GL.Vertex3(cbl, midPt - hlHeight, 0.0f);
                    GL.Vertex3(cbr, bFillTop, 0.0f);
                    GL.Vertex3(cbr, midPt - hlHeight, 0.0f);

                    GL.Color(blackKeyOp);
                    GL.Vertex3(cbl, wFillTop, 0.0f);
                    GL.Vertex3(cbr, wFillTop, 0.0f);
                    GL.Color(blackKeyTr);
                    GL.Vertex3(cbr, 0.0f, 0.0f);
                    GL.Color(blackKeyOp);
                    GL.Vertex3(cbl, wFillTop, 0.0f);
                    GL.Color(blackKeyTr);
                    GL.Vertex3(cbr, 0.0f, 0.0f);
                    GL.Vertex3(cbl, 0.0f, 0.0f);


                    // Draw the outlines
                    GL.Color(Color.black);
                    // Left of black
                    GL.Vertex3(cbl - 2.0f, midPt, 0.0f);
                    GL.Vertex3(cbl, midPt, 0.0f);
                    GL.Vertex3(cbl, midPt + 2.0f, 0.0f);
                    //
                    GL.Vertex3(cbl,         0.0f, 0.0f);
                    GL.Vertex3(cbl - 2.0f,  0.0f, 0.0f);
                    GL.Vertex3(cbl - 2.0f,  midPt, 0.0f);
                    //
                    GL.Vertex3(cbl,         0.0f, 0.0f);
                    GL.Vertex3(cbl - 2.0f,  midPt, 0.0f);
                    GL.Vertex3(cbl,         midPt, 0.0f);

                    // Right of black
                    GL.Vertex3(cbr,         midPt, 0.0f);
                    GL.Vertex3(cbr + 2.0f,  midPt, 0.0f);
                    GL.Vertex3(cbr,  midPt + 2.0f, 0.0f);
                    //
                    GL.Vertex3(cbr + 2.0f,  0.0f, 0.0f);
                    GL.Vertex3(cbr,         0.0f, 0.0f);
                    GL.Vertex3(cbr + 2.0f,  midPt, 0.0f);
                    //
                    GL.Vertex3(cbr,         0.0f, 0.0f);
                    GL.Vertex3(cbr + 2.0f,  midPt, 0.0f);
                    GL.Vertex3(cbr,         midPt, 0.0f);

                    // Bridge
                    GL.Vertex3(cbl, midPt, 0.0f);
                    GL.Vertex3(cbl, midPt + 2.0f, 0.0f);
                    GL.Vertex3(cbr, midPt, 0.0f);

                    GL.Vertex3(cbl, midPt + 2.0f, 0.0f);
                    GL.Vertex3(cbr, midPt, 0.0f);
                    GL.Vertex3(cbr, midPt + 2.0f, 0.0f);
                }
            }

        GL.End();

        RenderTexture.active = null;

        if (this.rtOlys != null)
            this.rtOlys.Release();

        GL.PopMatrix();
        this.imgDst.texture = this.rtKeys;
        this.imgThumb.texture = this.rtKeys;

        this.UpdateThumbUVs();
        this.bevOctaveDirty = false;
    }

    public void UpdateThumbUVs()
    { 
        if(this.rtKeys == null)
            return;

        float keyAreaWidth = this.app.keyboardPane.GetKeyboardWidth();
        float thumbWidth = this.app.keyboardPane.pianoScroll.handleRect.rect.width;

        int lo = this.app.keyboardPane.lowerOctave;
        int uo = this.app.keyboardPane.upperOctave;
        float octaves = uo - lo + 1.0f;

        if (thumbWidth >= keyAreaWidth)
        { 
            this.imgThumb.rectTransform.anchorMin = new Vector2(0.0f, this.imgThumb.rectTransform.anchorMin.y);
            this.imgThumb.rectTransform.anchorMax = new Vector2(1.0f, this.imgThumb.rectTransform.anchorMax.y);

            this.imgThumb.uvRect = new Rect((float)(lo - 1), 0.0f, octaves, 1.0f);

            float halfDiff = (thumbWidth - keyAreaWidth)/thumbWidth * 0.5f;
            this.imgThumb.rectTransform.anchorMin = new Vector2(halfDiff, this.imgThumb.rectTransform.anchorMin.y);
            this.imgThumb.rectTransform.anchorMax = new Vector2(1.0f - halfDiff, this.imgThumb.rectTransform.anchorMax.y);
            this.imgThumb.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
        else
        {
            Rect rPlate = this.imgDst.rectTransform.rect;
            Rect rThumb = this.imgThumb.rectTransform.rect;

            float thxmin = (float)(lo - 1) + scrollbar.value * ((rPlate.width - rThumb.width) / rPlate.width) * octaves;
            float thwid = rThumb.width / rPlate.width * octaves;
            //

            this.imgThumb.rectTransform.anchorMin = new Vector2(0.0f, this.imgThumb.rectTransform.anchorMin.y);
            this.imgThumb.rectTransform.anchorMax = new Vector2(1.0f, this.imgThumb.rectTransform.anchorMax.y);
            this.imgThumb.color = Color.white;

            this.imgThumb.uvRect =
                new Rect(
                    thxmin,
                    0.0f,
                    thwid,
                    this.imgDst.uvRect.height);
        }
    }
}
