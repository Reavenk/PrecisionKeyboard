using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIL
    { 
        public class EleText : EleBaseRect
        { 
            public UnityEngine.UI.Text text;
            bool wrap = false;

            public EleText(EleBaseRect parent, string text, bool wrap, Font font, Color color, int fontSize, Vector2 size, string name = "")
                : base(parent, size, name)
            { 
                this._Create(parent, text, wrap, font, color, fontSize, size, name);
            }

            public EleText(EleBaseRect parent, string text, bool wrap, Font font, Color color, int fontSize)
                : base(parent, new Vector2(-1.0f, -1.0f), "")
            {
                this._Create(parent, text, wrap, font, color, fontSize, new Vector2(-1.0f, -1.0f), "");
            }

            protected void _Create(EleBaseRect parent, string text, bool wrap, Font font, Color color, int fontSize, Vector2 size, string name)
            {
                GameObject go = new GameObject("Text_" + name);
                go.transform.SetParent(parent.GetContentRect());

                this.text = go.AddComponent<UnityEngine.UI.Text>();
                this.text.color = color;
                this.text.font = font;
                this.text.fontSize = fontSize;
                this.text.alignment = TextAnchor.UpperLeft;
                this.text.text = text;
                this.text.verticalOverflow = VerticalWrapMode.Overflow;
                this.text.horizontalOverflow = 
                    wrap ? 
                        HorizontalWrapMode.Wrap : 
                        HorizontalWrapMode.Overflow;

                SetRTTopLeft(this.text.rectTransform);

                this.wrap = wrap;
                if (wrap == true)
                {
                    this.text.horizontalOverflow = HorizontalWrapMode.Wrap;
                }
                else
                {
                    this.text.horizontalOverflow = HorizontalWrapMode.Overflow;
                }

                this.text.rectTransform.RTQ().Identity();
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable = true)
            { 
                Vector2 ret = base.Layout(cached, widths, rectOffset, offset, size);
                return ret;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                if (this.text.font == null)
                    return 0.0f;

                float minWrapWidth = 50.0f;

                TextGenerationSettings tgs = 
                    (this.wrap == true) ?
                        this.text.GetGenerationSettings(new Vector2(minWrapWidth, float.PositiveInfinity)) :
                        this.text.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));

                //tgs.scaleFactor = 1.0f;

                TextGenerator tg = this.text.cachedTextGenerator;

                float ret = tg.GetPreferredWidth(this.text.text, tgs) / this.text.rectTransform.lossyScale.x;

                // There's an odd issue with wrapping not being honored.
                if(this.wrap == false)
                    return Mathf.Ceil(ret) + 1.0f;
                else
                    return Mathf.Min(minWrapWidth, ret);
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache, 
                Dictionary<Ele, float> widths, 
                float width,
                bool collapsable = true)
            { 
                if(this.text.font == null)
                    return new Vector2(0.0f, 0.0f);

                if(this.wrap == false)
                    width = float.PositiveInfinity;

                TextGenerationSettings tgs = this.text.GetGenerationSettings(new Vector2(width, Mathf.Infinity));

                // This is freaking bizzare. We need to account for scale, but we can't just do ...
                //tgs.scaleFactor = 1.0f;
                // ... because that can screw up the calculation of how many lines are in the 
                // output when GetPreferredHeight is called.
                // 
                TextGenerator tg = this.text.cachedTextGenerator;
                // So we end up accounting for it by looking at the world scale.
                float y = Mathf.Ceil(tg.GetPreferredHeight(this.text.text, tgs)) / this.text.rectTransform.lossyScale.y + 1.0f;

                if(this.wrap == true)
                {
                    float entireLineX = Mathf.Ceil(tg.GetPreferredWidth(this.text.text, tgs)) / this.text.rectTransform.lossyScale.x + 1.0f;
                    return new Vector2(Mathf.Min(width, entireLineX), y);
                }

                float x = Mathf.Ceil(tg.GetPreferredWidth(this.text.text, tgs)) / this.text.rectTransform.lossyScale.x + 1.0f;
                return new Vector2(x,y);
            }

            public override RectTransform RT
            {
                get{ return this.text.rectTransform; }
            }
        }
    }
}