using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class ElePropGrid : EleBaseSizer
        {
            public struct PairedLayoutData
            {
                public Ele ele;
                public float prop;
                public LFlag style;

                public string label;
                public UnityEngine.UI.Text labelText;
            }

            EleBaseRect parent;
            public EleBaseRect Parent {get{return this.parent; } }

            List<PairedLayoutData> layoutData = new List<PairedLayoutData>();
            public PadRect border = new PadRect(5.0f, 5.0f, 5.0f, 5.0f);
            public float padding = 5.0f;
            public float splitterWidth = 10.0f;

            float cachedLabelWidth = 0.0f;

            public TextAttrib labelText;

            public float labelPushdown = 20.0f;


            public ElePropGrid(EleBaseRect parent, TextAttrib labelText)
            { 
                this.parent = parent;
                this.labelText = labelText.Clone();
            }

            public ElePropGrid(EleBaseRect parent, TextAttrib labelText, int labelSize)
            {
                this.parent = parent;
                this.labelText = labelText.Clone();

                this.labelText.fontSize = labelSize;
            }

            public override void Add(Ele child, float proportion, LFlag flags)
            { 
                this.Add(child, proportion, flags, "");
            }

            public void Add(Ele child, float proportion, LFlag flags, string label)
            {
                PairedLayoutData pld = new PairedLayoutData();

                pld.ele = child;
                pld.prop = proportion;
                pld.style = flags;
                pld.label = label;

                if(string.IsNullOrEmpty(label) == false)
                { 
                    RTQuick.CreateGameObjectWithText(
                        parent.GetContentRect(), 
                        "Label_" + label, 
                        out pld.labelText).TopLeftAnchorsPivot();

                    this.labelText.Apply(pld.labelText);
                    pld.labelText.text = label;
                    pld.labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    pld.labelText.verticalOverflow = VerticalWrapMode.Overflow;
                }

                layoutData.Add(pld);
            }

            public override bool Remove(Ele child)
            {
                for(int i = 0; i < this.layoutData.Count; ++i)
                { 
                    if(this.layoutData[i].ele == child)
                    { 
                        UnityEngine.UI.Text labelToDel = this.layoutData[i].labelText;

                        this.layoutData.RemoveAt(i);

                        if(labelToDel != null)
                            GameObject.Destroy(labelToDel.gameObject);

                        return true;
                    }
                }
                return false;
            }

            public override bool HasEntry(Ele child)
            { 
                for(int i = 0; i < this.layoutData.Count; ++i)
                { 
                    if(this.layoutData[i].ele == child)
                        return true;
                }
                return false;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                float maxLabels = 0.0f;
                float maxContent = 0.0f;

                foreach(PairedLayoutData pld in this.layoutData)
                { 
                    if(pld.labelText == null)
                        continue;

                    TextGenerationSettings tgs = 
                        pld.labelText.GetGenerationSettings(
                            new Vector2(float.PositiveInfinity, float.PositiveInfinity));

                    tgs.scaleFactor = 1.0f;

                    TextGenerator tg = pld.labelText.cachedTextGenerator;

                    float labelWidth = tg.GetPreferredWidth(pld.labelText.text, tgs);
                    maxLabels = Mathf.Max(labelWidth, maxLabels);
                }

                this.cachedLabelWidth = maxLabels;

                foreach (PairedLayoutData pld in this.layoutData)
                { 
                    float width = pld.ele.GetMinWidth(cache);
                    maxContent = Mathf.Max(width, maxContent);
                }

                float ret = maxLabels + maxContent;
                if(maxLabels != 0.0f && maxContent != 0.0f)
                    ret += this.splitterWidth;

                ret += this.border.width;
                return ret;
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths, 
                float width,
                bool collapsable = true)
            { 
                float usableWidth = width;
                usableWidth -= this.border.width;

                if(this.cachedLabelWidth != 0.0f)
                    this.cachedLabelWidth += this.splitterWidth;

                float contentWidth = usableWidth - this.cachedLabelWidth - this.border.width;

                float fY = this.border.top;
                bool atLeastOne = false;
                foreach(PairedLayoutData pld in this.layoutData)
                { 
                    if(atLeastOne == false)
                        atLeastOne = true;
                    else
                        fY += this.padding;

                    float maxY = 0.0f;
                    if(pld.labelText != null)
                    { 
                        TextGenerationSettings tgs = 
                            pld.labelText.GetGenerationSettings(
                                new Vector2(
                                    float.PositiveInfinity, 
                                    float.PositiveInfinity));

                        TextGenerator tg = pld.labelText.cachedTextGenerator;

                        maxY = tg.GetPreferredHeight(pld.labelText.text, tgs);
                    }

                    Vector2 v2 = pld.ele.GetMinSize(cache, widths, contentWidth);
                    maxY = Mathf.Max(v2.y, maxY);

                    fY += maxY;
                }

                Vector2 ret = new Vector2(contentWidth, fY);
                ret = this.border.Pad(ret);

                return ret;
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths, 
                Vector2 rectOffset, 
                Vector2 offset, 
                Vector2 size,
                bool collapsable = true)
            {
                float usableWidth = size.x - this.border.width;

                float fxConStart = this.cachedLabelWidth;

                float contentWidth = usableWidth - this.cachedLabelWidth - this.splitterWidth;
                if (this.cachedLabelWidth != 0.0f)
                {
                    this.cachedLabelWidth -= this.splitterWidth;
                    fxConStart += this.splitterWidth;
                }

                float totalProp = 0.0f;
                float totalYExcess = 0.0f;

                bool atLeastOne = false;
                float fyCtr = border.height;
                foreach (PairedLayoutData pld in this.layoutData)
                {
                    if (atLeastOne == false)
                        atLeastOne = true;
                    else
                        fyCtr += this.padding;

                    totalProp += pld.prop;

                    Vector2 v2 = pld.ele.GetMinSize(cached, widths, contentWidth);
                    fyCtr += v2.y;
                }
                totalYExcess = Mathf.Max(0.0f, size.y - fyCtr);

                float fY = this.border.top;
                atLeastOne = false;
                foreach (PairedLayoutData pld in this.layoutData)
                {
                    if (atLeastOne == false)
                        atLeastOne = true;
                    else
                        fY += this.padding;

                    float maxY = 0.0f;
                    Vector2 v2 = pld.ele.GetMinSize(cached, widths, contentWidth);
                    maxY = v2.y;
                    if(pld.labelText != null)
                    {
                        TextGenerationSettings tgs =
                            pld.labelText.GetGenerationSettings(
                                new Vector2(
                                    float.PositiveInfinity,
                                    float.PositiveInfinity));

                        TextGenerator tg = pld.labelText.cachedTextGenerator;

                        float labelHeight = tg.GetPreferredHeight(pld.labelText.text, tgs);
                        maxY = Mathf.Max(labelHeight, maxY);

                        pld.labelText.alignment = TextAnchor.UpperRight;
                        pld.labelText.rectTransform.sizeDelta = new Vector2(this.cachedLabelWidth, labelHeight + 1.0f);
                        pld.labelText.rectTransform.anchoredPosition = 
                            new Vector2(
                                rectOffset.x + this.border.left,
                                -rectOffset.y - fY - this.labelPushdown);
                    }

                    float usableY = v2.y;
                    if(totalProp == 0.0f)
                        usableWidth += totalYExcess / (float)this.layoutData.Count;
                    else
                        usableY += pld.prop / totalProp * totalYExcess;

                    float fposx = fxConStart;
                    float fposy = fY;
                    float fwidth = v2.x;
                    float fheight = v2.y;

                    if((pld.style & LFlag.GrowHoriz) != 0)
                        fwidth = contentWidth;
                    else if((pld.style & LFlag.AlignHorizCenter) != 0)
                        fposx += (contentWidth - fwidth) * 0.5f;
                    else if((pld.style & LFlag.AlignRight) != 0)
                        fposx += (contentWidth - fwidth);

                    if((pld.style & LFlag.GrowVert) != 0)
                        fheight = usableY;
                    else if((pld.style & LFlag.AlignVertCenter) != 0)
                        fposy += (usableY - fheight) * 0.5f;
                    else if((pld.style & LFlag.AlignBot) != 0)
                        fposy += (usableY + fheight);

                    Vector2 of = new Vector2(fposx, fposy);
                    Vector2 sz = new Vector2(fwidth, fheight);
                    pld.ele.Layout(cached, widths, rectOffset + of, offset + of, sz);

                    fY += maxY;
                }

                Vector2 ret = new Vector2(size.x, fY);
                if (atLeastOne == true)
                    ret.y += this.border.bot;

                return ret;
            }
        }
    }
}
