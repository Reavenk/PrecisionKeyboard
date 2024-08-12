using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class EleHeader : EleBaseRect
        {
            UnityEngine.UI.Image plate;
            UnityEngine.UI.Text text;
            public PadRect border = new PadRect(10.0f, 10.0f, 10.0f, 10.0f);

            public EleHeader(EleBaseRect parent, string text, Font font, Color fontColor, int fontPointSize, Sprite frame, PadRect padding, Vector2 size, string name = "")
                : base(parent, size, name)
            { 
                this._Create(
                    parent, 
                    text, 
                    font, 
                    fontColor, 
                    fontPointSize, 
                    frame, 
                    padding, 
                    size, 
                    name);
            }

            public EleHeader(EleBaseRect parent, string text, Font font, Color fontColor, int fontPointSize, Sprite frame, PadRect padding)
                : base(parent)
            {
                this._Create(
                    parent, 
                    text, 
                    font, 
                    fontColor, 
                    fontPointSize, 
                    frame, 
                    padding, 
                    new Vector2(-1.0f, -1.0f), 
                    "");
            }

            protected void _Create(EleBaseRect parent, string text, Font font, Color fontColor, int fontPointSize, Sprite frame, PadRect padding, Vector2 size, string name)
            {
                GameObject goPlate = new GameObject("HeaderPlate_" + name);
                goPlate.transform.SetParent(parent.GetContentRect());

                this.plate = goPlate.AddComponent<UnityEngine.UI.Image>();
                this.plate.rectTransform.RTQ().Identity().TopLeftAnchorsPivot().ZeroOffsets();

                this.plate.sprite = frame;
                this.plate.type = UnityEngine.UI.Image.Type.Sliced;

                this.border = padding;

                GameObject goText = new GameObject("HeaderText_" + name);
                goText.transform.SetParent(this.plate.rectTransform);

                this.text = goText.AddComponent<UnityEngine.UI.Text>();
                this.text.rectTransform.RTQ().Identity().TopLeftAnchorsPivot().ZeroOffsets();

                this.text.font      = font;
                this.text.color     = fontColor;
                this.text.fontSize  = fontPointSize;
                this.text.text      = text;
            }

            protected override Vector2 ImplCalcMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true)
            {
                Vector2 ret = new Vector2();

                if(this.text != null)
                {
                    TextGenerationSettings tgs = 
                        this.text.GetGenerationSettings(
                            new Vector2(
                                float.PositiveInfinity, 
                                float.PositiveInfinity));

                    TextGenerator tg = this.text.cachedTextGenerator;
                    ret.x = tg.GetPreferredWidth(this.text.text, tgs);
                    ret.y = tg.GetPreferredHeight(this.text.text, tgs);
                }

                if(this.HasChildren() == true)
                { 
                    Vector2 v2 = this.GetMinSize(cache, widths, width);
                    ret.x = Mathf.Max(ret.x, v2.x);
                    ret.y = Mathf.Max(ret.y, v2.y);
                }

                ret.x += this.border.width;
                ret.y += this.border.height;
                return ret;
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

                if (this.text != null)
                {
                    TextGenerationSettings tgs =
                        this.text.GetGenerationSettings(
                            new Vector2(
                                float.PositiveInfinity,
                                float.PositiveInfinity));

                    TextGenerator tg = this.text.cachedTextGenerator;
                    float textWidth = tg.GetPreferredWidth(this.text.text, tgs);
                    float textHeight = tg.GetPreferredHeight(this.text.text, tgs);

                    this.text.rectTransform.anchoredPosition = new Vector2(this.border.left, -this.border.top);
                    this.text.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
                }

                if(this.HasChildren() == true)
                { 
                    Vector2 internalSz = size;
                    internalSz.x -= this.border.width;
                    internalSz.y -= this.border.height;
                    base.Layout(cached, widths, rectOffset, offset, internalSz);
                }

                // TODO:
                return size;
            }

            public override RectTransform RT => this.plate.rectTransform;

            public override void Deconstruct()
            {
                GameObject.Destroy(this.plate.gameObject);
            }
        }
    }
}
