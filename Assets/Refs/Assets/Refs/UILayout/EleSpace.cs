using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIL
    { 
        public class EleSpace : Ele
        { 
            Vector2 space;
            bool active;

            public EleSpace(float space)
                : base()
            { 
                this.space = new Vector2(space, space);
            }

            public override bool Active => this.active;

            public EleSpace(Vector2 size, string name = "")
                : base()
            { 
                this.space = size;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            { 
                return this.space.x;
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths,
                float width,
                bool collapsable = true)
            {
                return this.space;
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths, 
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable = true)
            {
                return size;
            }
        }
    }
}