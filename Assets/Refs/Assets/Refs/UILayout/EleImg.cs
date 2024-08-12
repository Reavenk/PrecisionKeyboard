using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIL
    { 
        public class EleImg : EleBaseRect
        { 
            UnityEngine.UI.Image img;

            public UnityEngine.UI.Image Img {get{return this.img; } }

            public EleImg(EleBaseRect parent, Sprite sprite, Vector2 size, string name = "")
                : base(parent, size, name)
            { 
                this._Create(parent, sprite, size, name);
            }

            public EleImg(EleBaseRect parent, Sprite sprite)
                : base(parent)
            { 
                this._Create(parent, sprite, new Vector2(-1.0f, -1.0f), "");
            }

            protected void _Create(EleBaseRect parent, Sprite sprite, Vector2 size, string name = "")
            { 
                GameObject go = new GameObject("Image_" + name);
                go.transform.SetParent(parent.GetContentRect(), false);

                this.img = go.AddComponent<UnityEngine.UI.Image>();
                this.img.rectTransform.RTQ().TopLeftAnchorsPivot().ZeroOffsets();

                this.img.sprite = sprite;
                this.img.type = UnityEngine.UI.Image.Type.Simple;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                float f = base.ImplCalcMinSizeWidth(cache);

                Sprite sprite = this.img.sprite;
                if (sprite != null)
                    f = Mathf.Max(f, sprite.rect.width, this.minSize.x);

                return f;
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths,
                float width,
                bool collapsable = true)
            {
                Vector2 mindef = base.ImplCalcMinSize(cache, widths, width, collapsable);
                Vector2 min = this.minSize;

                Sprite sprite = this.img.sprite;
                if (sprite != null)
                { 
                    Vector2 spriteMin = sprite.rect.size;

                    min.x = Mathf.Max(min.x, spriteMin.x);
                    min.y = Mathf.Max(min.y, spriteMin.y);
                }

                min.x = Mathf.Max(mindef.x, min.x);
                min.y = Mathf.Max(mindef.y, min.y);

                return min;
            }

            public override RectTransform RT => this.img.rectTransform;

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached, 
                Dictionary<Ele, float> widths, 
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable = true)
            { 
                return base.Layout(cached, widths, rectOffset, offset, size, collapsable);
            }
        }
    }
}