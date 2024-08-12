using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public abstract class EleBaseRect : Ele
        {
            protected string name;
            protected EleBaseSizer sizer = null;
            public EleBaseSizer Sizer { get{return this.sizer; } }

            EleBaseRect parent;
            EleBaseRect Parent { get { return this.parent; } }

            List<EleBaseRect> children = null;

            public static void SetRTTopLeft(RectTransform rt)
            {
                rt.anchorMin = new Vector2(0.0f, 1.0f);
                rt.anchorMax = new Vector2(0.0f, 1.0f);
                rt.pivot = new Vector2(0.0f, 1.0f);
                rt.offsetMin = new Vector2(0.0f, 0.0f);
                rt.offsetMax = new Vector2(0.0f, 0.0f);
            }

            public EleBaseRect(EleBaseRect parent, Vector2 size, string name = "")
                : base(size, name)
            {
                this.parent = parent;

                if (parent != null)
                    parent.AddChild(this);
            }

            public abstract RectTransform RT {get; }

            public override bool Active 
            { 
                get
                { 
                    RectTransform rt = this.RT;
                    if(rt == null)
                        return false;

                    return rt.gameObject.activeSelf;
                }
            }

            public EleBaseRect(EleBaseRect parent)
                : base()
            {
                this.parent = parent;

                if (this.parent != null)
                    this.parent.AddChild(this);

                this.name = "";
            }

            public bool HasChildren()
            {
                return
                    this.children != null &&
                    this.children.Count > 0;
            }

            public virtual bool CanHaveChildren()
            {
                return true;
            }

            public virtual bool AddChild(EleBaseRect ele)
            {
                if (this.CanHaveChildren() == false)
                    return false;

                if (this.GetContentRect() == null)
                    return false;

                if (this.children == null)
                    this.children = new List<EleBaseRect>();

                this.children.Add(ele);
                return true;
            }

            public void SetSizer(EleBaseSizer sz)
            { 
                this.sizer = sz;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            { 
                float min = this.minSize.x;

                if(this.sizer != null)
                { 
                    float szmin = this.sizer.GetMinWidth(cache);
                    min = Mathf.Max(min, szmin);
                }
                return min;
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths, 
                float width,
                bool collapsable = true)
            {
                Vector2 min = this.minSize;

                if(this.sizer != null)
                {
                    Vector2 szmin = this.sizer.GetMinSize(cache, widths, width, collapsable);
                    min = Vector2.Max(min, szmin);
                }
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

                RectTransform rt = this.RT;
                if(rt == null)
                    return size;

                Vector2 ret = size;
                if(this.sizer != null)
                { 
                    Vector2 szRet = this.sizer.Layout(cached, widths, Vector2.zero, offset, size, collapsable);
                    ret = Vector2.Max(ret, szRet);
                }

                rt.anchoredPosition = new Vector2(rectOffset.x, -rectOffset.y);
                rt.sizeDelta = ret;

                return ret;
            }

            public virtual void Deconstruct()
            {
                this.ClearChildren();
            }

            public void ClearChildren()
            {
                if (this.children != null)
                {
                    foreach (EleBaseRect e in this.children)
                        e.Deconstruct();
                }

                this.children.Clear();
            }

            public virtual RectTransform GetParentRect()
            {
                if (this.parent == null)
                    return null;

                return this.parent.GetContentRect();
            }

            public virtual RectTransform GetContentRect()
            {
                RectTransform rt = this.RT;
                if(rt != null)
                    return rt;

                return this.GetParentRect();
            }
        }
    }
}
