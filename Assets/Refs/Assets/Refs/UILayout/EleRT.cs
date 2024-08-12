using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIL
    { 
        public class EleRT : Ele
        { 
            RectTransform rt;

            public EleRT(RectTransform rt, Vector2 minsize, string name = "")
                : base(minsize, name)
            { 
                this.rt = rt;
            }

            public override bool Active => this.rt.gameObject.activeSelf;

            public override Vector2 Layout(Dictionary<Ele, Vector2> cached, Dictionary<Ele, float> widths, Vector2 rectOffset, Vector2 offset, Vector2 size, bool collapsable = true)
            {
                this.rt.anchoredPosition = new Vector2(rectOffset.x, -rectOffset.y);
                this.rt.sizeDelta = size;
                return size;
            }
            protected override Vector2 ImplCalcMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true)
            {
                return this.minSize;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                return this.minSize.x;
            }
        }
    }
}