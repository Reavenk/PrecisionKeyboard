using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIDIPrevGlyph : MonoBehaviour
{    
    public PaneKeyboard pkManager;

    const float lifetime = 1.0f;
    const float heightDrop = 50.0f;

    UnityEngine.UI.Image image;
    float startPos;
    float endPos;

    float startTime;

    int noteID;
    public int NoteID{get{return this.noteID; } }

    void Start()
    {
        this.startTime = Time.time;
    }

    void Update()
    {
        float timeAlive = Time.time - this.startTime;
        if(timeAlive >= lifetime)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }

        float lam = timeAlive / lifetime;
        float yPos = Mathf.Lerp(this.startPos, this.endPos, lam);

        Color c = this.image.color;
        this.image.rectTransform.anchoredPosition = new Vector2(0.0f, yPos);
        this.image.color = new Color(c.r, c.g, c.b, 1.0f - lam);
    }

    public static MIDIPrevGlyph CreateOnKey(PaneKeyboard pkMgr, Key k, Sprite s, float upFrom)
    { 
        if(s == null)
            return null;

        if(k.isActiveAndEnabled == false)
            return null;

        GameObject go = new GameObject("MIDIPreviewGlyph");
        go.transform.SetParent(k.transform);

        MIDIPrevGlyph pg = go.AddComponent<MIDIPrevGlyph>();
        pg.noteID = PxPre.Phonics.WesternFreqUtils.GetNote(k.key, k.octave);
        pg.pkManager = pkMgr;
        pg.image = go.AddComponent<UnityEngine.UI.Image>();
        pg.image.sprite = s;
        //
        pg.startPos = upFrom + heightDrop;
        pg.endPos = upFrom;

        RectTransform rt = pg.image.rectTransform;
        rt.pivot = new Vector2(0.5f, 0.0f);
        rt.anchorMin = new Vector2(0.5f, 0.0f);
        rt.anchorMax = new Vector2(0.5f, 0.0f);
        rt.sizeDelta = s.rect.size;
        rt.anchoredPosition = new Vector2(0.0f, pg.startPos);

        return pg;
    }

    private void OnDestroy()
    {
        this.pkManager.OnEndedMIDIPrevGlyph(this);
    }

    private void OnDisable()
    {
        this.pkManager.OnEndedMIDIPrevGlyph(this);
    }
}
