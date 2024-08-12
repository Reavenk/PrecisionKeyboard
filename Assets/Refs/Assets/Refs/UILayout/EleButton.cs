using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIL
    {
        public class EleButton : EleGenButton<UnityEngine.UI.Button>
        {
            public EleButton(EleBaseRect parent, Font font, int fontSize, Color fontColor, string text, Sprite plateSprite, Vector2 size, string name = "")
                : base(parent, font, fontSize, fontColor, text, plateSprite, size, name)
            {
                //this._Create(parent, font, fontSize, fontColor, text, plateSprite, size, name);
            }

            public EleButton(EleBaseRect parent, Font font, int fontSize, Color fontColor, string text, Sprite plateSprite)
                : base(parent, font, fontSize, fontColor, text, plateSprite)
            {
                //this._Create(parent, font, fontSize, fontColor, text, plateSprite, new Vector2(-1.0f, -1.0f), "");
            }

            public EleButton(EleBaseRect parent, Sprite plateSprite, Vector2 size, string name = "")
                : base(parent, plateSprite, size, name)
            {
                //this._Create(parent, null, 0, Color.black, null, plateSprite, size, name);
            }

            public EleButton(EleBaseRect parent, Sprite plateSprite)
                : base(parent, plateSprite)
            {
                //this._Create(parent, null, 0, Color.black, null, plateSprite, new Vector2(-1.0f, -1.0f), "");
            }
        }

        public class EleGenButton<BtnTy> : EleBaseRect
            where BtnTy : UnityEngine.UI.Button
            
        {
            BtnTy button;
            UnityEngine.UI.Image plate;
            public UnityEngine.UI.Image Plate { get { return this.plate; } }

            public UnityEngine.UI.Text text = null;
            public PadRect border = new PadRect(10.0f, 10.0f, 10.0f, 1.0f);

            public BtnTy Button {get{return this.button; } }

            public EleGenButton(EleBaseRect parent, Font font, int fontSize, Color fontColor, string text, Sprite plateSprite, Vector2 size, string name = "")
                : base(parent, size, name)
            { 
                this._Create(parent, font, fontSize, fontColor, text, plateSprite, size, name);
            }

            public EleGenButton(EleBaseRect parent, Font font, int fontSize, Color fontColor, string text, Sprite plateSprite)
                : base(parent)
            { 
                this._Create(parent, font, fontSize, fontColor, text, plateSprite, new Vector2(-1.0f, -1.0f), "");
            }

            public EleGenButton(EleBaseRect parent, Sprite plateSprite, Vector2 size, string name = "")
                : base(parent, size, name)
            { 
                this._Create(parent, null, 0, Color.black, null, plateSprite, size, name);
            }

            public EleGenButton(EleBaseRect parent, Sprite plateSprite)
                : base(parent)
            { 
                this._Create(parent, null, 0, Color.black, null, plateSprite, new Vector2(-1.0f, -1.0f), "");
            }

            protected void _Create(EleBaseRect parent, Font font, int fontSize, Color fontColor, string text, Sprite plateSprite, Vector2 size, string name)
            { 
                GameObject go = new GameObject("Button_" + name);

                go.transform.SetParent(parent.GetContentRect(), false);

                this.plate = go.AddComponent<UnityEngine.UI.Image>();
                this.button = go.AddComponent<BtnTy>();
                this.plate.RTQ().TopLeftAnchorsPivot();

                this.button.targetGraphic = this.plate;
                this.plate.sprite = plateSprite;
                this.plate.type = UnityEngine.UI.Image.Type.Sliced;

                if(string.IsNullOrEmpty(text) == false)
                { 
                    GameObject goChild = new GameObject("ButtonText_" + name);
                    goChild.transform.SetParent(go.transform, false);

                    this.text = goChild.AddComponent<UnityEngine.UI.Text>();
                    this.text.text                      = text;
                    this.text.font                      = font;
                    this.text.color                     = fontColor;
                    this.text.fontSize                  = fontSize;
                    this.text.horizontalOverflow        = HorizontalWrapMode.Overflow;
                    this.text.verticalOverflow          = VerticalWrapMode.Overflow;
                    this.text.alignment                 = TextAnchor.MiddleCenter;
                    this.text.rectTransform.RTQ().ExpandParentFlush();
                }
            }

            public override RectTransform RT
            {
                get{ return this.plate.rectTransform; }
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            { 
                float min = 0.0f;
                if(this.sizer != null)
                { 
                    float szmin = this.sizer.GetMinWidth(cache);
                    min = Mathf.Max(min, szmin);
                }

                if(this.text != null)
                {
                    TextGenerationSettings tgs =
                        this.text.GetGenerationSettings(
                            new Vector2(
                                float.PositiveInfinity,
                                float.PositiveInfinity));

                    tgs.scaleFactor = 1.0f;

                    float width = this.text.cachedTextGenerator.GetPreferredWidth(this.text.text, tgs);
                    width = Mathf.Ceil(width) + 1.0f;
                    min = Mathf.Max(min, width);
                }

                min += this.border.width;
                return min;
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache, 
                Dictionary<Ele, float> widths, 
                float width,
                bool collapsable = true)
            {
                Vector2 min = Vector2.zero;
                if(this.sizer != null)
                { 
                    Vector2 szMin = this.sizer.GetMinSize(cache, widths, width);
                    min = Vector2.Max(min, szMin);
                }

                if(this.text != null)
                { 
                    TextGenerationSettings tgs = 
                        this.text.GetGenerationSettings(
                            new Vector2(
                                float.PositiveInfinity, 
                                float.PositiveInfinity));

                    tgs.scaleFactor = 1.0f;

                    Vector2 minTxt = 
                        new Vector2(
                            this.text.cachedTextGenerator.GetPreferredWidth(this.text.text, tgs),
                            this.text.cachedTextGenerator.GetPreferredHeight(this.text.text, tgs));

                    minTxt.x = Mathf.Ceil(minTxt.x) + 1.0f;
                    minTxt.y = Mathf.Ceil(minTxt.y) + 1.0f;

                    min = Vector2.Max(min, minTxt);
                }

                min.x += this.border.width;
                min.y += this.border.height;
                min = Vector2.Max(min, this.minSize);
                return min;
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable = true)
            {
                this.plate.rectTransform.anchoredPosition = new Vector2(rectOffset.x, -rectOffset.y);
                this.plate.rectTransform.sizeDelta = size;

                if(this.text != null)
                {
                    //Vector2 innerSz = size - this.padding.dim;

                    //TextGenerationSettings tgs =
                    //    this.text.GetGenerationSettings(
                    //        new Vector2(
                    //            innerSz.x,
                    //            float.PositiveInfinity));

                    //TextGenerator tg = this.text.cachedTextGenerator;
                    //float width = tg.GetPreferredWidth(this.text.text, tgs);
                    //float height = tg.GetPreferredHeight(this.text.text, tgs);

                    //this.text.rectTransform.anchoredPosition = 
                    //    new Vector2(
                    //        this.padding.left + (innerSz.x - width)/2.0f,
                    //        -this.padding.top - (innerSz.y - height)/2.0f);

                    //this.text.rectTransform.sizeDelta = 
                    //    new Vector2(
                    //        width, 
                    //        height);
                }

                //return base.Layout(cached, widths, rectOffset, offset, size);
                RectTransform rt = this.RT;

                rt.anchoredPosition = new Vector2(rectOffset.x, -rectOffset.y);

                Vector2 ret = size;
                if(this.sizer != null)
                { 
                    offset.x += this.border.left;
                    offset.y += this.border.top;

                    ret.x -= this.border.width;
                    ret.y -= this.border.height;

                    ret = this.sizer.Layout(cached, widths, new Vector2(this.border.left, this.border.top), offset, ret, collapsable);
                    ret.x += this.border.width;
                    ret.y += this.border.height;

                    ret = Vector2.Max(ret, size);
                }
                rt.sizeDelta = ret;


                return ret;
            }
        }
    }
}