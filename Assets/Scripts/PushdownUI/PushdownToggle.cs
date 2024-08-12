using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushdownToggle : UnityEngine.UI.Toggle
{
    public struct OriginalInfo
    { 
        public Vector2 anchoredPos;
        public Color c;

        public OriginalInfo(UnityEngine.UI.Graphic g)
        { 
            this.c = g.color;
            this.anchoredPos = g.rectTransform.anchoredPosition;
        }

        public void Apply(UnityEngine.UI.Graphic g)
        { 
            g.rectTransform.anchoredPosition = this.anchoredPos;
            g.color = this.c;
        }
    }

    [System.Serializable]
    public struct OnActivateGraphic
    { 
        public Color color;
        public UnityEngine.UI.Graphic graphic;

        public OnActivateGraphic(Color color, UnityEngine.UI.Graphic graphic)
        { 
            this.color = color;
            this.graphic = graphic;
        }
    }

    public UnityEngine.UI.SpriteState downSpriteState;
    public UnityEngine.UI.ColorBlock downColorBlock;
    public Color downColor;
    public Sprite downSprite;

    UnityEngine.UI.SpriteState upSpriteState;
    UnityEngine.UI.ColorBlock upColorBlock;
    Color upColor;
    Sprite upSprite;

    public Vector2 pushdown = new Vector2(0.0f, -5.0f); 

    public UnityEngine.UI.Image plate;

    Dictionary<UnityEngine.UI.Graphic, OriginalInfo> originalGraphicInfo = 
        new System.Collections.Generic.Dictionary<UnityEngine.UI.Graphic, OriginalInfo>();

    public List<OnActivateGraphic> graphicsToActivate = 
        new List<OnActivateGraphic>();

    bool initialized = false;

    protected override void Awake()
    {
        base.Awake();

        this.Initialize();
    }

    public void Initialize(bool force = false)
    {
        if(UnityEngine.Application.isPlaying == false)
            return;

        if(this.initialized == true && force == false)
            return;

        this.initialized = true;

        this.onValueChanged.AddListener(this.OnValueChanged);

        this.upSpriteState = this.spriteState;
        this.upColorBlock = this.colors;
        this.upColor = this.targetGraphic.color;

        if (this.plate != null)
            this.upSprite = this.plate.sprite;

        if(this.isOn == true)
            SetSpriteDown(true);
    }


    public void OnValueChanged(bool x)
    { 
        if(UnityEngine.Application.isPlaying == false)
            return;

        this.Initialize();

        this.SetSpriteDown(x);
    }

    public void SetSpriteDown(bool down)
    {
        if (down == true)
        {
            this.RestoreChildrenStates();

            this.spriteState = this.downSpriteState;
            this.colors = this.downColorBlock;
            this.targetGraphic.color = this.downColor;

            if (this.plate != null)
                this.plate.sprite = this.downSprite;

            this.SaveChildrenStates();
            foreach (KeyValuePair<UnityEngine.UI.Graphic, OriginalInfo> kvp in this.originalGraphicInfo)
            {
                kvp.Key.rectTransform.anchoredPosition += this.pushdown;

                //Color c = kvp.Value.c;
                //kvp.Key.color = c;
            }

            foreach (OnActivateGraphic oag in this.graphicsToActivate)
                oag.graphic.color = oag.color;
        }
        else
        {
            this.spriteState = this.upSpriteState;
            this.colors = this.upColorBlock;
            this.targetGraphic.color = this.upColor;

            if (this.plate != null)
                this.plate.sprite = this.upSprite;

            this.RestoreChildrenStates();
        }
    }

    void SaveChildrenStates()
    { 
        this.originalGraphicInfo = 
            new Dictionary<UnityEngine.UI.Graphic, OriginalInfo>();

        foreach(Transform t in this.transform)
        { 
            UnityEngine.UI.Graphic g = t.GetComponent<UnityEngine.UI.Graphic>();

            if(g == null)
                continue;

            this.originalGraphicInfo.Add(g, new OriginalInfo(g));
        }
    }

    void RestoreChildrenStates()
    { 
        if(this.originalGraphicInfo == null)
            return;

        foreach(KeyValuePair<UnityEngine.UI.Graphic, OriginalInfo> kvp in this.originalGraphicInfo)
            kvp.Value.Apply(kvp.Key);

        this.originalGraphicInfo = null;
    }
}
