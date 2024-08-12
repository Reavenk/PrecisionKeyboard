using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIL
    { 
        [System.Flags]
        public enum LFlag
        { 
            GrowHoriz           = 1 << 0,
            GrowVert            = 1 << 2,
            Grow                = GrowHoriz | GrowVert,
            AlignLeft           = 1 << 3,
            AlignRight          = 1 << 4,
            AlignTop            = 1 << 5,
            AlignBot            = 1 << 6,
            AlignVertCenter     = 1 << 7,
            AlignHorizCenter    = 1 << 8,
            AlignCenter         = AlignVertCenter | AlignHorizCenter,
            GrowHorizOnCollapse = 1 << 9,
            GrowVertOnCollapse  = 1 << 10,
            GrowOnCollapse      = GrowHorizOnCollapse | GrowVertOnCollapse

        }

        public enum Direction
        { 
            Horiz,
            Vert
        }

        public abstract class Ele
        { 
            public Vector2 minSize = new Vector2(-1.0f, -1.0f);

            string name = "";
            public string Name {get{return this.name; } }

            public abstract bool Active {get; }

            public Ele(Vector2 size, string name)
            { 
                this.name = name;
                this.minSize = size;
            }

            public Ele(string name = "")
            { 
                this.name = name;
            }

            public static LFlag GetFinalFlags(LFlag orig, bool collapsed)
            { 
                if(collapsed == true)
                { 
                    if((orig & LFlag.GrowHorizOnCollapse) != 0)
                        orig |= LFlag.GrowHoriz;

                    if((orig & LFlag.GrowVertOnCollapse) != 0)
                        orig |= LFlag.GrowVert;
                }

                return orig;
            }

            public Vector2 GetMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true)
            { 
                Vector2 v2;
                if(cache.TryGetValue(this, out v2) == true)
                    return v2;

                v2 = this.CalcMinSize(cache, widths, width, collapsable);
                cache.Add(this, v2);
                return v2;
            }

            private Vector2 CalcMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true)
            { 
                Vector2 ret = ImplCalcMinSize(cache, widths, width, collapsable);

                ret = Vector2.Max(ret, this.minSize);
                return ret;
            }

            public float GetMinWidth(Dictionary<Ele, float> cache)
            { 
                float f;
                if(cache.TryGetValue(this, out f) == true)
                    return f;

                f = this.CalcMinWidth(cache);
                cache.Add(this, f);
                return f;
            }

            private float CalcMinWidth(Dictionary<Ele, float> cache)
            { 
                float ret = ImplCalcMinSizeWidth(cache);

                ret = Mathf.Max(ret, this.minSize.x);
                return ret;
            }

            protected abstract float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache);
            protected abstract Vector2 ImplCalcMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true);

            public abstract Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset, Vector2 offset, Vector2 size,
                bool collapsable = true);
        }
    }
}
