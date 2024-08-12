using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class EleToggle : EleGenToggle<UnityEngine.UI.Toggle>
        {
            public EleToggle(
                EleBaseRect parent,
                TextAttrib textAttrib,
                string text,
                SelectableInfo plateStyle,
                float toggleWidth,
                Sprite toggleSprite,
                UnityEngine.UI.Image.Type icoType,
                PadRect togglePad,
                Vector2 size,
                float separation = 0.0f,
                string name = "")
                : base(
                      parent, 
                      textAttrib, 
                      text,
                      plateStyle, 
                      toggleWidth, 
                      toggleSprite, 
                      icoType, 
                      togglePad,
                      size, 
                      separation, 
                      name)
            {}

            public EleToggle(
                EleBaseRect parent,
                TextAttrib textAttrib,
                string text,
                SelectableInfo plateStyle,
                float toggleWidth,
                Sprite toggleSprite,
                UnityEngine.UI.Image.Type icoType,
                Vector2 size,
                float separation = 0.0f,
                string name = "")
                : base(
                      parent, 
                      textAttrib, 
                      text,
                      plateStyle,
                      toggleWidth,
                      toggleSprite,
                      icoType,
                      size,
                      separation,
                      name)
            {}
        }

        public class EleGenToggle<ty> : EleBaseRect
            where ty : UnityEngine.UI.Toggle
        {
            public ty toggle = null;
            UnityEngine.UI.Image plate = null;
            UnityEngine.UI.Image checkbox = null;
            UnityEngine.UI.Text label = null;

            // NOTE: There maybe no need to hold onto this.
            PadRect padding = new PadRect(0.0f);

            float separation = 0.0f;
            float toggleWidth = 50.0f;

            public override RectTransform RT => this.plate.rectTransform;
            public override bool CanHaveChildren() => false;


            public EleGenToggle(
                EleBaseRect parent,
                TextAttrib textAttrib,
                string text,
                SelectableInfo plateStyle,
                float toggleWidth,
                Sprite toggleSprite,
                UnityEngine.UI.Image.Type icoType,
                PadRect togglePad, 
                Vector2 size,
                float separation = 0.0f,
                string name = "")
                : base(parent, size, name)
            {
                this._Create(
                    parent,
                    textAttrib,
                    text,
                    plateStyle,
                    toggleWidth,
                    toggleSprite,
                    icoType,
                    togglePad,
                    size,
                    separation,
                    name);
            }

            public EleGenToggle(
                EleBaseRect parent,
                TextAttrib textAttrib,
                string text,
                SelectableInfo plateStyle,
                float toggleWidth,
                Sprite toggleSprite,
                UnityEngine.UI.Image.Type icoType,
                Vector2 size,
                float separation = 0.0f,
                string name = "")
                : base(parent, size, name)
            {
                this._Create(
                    parent,
                    textAttrib,
                    text,
                    plateStyle,
                    toggleWidth,
                    toggleSprite,
                    icoType,
                    new PadRect(0.0f), size, separation,
                    name);
            }

            protected void _Create(
                EleBaseRect parent,
                TextAttrib textAttrib,
                string text,
                SelectableInfo plateStyle,
                float toggleWidth,
                Sprite toggleSprite,
                UnityEngine.UI.Image.Type icoType,
                PadRect togglePad,
                Vector2 size,
                float separation,
                string name)
            {
                this.minSize = size;

                this.separation = separation;
                this.toggleWidth = toggleWidth;
                this.padding = togglePad;

                GameObject go = new GameObject("Checkbox_" + name);
                go.transform.SetParent(parent.GetContentRect(), false);
                //
                this.plate = go.AddComponent<UnityEngine.UI.Image>();
                this.plate.type = UnityEngine.UI.Image.Type.Sliced;
                this.plate.RTQ().TopLeftAnchorsPivot();
                //
                this.toggle = go.AddComponent<ty>();
                plateStyle.Apply(this.toggle, this.plate);

                GameObject goCheckmark = new GameObject("Checkmark_" + name);
                goCheckmark.transform.SetParent(go.transform, false);
                this.checkbox = goCheckmark.AddComponent<UnityEngine.UI.Image>();
                this.checkbox.sprite = toggleSprite;
                this.checkbox.type = icoType;
                
                if (icoType != UnityEngine.UI.Image.Type.Simple)
                {
                    this.checkbox.
                        RTQ().
                            CenterPivot().
                            ExpandParentFlush().
                            OffsetMin(togglePad.left, togglePad.bot).
                            OffsetMax(-togglePad.right, -togglePad.top);
                }
                else
                {
                    this.checkbox.RTQ().
                        CenterPivot().
                        OffsetMin(0.5f, 0.5f).
                        OffsetMax(0.5f, 0.5f).
                        AnchPos(
                            (togglePad.left - togglePad.right) * 0.5f,
                            (togglePad.bot - togglePad.top) * 0.5f).
                        SizeDelta(toggleSprite.rect.size);
                }

                if (textAttrib != null)
                {
                    GameObject goText = new GameObject("Text_" + name);
                    goText.transform.SetParent(go.transform, false);

                    this.label = goText.AddComponent<UnityEngine.UI.Text>();
                    this.label.text = text;
                    this.label.horizontalOverflow = HorizontalWrapMode.Overflow;
                    this.label.verticalOverflow = VerticalWrapMode.Overflow;
                    this.label.alignment = TextAnchor.LowerLeft;
                    this.label.RTQ().Pivot(0.0f, 0.0f).BotLeftAnchors();
                    textAttrib.Apply(this.label);
                }

                this.toggle.targetGraphic = this.plate;
                this.toggle.graphic = this.checkbox;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                float x = this.toggleWidth;
                if (this.label != null)
                {
                    x += this.separation;

                    TextGenerationSettings tgs = this.label.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                    tgs.scaleFactor = 1.0f;
                    TextGenerator tg = this.label.cachedTextGenerator;
                    x += tg.GetPreferredWidth(this.label.text, tgs);
                }

                return Mathf.Max(x, this.minSize.x);
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths,
                float width,
                bool collapsable = true)
            {
                float x = this.toggleWidth;
                float y = this.minSize.y;
                if (this.label != null)
                {
                    x += this.separation;

                    TextGenerationSettings tgs = this.label.GetGenerationSettings(new Vector2(float.PositiveInfinity, float.PositiveInfinity));
                    tgs.scaleFactor = 1.0f;
                    TextGenerator tg = this.label.cachedTextGenerator;
                    x += tg.GetPreferredWidth(this.label.text, tgs);

                    y = Mathf.Max(y, tg.GetPreferredHeight(this.label.text, tgs));
                }

                x = Mathf.Max(x, this.minSize.x);
                return new Vector2(x, y);
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset,
                Vector2 offset,
                Vector2 size,
                bool collapsable = true)
            {
                //return base.Layout(cached, widths, rectOffset, offset, size);

                RectTransform rtP = this.plate.rectTransform;
                rtP.anchoredPosition = new Vector2(rectOffset.x, -rectOffset.y);
                rtP.sizeDelta = new Vector2(this.toggleWidth, size.y);

                if (this.label != null)
                {
                    float labOff = this.toggleWidth + this.separation;

                    RectTransform rtL = this.label.rectTransform;
                    rtL.anchoredPosition = new Vector2(labOff, 0.0f);
                    rtL.sizeDelta = new Vector2(size.x - labOff, size.y);
                }

                return size;
            }
        }
    }
}