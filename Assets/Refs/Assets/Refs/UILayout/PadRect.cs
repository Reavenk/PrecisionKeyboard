using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        [System.Serializable]
        public struct PadRect
        {
            public float left;
            public float top;
            public float right;
            public float bot;

            public float width {get{return this.left + this.right; } }
            public float height {get{return this.top + this.bot; } }

            public Vector2 dim {get{return new Vector2(this.width, this.height); } }

            public PadRect(float left, float top, float right, float bot)
            {
                this.left   = left;
                this.top    = top;
                this.right  = right;
                this.bot    = bot;
            }

            public PadRect(float all)
            { 
                this.left   = all;
                this.top    = all;
                this.right  = all;
                this.bot    = all;
            }

            public void Set(float all)
            {
                this.left   = all;
                this.top    = all;
                this.right  = all;
                this.bot    = all;
            }

            public void Set(float left, float top, float right, float bot)
            {
                this.left   = left;
                this.top    = top;
                this.right  = right;
                this.bot    = bot;
            }

            public Vector2 Pad(Vector2 v)
            { 
                return v + 
                    new Vector2(
                        this.left + this.right, 
                        this.top + this.bot);
            }
        }
    }
}