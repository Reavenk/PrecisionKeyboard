using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class EleVertScrollRgn : EleGenVertScrollRgn<UnityEngine.UI.ScrollRect, UnityEngine.UI.Scrollbar>
        { 
            public EleVertScrollRgn(EleBaseRect parent, ScrollInfo horiz, ScrollInfo vert, bool showBack, Vector2 size, float sensitivity, string name = "")
                : base(parent, horiz, vert, showBack, size, sensitivity, name)
            { 
                this._Create(parent, horiz, vert, showBack, size, sensitivity, name);
            }

            public EleVertScrollRgn(EleBaseRect parent, ScrollInfo horiz, ScrollInfo vert, bool showBack, float sensitivity, string name = "")
                : base(parent, horiz, vert, showBack, sensitivity, name)
            { 
                this._Create(parent, horiz, vert, showBack, new Vector2(-1.0f, -1.0f), sensitivity, name);
            }
        }

        public class EleGenVertScrollRgn<RectTy, ScrollTy> : EleBaseRect
            where RectTy : UnityEngine.UI.ScrollRect
            where ScrollTy : UnityEngine.UI.Scrollbar
        {
            ScrollTy vertScroll;
            ScrollTy horizScroll;
            RectTy scrollRect;
            UnityEngine.UI.Mask viewportMask;

            RectTransform rt;

            public UnityEngine.UI.Scrollbar VertScroll {get{return this.vertScroll; } }
            public UnityEngine.UI.Scrollbar HorizScroll {get{return this.horizScroll; } }
            public UnityEngine.UI.ScrollRect ScrollRect {get{return this.scrollRect; } }
            public UnityEngine.UI.Mask ViewportMask {get{return this.viewportMask; } }


            public override RectTransform GetContentRect()
            { 
                return this.scrollRect.content;
            }

            public override RectTransform RT => this.rt;

            public EleGenVertScrollRgn(EleBaseRect parent, ScrollInfo horiz, ScrollInfo vert, bool showBack, Vector2 size, float sensitivity, string name = "")
                : base(parent, size, name)
            { 
                this._Create(parent, horiz, vert, showBack, size, sensitivity, name);
            }

            public EleGenVertScrollRgn(EleBaseRect parent, ScrollInfo horiz, ScrollInfo vert, bool showBack, float sensitivity, string name = "")
                : base(parent)
            { 
                this._Create(parent, horiz, vert, showBack, new Vector2(-1.0f, -1.0f), sensitivity, name);
            }

            protected void _Create(
                EleBaseRect parent, 
                ScrollInfo horiz,
                ScrollInfo vert,
                bool showBack,
                Vector2 size, 
                float sensitivity,
                string name = "")
            {
                //      SCROLL ITEMS
                ////////////////////////////////////////////////////////////////////////////////
                GameObject go = new GameObject("ScrollRgn_" + name);
                go.transform.SetParent(parent.GetContentRect(), false);
                RectTransform rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.0f, 1.0f);
                rt.anchorMax = new Vector2(0.0f, 1.0f);
                rt.pivot = new Vector2(0.0f, 1.0f);

                GameObject goViewport = new GameObject("Viewport_" + name);
                goViewport.transform.SetParent(go.transform, false);
                UnityEngine.UI.Image imgViewport = goViewport.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtViewport = imgViewport.rectTransform;
                rtViewport.anchorMin = new Vector2(0.0f, 0.0f);
                rtViewport.anchorMax = new Vector2(1.0f, 1.0f);
                rtViewport.pivot = new Vector2(0.0f, 1.0f);
                rtViewport.offsetMin = new Vector2(0.0f, 0.0f);
                rtViewport.offsetMax = new Vector2(0.0f, 0.0f);

                UnityEngine.UI.Mask maskViewport = goViewport.AddComponent<UnityEngine.UI.Mask>();
                maskViewport.showMaskGraphic = showBack;

                GameObject goContent = new GameObject("Content_" + name);
                goContent.transform.SetParent(goViewport.transform, false);
                RectTransform rtContent = goContent.AddComponent<RectTransform>();

                //
                //      VERTICAL SCROLLBAR
                ////////////////////////////////////////////////////////////////////////////////
                GameObject goScrollVert = new GameObject("ScrollVert_" + name);
                goScrollVert.transform.SetParent(go.transform, false);
                UnityEngine.UI.Image imgScrollVert = goScrollVert.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtScrollVert = imgScrollVert.rectTransform;
                rtScrollVert.pivot = new Vector2(0.0f, 1.0f);
                rtScrollVert.anchorMin = new Vector2(1.0f, 0.0f);
                rtScrollVert.anchorMax = new Vector2(1.0f, 1.0f);
                rtScrollVert.anchoredPosition = new Vector2(-vert.scrollbarDim, 0.0f);
                rtScrollVert.sizeDelta = new Vector2(vert.scrollbarDim, 0.0f);
                imgScrollVert.sprite = vert.backplateSprite;
                imgScrollVert.type = UnityEngine.UI.Image.Type.Sliced;

                GameObject goThumbVert = new GameObject("ThumbVert_" + name);
                goThumbVert.transform.SetParent(goScrollVert.transform, false);
                UnityEngine.UI.Image imgThumbVert = goThumbVert.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtThumbVert = imgThumbVert.rectTransform;
                rtThumbVert.pivot = new Vector2(1.0f, 1.0f);
                rtThumbVert.anchorMin = new Vector2(0.0f, 0.0f);
                rtThumbVert.anchorMax = new Vector2(1.0f, 1.0f);
                rtThumbVert.anchoredPosition = new Vector2(0.0f, 0.0f);
                rtThumbVert.sizeDelta = new Vector2(0.0f, 0.0f);

                ScrollTy sbVert = goScrollVert.AddComponent<ScrollTy>();
                sbVert.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
                sbVert.targetGraphic = imgThumbVert;
                sbVert.handleRect = rtThumbVert;
                vert.Apply(sbVert, imgThumbVert);

                //
                //      HORIZONTAL SCROLLBAR
                ////////////////////////////////////////////////////////////////////////////////
                GameObject goScrollHoriz = new GameObject("ScrollHoriz_" + name);
                goScrollHoriz.transform.SetParent(go.transform, false);
                UnityEngine.UI.Image imgScrollHoriz = goScrollHoriz.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtScrollHoriz = imgScrollHoriz.rectTransform;
                rtScrollHoriz.pivot = new Vector2(0.0f, 0.0f);
                rtScrollHoriz.anchorMin = new Vector2(0.0f, 0.0f);
                rtScrollHoriz.anchorMax = new Vector2(1.0f, 0.0f);
                rtScrollHoriz.sizeDelta = new Vector2(0.0f, horiz.scrollbarDim);
                imgScrollHoriz.sprite = horiz.backplateSprite;
                imgScrollHoriz.type = UnityEngine.UI.Image.Type.Sliced;

                GameObject goThumbHoriz = new GameObject("ThumbHoriz_" + name);
                goThumbHoriz.transform.SetParent(goScrollHoriz.transform, false);
                UnityEngine.UI.Image imgThumbHoriz = goThumbHoriz.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtThumbHoriz = imgThumbHoriz.rectTransform;
                rtThumbHoriz.pivot = new Vector2(0.0f, 1.0f);
                rtThumbHoriz.anchorMin = new Vector2(0.0f, 0.0f);
                rtThumbHoriz.anchorMax = new Vector2(1.0f, 1.0f);
                rtThumbHoriz.anchoredPosition = new Vector2(0.0f, 0.0f);
                rtThumbHoriz.sizeDelta = new Vector2(0.0f, 0.0f);

                ScrollTy sbHoriz = goScrollHoriz.AddComponent<ScrollTy>();
                sbHoriz.direction = UnityEngine.UI.Scrollbar.Direction.LeftToRight;
                sbHoriz.targetGraphic = imgThumbHoriz;
                sbHoriz.handleRect = rtThumbHoriz;
                horiz.Apply(sbHoriz, imgThumbHoriz);

                //      FINISH
                ////////////////////////////////////////////////////////////////////////////////

                RectTy scrollR = go.AddComponent<RectTy>();
                scrollR.horizontalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollR.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
                scrollR.viewport = rtViewport;
                scrollR.content = rtContent;
                scrollR.horizontalScrollbar = sbHoriz;
                scrollR.verticalScrollbar = sbVert;
                scrollR.scrollSensitivity = sensitivity;

                this.viewportMask = maskViewport;
                //
                this.horizScroll = sbHoriz;
                this.vertScroll = sbVert;
                //
                this.rt = rt;
                this.scrollRect = scrollR;
            }

            public override Vector2 Layout(
                Dictionary<Ele, Vector2> cached,
                Dictionary<Ele, float> widths,
                Vector2 rectOffset,
                Vector2 offset,
                Vector2 size,
                bool collapsable = true)
            {

                if(collapsable == false)
                    this.scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                else
                    this.scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

                this.rt.sizeDelta = size;
                this.rt.anchoredPosition = new Vector2(rectOffset.x, -rectOffset.y);

                this.LayoutInternal();

                return size;
            }

            void LayoutInternal()
            {
                // If we have content to layout, we can do this independently, after-the-fact
                if (this.sizer == null)
                    return;

                Dictionary<Ele, Vector2> cachedChildren = new Dictionary<Ele, Vector2>();
                Dictionary<Ele, float> widthsChildren = new Dictionary<Ele, float>();

                Rect rInner = this.rt.rect; 

                float vSBWidth = this.vertScroll.GetComponent<RectTransform>().sizeDelta.x;
                float hSBHeight = this.horizScroll.GetComponent<RectTransform>().sizeDelta.y;

                this.sizer.GetMinWidth(widthsChildren);

                float noSBWidth = rInner.width - vSBWidth;
                float innerWidth = noSBWidth;

                Vector2 innserSz =
                    this.sizer.GetMinSize(
                        cachedChildren,
                        widthsChildren,
                        innerWidth);

                // If we don't do something that requires the right vertical scrollbar,
                // we can use its real-estate for GUI placement.
                if(innserSz.y <= rInner.height)
                    innerWidth = rInner.width;

                innserSz.x = Mathf.Max(innserSz.x, innerWidth);

                Vector2 innerLayoutSz =
                    this.sizer.Layout(
                        cachedChildren,
                        widthsChildren,
                        new Vector2(0.0f, 0.0f),
                        new Vector2(0.0f, 0.0f),
                        innserSz);

                if(innerLayoutSz.x <= innerWidth)
                { 
                    this.ScrollRect.horizontal = false;

                    RectTransform con = this.scrollRect.content;
                    con.anchoredPosition = new Vector2(0.0f, con.anchoredPosition.y);
                }
                else
                {
                    this.ScrollRect.horizontal = true;
                }

                this.scrollRect.content.sizeDelta =
                    innerLayoutSz;
            }

            protected override float ImplCalcMinSizeWidth(Dictionary<Ele, float> cache)
            {
                float min = this.minSize.x;

                //if (this.sizer != null)
                //{
                //    float szmin = this.sizer.GetMinWidth(cache);
                //    min = Mathf.Max(min, szmin);
                //}
                return min;
            }

            protected override Vector2 ImplCalcMinSize(
                Dictionary<Ele, Vector2> cache,
                Dictionary<Ele, float> widths,
                float width,
                bool collapsable = true)
            {
                Vector2 min = this.minSize;

                
                if (collapsable == false)
                {
                    if (this.sizer != null)
                    {
                        float vSBWidth = this.vertScroll.GetComponent<RectTransform>().sizeDelta.x;
                        float noSBWidth = width - vSBWidth;

                        Vector2 szmin = this.sizer.GetMinSize(cache, widths, noSBWidth, collapsable);
                        min = Vector2.Max(min, szmin);
                    }
                }

                return min;
            }

            public IEnumerator SetVertScrollLater(float val)
            { 
                return SetVertScrollLater(this.scrollRect, val);
            }

            public static IEnumerator SetVertScrollLater(UnityEngine.UI.ScrollRect sr, float val)
            { 
                yield return new WaitForEndOfFrame();
                sr.verticalNormalizedPosition = val;
            }
        }
    }
}
