using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class EleSlider : EleGenSlider<UnityEngine.UI.Slider>
        {
            public EleSlider(EleBaseRect parent, ScrollInfo scrollInfo, Vector2 size, string name = "")
               : base(parent, scrollInfo, size, name)
            {}
        }

        public class EleGenSlider<SldTy> : EleBaseRect
            where SldTy : UnityEngine.UI.Slider
        { 
            SldTy slider;

            public SldTy Slider {get{return this.slider; } }

            UnityEngine.UI.Image plate;
            UnityEngine.UI.Image thumb;

            public EleGenSlider(EleBaseRect parent, ScrollInfo scrollInfo, Vector2 size, string name = "")
                : base(parent, size, name)
            {
                size = new Vector2(
                    Mathf.Max(scrollInfo.scrollbarDim, size.x),
                    Mathf.Max(scrollInfo.scrollbarDim, size.y));

                this._Create(parent, scrollInfo, size);
            }

            protected void _Create(EleBaseRect parent, ScrollInfo scrollInfo, Vector2 size)
            { 
                this.minSize = size;

                GameObject go = new GameObject("Slider_" + name);
                go.transform.SetParent(parent.GetContentRect(), false);
                //
                this.plate = go.AddComponent<UnityEngine.UI.Image>();
                this.plate.type = UnityEngine.UI.Image.Type.Sliced;
                this.plate.sprite = scrollInfo.backplateSprite;
                //
                this.slider = go.AddComponent<SldTy>();
                this.plate.RTQ().TopLeftAnchorsPivot();

                GameObject slideRegion = new GameObject("Slidergn_" + name);
                slideRegion.transform.SetParent(go.transform, false);
                RectTransform rtSliderRegion = slideRegion.AddComponent<RectTransform>();
                rtSliderRegion.RTQ().ExpandParentFlush().
                    OffsetMin(scrollInfo.scrollbarDim * 0.5f, 0.0f).
                    OffsetMax(-scrollInfo.scrollbarDim * 0.5f, 0.0f);

                GameObject goThumb = new GameObject("Thumb_" + name);
                goThumb.transform.SetParent(slideRegion.transform, false);
                //
                this.thumb = goThumb.AddComponent<UnityEngine.UI.Image>();
                this.thumb.type = UnityEngine.UI.Image.Type.Sliced;
                //
                this.thumb.RTQ().CenterPivot().SizeDelta(scrollInfo.scrollbarDim, 0.0f);

                this.slider.targetGraphic = this.thumb;
                this.slider.handleRect = this.thumb.rectTransform;

                scrollInfo.Apply(this.slider, this.thumb);
            }

            public override bool CanHaveChildren() => true;

            public override RectTransform GetContentRect()
            {
                return this.thumb.rectTransform;
            }

            public override RectTransform RT {get{return this.plate.rectTransform; } }

            protected override Vector2 ImplCalcMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true)
            {
                return base.ImplCalcMinSize(cache, widths, width, collapsable);
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                return base.ImplCalcMinSizeWidth(cache);
            }

            public override Vector2 Layout(Dictionary<Ele, Vector2> cached, Dictionary<Ele, float> widths, Vector2 rectOffset, Vector2 offset, Vector2 size, bool collapsable = true)
            {
                return base.Layout(cached, widths, rectOffset, offset, size, collapsable);
            }
        }
    }
}
