using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class EleBoxSizer : EleBaseSizer
        {
            public struct PairedLayoutData
            {
                public Ele ele;
                public float prop;
                public LFlag style;

                public PairedLayoutData(Ele ele, float p, LFlag s)
                { 
                    this.ele = ele;
                    this.prop = p;
                    this.style = s;
                }
            }

            Direction direction = Direction.Vert;
            List<PairedLayoutData> entries = new List<PairedLayoutData>();

            public float padding = 0.0f;

            public PadRect border = new PadRect();

            public EleBoxSizer(EleBaseRect parent, Direction direction, string name)
                : base(parent,name)
            {
                this.direction = direction;
            }

            public EleBoxSizer(EleBaseSizer parent, Direction direction, float proportion, LFlag flags, string name = "")
                : base(parent, proportion, flags, name)
            {
                this.direction = direction;
            }

            public EleBoxSizer(Direction direction, string name = "")
                : base(name)
            { 
                this.direction = direction;
            }

            public override void Add(Ele ele, float proportion, LFlag flags)
            { 
                this.entries.Add(
                    new PairedLayoutData(ele, proportion, flags));
            }

            public override bool Remove(Ele child)
            {
                for(int i = 0; i < this.entries.Count; ++i)
                { 
                    if(this.entries[i].ele == child)
                    { 
                        this.entries.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }

            public override bool HasEntry(Ele child)
            { 
                for(int i = 0; i < this.entries.Count; ++i)
                { 
                    if(this.entries[i].ele == child)
                        return true;
                }
                return false;
            }

            protected override float ImplCalcMinSizeWidth(
                Dictionary<Ele, float> cached)
            { 
                if(this.direction == Direction.Horiz)
                    return this.ImplCalcMinSize_HorizWidth(cached);

                return this.ImplCalcMinSize_VertWidth(cached);
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths,
                float width,
                bool collapsable = true)
            {
                if (this.direction == Direction.Horiz)
                    return this.ImplCalcMinSize_Horiz(cache, widths, width, collapsable);

                return this.ImplCalcMinSize_Vert(cache, widths, width, collapsable);
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable = true)
            { 
                if(this.direction == Direction.Horiz)
                    return this.Layout_Horiz(cached, widths, rectOffset, offset, size, collapsable);

                return this.Layout_Vert(cached, widths, rectOffset, offset, size, collapsable);
            }

            float ImplCalcMinSize_VertWidth(Dictionary<Ele, float> cache)
            {
                float xMax = 0.0f;
                bool atLeastOne = false;
                foreach (PairedLayoutData pld in this.entries)
                {
                    atLeastOne = true;
                    float w = pld.ele.GetMinWidth(cache);
                    xMax = Mathf.Max(xMax, w);
                }

                if(atLeastOne == true)
                    xMax += this.border.width;

                return xMax;
            }

            Vector2 ImplCalcMinSize_Vert(
                Dictionary<Ele, Vector2> cache, 
                Dictionary<Ele, float> widths, 
                float width, 
                bool collapsable)
            { 
                Vector2 ret = Vector2.zero;
                width -= this.border.width;
                bool atleastOne = false;
                foreach(PairedLayoutData pld in this.entries)
                { 
                    if(atleastOne == false)
                        ret.y += this.padding;

                    Vector2 sz;
                    LFlag pldFlag = GetFinalFlags(pld.style, collapsable);
                    if ((pldFlag & LFlag.GrowHoriz) != 0)
                        sz = pld.ele.GetMinSize(cache, widths, width, collapsable);
                    else
                        sz = pld.ele.GetMinSize(cache, widths, pld.ele.GetMinWidth(widths), collapsable);
                    
                    ret.x = Mathf.Max(sz.x, ret.x);
                    ret.y += sz.y;
                }
                ret.y += this.border.height;
                ret.x += this.border.width;
                return ret;
            }

            Vector2 Layout_Vert(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable)
            {

                size.x -= this.border.width;
                size.y -= this.border.height;
                float maxX = 0.0f;
                List<PairedLayoutData> proportions = new List<PairedLayoutData>();

                float totalMinY = 0.0f;
                float totalProp = 0.0f;

                bool atleastone = false;
                foreach (PairedLayoutData pld in this.entries)
                {
                    totalProp += pld.prop;

                    if(atleastone == false)
                        atleastone = true;
                    else
                        totalMinY += this.padding;

                    Vector2 v2 = pld.ele.GetMinSize(cached, widths, size.x, collapsable);
                    maxX = Mathf.Max(maxX, size.x);
                    totalMinY += v2.y;

                }
                maxX = Mathf.Max(maxX, size.x - this.border.width);
                // The free space to be distributed amongst the proportions
                float freeSpace = Mathf.Max(0.0f, size.y - totalMinY);

                atleastone = false;
                float fx = this.border.left;
                float fy = this.border.top;
                foreach (PairedLayoutData pld in this.entries)
                {
                    if (atleastone == false)
                        atleastone = true;
                    else
                        fy += this.padding;

                    Vector2 m = pld.ele.GetMinSize(cached, widths, size.x, collapsable);
                    float eleHeight = m.y;

                    if(pld.prop != 0.0f)
                        eleHeight += pld.prop / totalProp * freeSpace;
                    else if(totalProp == 0.0f)
                        eleHeight += freeSpace / (float)this.entries.Count;

                    float feleX = fx;
                    float feleY = fy;
                    float feleSzX = m.x;
                    float feleSzY = m.y;

                    LFlag pldFlag = GetFinalFlags(pld.style, collapsable);

                    if ((pldFlag & LFlag.GrowHoriz) != 0)
                        feleSzX = maxX;
                    else if((pldFlag & LFlag.AlignHorizCenter) != 0)
                        feleX += (maxX - m.x) * 0.5f;
                    else if((pldFlag & LFlag.AlignRight) != 0)
                        feleX += maxX - m.x;

                    if((pldFlag & LFlag.GrowVert) != 0)
                        feleSzY = eleHeight;
                    else if((pldFlag & LFlag.AlignVertCenter) != 0)
                        feleY += (eleHeight - m.y) * 0.5f;
                    else if((pldFlag & LFlag.AlignBot) != 0)
                        feleY += eleHeight - m.y;

                    Vector2 losz = 
                        pld.ele.Layout(
                            cached,
                            widths,
                            rectOffset + new Vector2(feleX, feleY), 
                            offset + new Vector2(feleX, feleY), 
                            new Vector2(feleSzX, feleSzY),
                            collapsable);

                    maxX = Mathf.Max(maxX, losz.x);

                    fy += eleHeight;
                }

                maxX += this.border.width;
                fy += this.border.bot;

                return new Vector2(maxX, fy);
            }

            float ImplCalcMinSize_HorizWidth(Dictionary<Ele, float> cache)
            {
                float fx = 0.0f;
                bool atLeastOne = false;
                foreach (PairedLayoutData pld in this.entries)
                {
                    if(atLeastOne == false)
                        atLeastOne = true;
                    else
                        fx += this.padding;

                    float w = pld.ele.GetMinWidth(cache);
                    fx += Mathf.Max(w);
                }

                if (atLeastOne == true)
                    fx += this.border.width;

                return fx;
            }

            Vector2 ImplCalcMinSize_Horiz(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable)
            {
                float xBuild = 0.0f;        // The total width of everything before allocating proportions
                float yMax = 0.0f;          // The total height calculated.

                float usableWidth = width - this.border.width;
                float totalProportions = 0.0f;

                bool atleastone = false;
                // First pass, accumulate xBuild
                foreach (PairedLayoutData pld in this.entries)
                {
                    if(atleastone == false)
                        atleastone = true;
                    else
                        xBuild += this.padding;

                    xBuild += pld.ele.GetMinWidth(widths);
                    totalProportions += pld.prop;
                }

                float distrSpace = Mathf.Max(0.0f, usableWidth - xBuild);

                bool atLeastOne = false;
                float accumW = 0.0f;
                foreach(PairedLayoutData pld in this.entries)
                { 
                    if(atLeastOne == false)
                        atLeastOne = true;
                    else
                        accumW += this.padding;

                    float fx = pld.ele.GetMinWidth(widths);
                    if(totalProportions == 0.0f)
                        fx += distrSpace / (float)this.entries.Count;
                    else
                        fx += pld.prop / totalProportions * distrSpace;

                    Vector2 sz = pld.ele.GetMinSize(cache, widths, fx, collapsable);
                    yMax = Mathf.Max(yMax, sz.y);
                    accumW += sz.x;
                }

                
                yMax += this.border.height;
                accumW += this.border.width;

                return new Vector2(accumW, yMax);
            }

            Vector2 Layout_Horiz(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable)
            {
                // TODO: The entire implementation!
                size.x -= this.border.width;
                size.y -= this.border.height;
                float maxY = 0.0f;
                List<PairedLayoutData> proportions = new List<PairedLayoutData>();

                float totalMinX = 0.0f;
                float totalProp = 0.0f;

                bool atleastone = false;
                foreach (PairedLayoutData pld in this.entries)
                {
                    totalProp += pld.prop;

                    if (atleastone == false)
                        atleastone = true;
                    else
                        totalMinX += this.padding;

                    Vector2 v2 = pld.ele.GetMinSize(cached, widths, size.x, collapsable);
                    maxY = Mathf.Max(maxY, size.y);
                    totalMinX += v2.x;

                }
                maxY = Mathf.Max(maxY, size.y);
                // The free space to be distributed amongst the proportions
                float freeSpace = Mathf.Max(0.0f, size.x - totalMinX);

                atleastone = false;
                float fx = this.border.left;
                float fy = this.border.top;
                foreach (PairedLayoutData pld in this.entries)
                {
                    if (atleastone == false)
                        atleastone = true;
                    else
                        fx += this.padding;

                    Vector2 m = pld.ele.GetMinSize(cached, widths, size.x, collapsable);
                    float eleWidth = m.x;

                    if (pld.prop != 0.0f)
                        eleWidth += pld.prop / totalProp * freeSpace;
                    else if (totalProp == 0.0f)
                        eleWidth += freeSpace / (float)this.entries.Count;

                    float feleX = fx;
                    float feleY = fy;
                    float feleSzX = m.x;
                    float feleSzY = m.y;

                    LFlag pldFlag = GetFinalFlags(pld.style, collapsable);

                    if ((pldFlag & LFlag.GrowHoriz) != 0)
                        feleSzX = eleWidth;
                    else if ((pldFlag & LFlag.AlignHorizCenter) != 0)
                        feleX += (eleWidth - m.x) * 0.5f;
                    else if ((pldFlag & LFlag.AlignRight) != 0)
                        feleX += eleWidth - m.x;

                    if ((pldFlag & LFlag.GrowVert) != 0)
                        feleSzY = maxY;
                    else if ((pldFlag & LFlag.AlignVertCenter) != 0)
                        feleY += (maxY - m.y) * 0.5f;
                    else if ((pldFlag & LFlag.AlignBot) != 0)
                        feleY += maxY - m.y;

                    pld.ele.Layout(
                        cached,
                        widths,
                        rectOffset + new Vector2(feleX, feleY),
                        offset + new Vector2(feleX, feleY),
                        new Vector2(feleSzX, feleSzY),
                        collapsable);

                    fx += eleWidth;
                }

                fy += maxY;

                fx += this.border.right;
                fy += this.border.bot;

                return new Vector2(fx, fy);
            }
        }
    }
}