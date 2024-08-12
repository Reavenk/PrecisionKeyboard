using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class EleGridSizer : EleBaseSizer
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

            List<PairedLayoutData> entries = 
                new List<PairedLayoutData>();

            public Vector2 padding = Vector2.zero;
            public PadRect border = new PadRect();

            HashSet<int> flexColumns = new HashSet<int>();
            HashSet<int> flexRows = new HashSet<int>();

            public int columns = 1;

            public EleGridSizer(EleBaseRect parent, int col, string name)
                : base(parent, name)
            {
                this.columns = col;
            }

            public EleGridSizer(EleBaseSizer parent, int col, float proportion, LFlag flags, string name = "")
                : base(parent, proportion, flags, name)
            { 
                this.columns = col;
            }

            public EleGridSizer(int col, string name = "")
                : base(name)
            { 
                this.columns = col;
            }

            public override void Add(Ele ele, float proportion, LFlag flags)
            {
                this.entries.Add(new PairedLayoutData(ele, proportion, flags));
            }

            public override bool Remove(Ele child)
            {
                for(int i = 0; i< this.entries.Count; ++i)
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
                for(int i = 0; i< this.entries.Count; ++i)
                { 
                    if(this.entries[i].ele == child)
                        return true;
                }
                return false;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                float maxwidth =  0.0f;
                int cols = Mathf.Max(this.columns, 1);

                int i = 0;
                while(i < this.entries.Count)
                { 
                    float fx = 0.0f;
                    bool atLeastOneX = false;
                    for(int j = 0; j < cols; ++j)
                    { 
                        if(atLeastOneX == false)
                            atLeastOneX = true;
                        else
                            fx += this.padding.x;

                        fx += this.entries[i].ele.GetMinWidth(cache);

                        ++i;
                        if(i >= this.entries.Count)
                            break;
                    }

                    maxwidth = Mathf.Max(fx, maxwidth);
                }
                return maxwidth + this.border.width;
            }

            protected override Vector2 ImplCalcMinSize(Dictionary<Ele, Vector2> cache, Dictionary<Ele, float> widths, float width, bool collapsable = true)
            {
                if(this.entries.Count == 0)
                    return new Vector2(this.border.width, this.border.height);

                int cols = Mathf.Max(this.columns, 1);
                int rows = Mathf.CeilToInt(this.entries.Count / (float)cols);

                List<float> minWidths = new List<float>();
                List<float> minHeights = new List<float>();
                for(int i = 0; i < cols; ++i)
                    minWidths.Add(0.0f);

                for(int i = 0; i < rows; ++i)
                    minHeights.Add(0.0f);


                for(int i = 0; i < this.entries.Count; ++i)
                { 
                    int scol = i % cols;
                    int srow = i / cols;

                    Vector2 mw = this.entries[i].ele.GetMinSize(cache, widths, width, collapsable);

                    minWidths[scol] = Mathf.Max(minWidths[scol], mw.x);
                    minHeights[srow] = Mathf.Max(minHeights[srow], mw.y);
                }

                float finalWidth = 0.0f;
                foreach(float f in minWidths)
                    finalWidth += f;

                finalWidth += this.padding.x * (cols - 1);

                float finalHeight = 0.0f;
                foreach(float f in minHeights)
                    finalHeight += f;

                finalHeight += this.padding.y * (rows - 1);

                return 
                    new Vector2(
                        finalWidth + this.border.width,
                        finalHeight + this.border.height);
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset,
                Vector2 offset,
                Vector2 size,
                bool collapsable = true)
            {
                if (this.entries.Count == 0)
                    return new Vector2(this.border.width, this.border.height);

                int cols = Mathf.Max(this.columns, 1);
                int rows = Mathf.CeilToInt(this.entries.Count / (float)cols);

                List<float> minWidths = new List<float>();
                List<float> minHeights = new List<float>();

                for (int i = 0; i < cols; ++i)
                    minWidths.Add(0.0f);

                for(int i = 0; i < rows; ++i)
                    minHeights.Add(0.0f);

                // Collect dimensions
                float allocWidth = rectOffset.x - this.border.width;
                for (int i = 0; i < this.entries.Count; ++i)
                {
                    int scol = i % cols;
                    int srow = i / cols;

                    Vector2 mw = this.entries[i].ele.GetMinSize(cached, widths, rectOffset.x, collapsable);

                    minWidths[scol] = Mathf.Max(minWidths[scol], mw.x);
                    minHeights[srow] = Mathf.Max(minHeights[srow], mw.y);
                }

                // The total min width taken
                float finalWidth = 0.0f;
                foreach (float f in minWidths)
                    finalWidth += f;

                finalWidth += this.padding.x * (cols - 1);

                // The total min height taken
                float finalHeight = 0.0f;
                foreach (float f in minHeights)
                    finalHeight += f;

                finalHeight += this.padding.y * (rows - 1);

                // Extra flex space
                float extraWidth = Mathf.Max(0.0f, size.x - finalWidth);
                float extraHeight = Mathf.Max(0.0f, size.y - finalHeight);

                // Get rid of false flexes
                List<int> useFlexCols = new List<int>(this.flexColumns);
                List<int> useFlexRows = new List<int>(this.flexRows);

                for(int i = 0; i < useFlexCols.Count;)
                { 
                    if(useFlexCols[i] >= cols)
                        useFlexCols.RemoveAt(i);
                    else
                        ++i;
                }

                for(int i = 0; i < useFlexRows.Count; ++i)
                { 
                    if(useFlexRows[i] >= rows)
                        useFlexRows.RemoveAt(i);
                    else
                        ++i;
                }

                float flexWidth = 0.0f;
                if(useFlexCols.Count > 0)
                    flexWidth = extraWidth / (float)useFlexCols.Count;

                float flexHeight = 0.0f;
                if(useFlexRows.Count > 0)
                    flexHeight = extraHeight / (float)useFlexRows.Count;

                // Adjust cell borders
                foreach(int i in this.flexColumns)
                    minWidths[i] += flexWidth;

                foreach(int i in this.flexRows)
                    minHeights[i] += flexHeight;

                // float layout grid
                float maxX = 0.0f;
                float fy = this.border.left;
                int id = 0;
                bool atLeastOneY = false;
                for(int y = 0; y < rows; ++y)
                { 
                    if (id >= this.entries.Count)
                        break;

                    float ht = minHeights[y];

                    if(atLeastOneY == false)
                        atLeastOneY = true;
                    else
                        fy += this.padding.y;

                    float fx = this.border.top;
                    bool atLeastOneX = false;
                    for (int x = 0; x < cols; ++x)
                    {
                        if (id >= this.entries.Count)
                            break;

                        float w = minWidths[x];

                        if(atLeastOneX == false)
                            atLeastOneX = true;
                        else
                            fx += this.padding.x;

                        Vector2 pos = new Vector2(fx, fy);
                        Vector2 useSz = this.entries[id].ele.GetMinSize(cached, widths, minWidths[id % cols], collapsable);
                        Vector2 maxSz = new Vector2(w, ht);

                        LFlag finalFlag = GetFinalFlags(this.entries[id].style, collapsable);
                        DoRectProcessing(ref pos, ref useSz, maxSz, finalFlag);

                        this.entries[id].ele.Layout(
                            cached, 
                            widths, 
                            pos, 
                            rectOffset + pos, 
                            useSz, 
                            collapsable);

                        fx += w;
                        ++id;
                    }
                    fx += this.border.right;
                    maxX = Mathf.Max(fx, maxX);

                    fy += ht;
                }
                fy += this.border.bot;

                return new Vector2(maxX, fy);
            }
        }
    }
}