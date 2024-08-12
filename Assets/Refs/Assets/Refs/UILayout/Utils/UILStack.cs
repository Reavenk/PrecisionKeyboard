using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class UILStack
        {
            struct Entry
            {
                public EleBaseRect rect;
                public EleBaseSizer sizer;

                public Entry(EleBaseRect rect)
                { 
                    this.rect = rect;
                    this.sizer = null;
                }

                public Entry(EleBaseRect rect, EleBaseSizer sizer)
                { 
                    this.rect = rect;
                    this.sizer = sizer;
                }

                public EleBaseSizer GetSizer()
                { 
                    if(this.sizer != null)
                        return this.sizer;

                    return this.rect.Sizer;
                }
            }

            Factory uiFactory;

            Entry head;
            Stack<Entry> stack = new Stack<Entry>();

            public EleBaseSizer GetHeadSizer()
            { 
                return head.sizer;
            }

            public RectTransform GetHeadRectTransform()
            { 
                return head.rect.RT;
            }

            public UILStack(Factory factory, EleBaseRect rect, EleBaseSizer sizer)
            { 
                this.uiFactory = factory;
                this.head = new Entry(rect, sizer);
            }

            public UILStack GetSnapshot()
            { 
                return new UILStack(this.uiFactory, this.head.rect, this.head.sizer);
            }

            public UILStack(Factory factory, EleBaseRect rect)
            { 
                this.uiFactory = factory;
                this.head = new Entry(rect, rect.Sizer);
            }

            public EleBoxSizer PushHorizSizer()
            { 
                return this.PushHorizSizer(0.0f, 0);
            }

            public EleBoxSizer AddHorizSizer()
            { 
                return this.AddHorizSizer(0.0f, 0);
            }

            public EleBoxSizer AddHorizSizer(float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                EleBoxSizer ret;

                if (szr == null)
                    ret = this.uiFactory.HorizontalSizer(this.head.rect);
                else
                    ret = this.uiFactory.HorizontalSizer(szr, proportion, flags);

                return ret;
            }

            public EleBoxSizer PushHorizSizer(float proportion, LFlag flags)
            {
                EleBoxSizer szr = this.AddHorizSizer(proportion, flags);
                if(szr == null)
                    return null;

                Entry newE = new Entry(this.head.rect, szr);
                this.stack.Push(this.head);
                this.head = newE;

                return szr;
            }

            public EleBoxSizer PushVertSizer()
            { 
                return this.PushVertSizer(0.0f, 0);
            }

            public EleBoxSizer AddVertSizer()
            {
                return this.AddVertSizer(0.0f, 0);
            }

            public EleBoxSizer AddVertSizer(float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                EleBoxSizer ret;

                if (szr == null)
                    ret = this.uiFactory.VerticalSizer(this.head.rect);
                else
                    ret = this.uiFactory.VerticalSizer(szr, proportion, flags);

                return ret;
            }

            public EleBoxSizer PushVertSizer(float proportion, LFlag flags)
            {
                EleBoxSizer szr = this.AddVertSizer(proportion, flags);
                if(szr == null)
                    return null;

                Entry newE = new Entry(this.head.rect, szr);
                this.stack.Push(this.head);
                this.head = newE;

                return szr;
            }

            public EleGridSizer AddGridSizer(int cols, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                EleGridSizer ret;

                if(szr == null)
                    ret = this.uiFactory.GridSizer(this.head.rect, cols);
                else
                    ret = this.uiFactory.GridSizer(szr, cols, proportion, flags);

                return ret;
            }

            public EleGridSizer PushGridSizer(int cols, float proportion, LFlag flags)
            { 
                EleGridSizer szr = this.AddGridSizer(cols, proportion, flags);
                if(szr == null)
                    return null;

                Entry newE = new Entry(this.head.rect, szr);
                this.stack.Push(this.head);
                this.head = newE;

                return szr;
            }

            public EleGridSizer AddGridSizer(int cols)
            { 
                return this.AddGridSizer(cols, 0.0f, 0);
            }

            public EleGridSizer PushGridSizer(int cols)
            { 
                return this.PushGridSizer(cols, 0.0f, 0);
            }

            public EleText CreateText(string text, int fontSize, bool wrap)
            {
                if(this.head.rect == null)
                    return null;

                EleText ret = this.uiFactory.CreateText(this.head.rect, text, fontSize, wrap);
                return ret;
            }

            public EleText AddText(string text, int fontSize, bool wrap, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleText ret = this.uiFactory.CreateText(this.head.rect, text, fontSize, wrap);
                szr.Add(ret, proportion, flags);
                return ret;
            }

            public EleText CreateText(string text, bool wrap)
            {
                if(this.head.rect == null)
                    return null;

                EleText ret = this.uiFactory.CreateText(this.head.rect, text, wrap);
                return ret;
            }

            public EleText AddText(string text, bool wrap, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;
                
                EleText ret = this.uiFactory.CreateText(this.head.rect, text, wrap);
                szr.Add(ret, proportion, flags);
                return ret;
            }

            public EleImg AddImage(Sprite sprite, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleImg img = this.uiFactory.CreateImage(this.head.rect, sprite);
                szr.Add(img, proportion, flags);
                return img;
            }

            public EleImg PushImage(Sprite sprite, float proportion, LFlag flags)
            { 
                EleImg ret = this.AddImage(sprite, proportion, flags);
                if(ret == null)
                    return null;

                this.stack.Push(this.head);

                this.head = new Entry(ret);
                return ret;
            }

            public EleButton AddButton(string text, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleButton btn = this.uiFactory.CreateButton(this.head.rect, text);
                szr.Add(btn, proportion, flags);
                return btn;
            }

            public EleButton CreateButton(string text)
            {
                if(this.head.rect == null)
                    return null;

                EleButton btn = this.uiFactory.CreateButton(this.head.rect, text);
                return btn;
            }

            public EleButton PushButton(string text, float proportion, LFlag flags)
            {
                EleButton ret = this.AddButton(text, proportion, flags);
                if (ret == null)
                    return null;

                this.stack.Push(this.head);

                this.head = new Entry(ret);
                return ret;
            }

            public EleGenButton<ty> CreateButton<ty>(string text) where ty : UnityEngine.UI.Button
            {
                if(this.head.rect == null)
                    return null;

                EleGenButton<ty> btn = this.uiFactory.CreateButton<ty>(this.head.rect, text);
                return btn;
            }

            public EleGenButton<ty> AddButton<ty>(string text, float proportion, LFlag flags) where ty : UnityEngine.UI.Button
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleGenButton<ty> btn = this.uiFactory.CreateButton<ty>(this.head.rect, text);
                szr.Add(btn, proportion, flags);
                return btn;
            }

            public EleGenButton<ty> PushButton<ty>(string text, float proportion, LFlag flags) where ty : UnityEngine.UI.Button
            {
                EleGenButton<ty> ret = this.AddButton<ty>(text, proportion, flags);
                if (ret == null)
                    return null;

                this.stack.Push(this.head);

                this.head = new Entry(ret);
                return ret;
            }

            public EleGenSlider<ty> CreateHorizontalSlider<ty>() where ty : UnityEngine.UI.Slider
            {
                if(this.head.rect == null)
                    return null;

                EleGenSlider<ty> sldr = this.uiFactory.CreateHorizontalSlider<ty>(this.head.rect);
                return sldr;
            }

            public EleGenSlider<ty> AddHorizontalSlider<ty>(float proportion, LFlag flags) where ty : UnityEngine.UI.Slider
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleGenSlider<ty> sldr = this.uiFactory.CreateHorizontalSlider<ty>(this.head.rect);
                szr.Add(sldr, proportion, flags);
                return sldr;
            }

            public EleSlider CreateHorizontalSlider()
            {
                if(this.head.rect == null)
                    return null;

                EleSlider sldr = this.uiFactory.CreateHorizontalSlider(this.head.rect);
                return sldr;
            }

            public EleSlider AddHorizontalSlider(float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleSlider sldr = this.uiFactory.CreateHorizontalSlider(this.head.rect);
                szr.Add(sldr, proportion, flags);
                return sldr;
            }

            public EleGenVertScrollRgn<RTy, STy> AddVertScrollRect<RTy, STy>(float proportion, LFlag flags, string name = "")
                where RTy : UnityEngine.UI.ScrollRect
                where STy : UnityEngine.UI.Scrollbar
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;
                
                EleGenVertScrollRgn<RTy, STy> srgn = this.uiFactory.CreateGenVerticalScrollRect<RTy, STy>(this.head.rect, name);
                szr.Add(srgn, proportion, flags);
                return srgn;
            }
            
            public EleGenVertScrollRgn<RTy, STy> PushVertScrollRect<RTy, STy>(float proportion, LFlag flags, string name = "")
                where RTy : UnityEngine.UI.ScrollRect
                where STy : UnityEngine.UI.Scrollbar
            {
                EleGenVertScrollRgn<RTy, STy> ret = this.AddVertScrollRect<RTy, STy>(proportion, flags, name);
                if (ret == null)
                    return null;
            
                this.stack.Push(this.head);
            
                this.head = new Entry(ret);
                return ret;
            }



            public EleVertScrollRgn AddVertScrollRect(float proportion, LFlag flags, string name)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleVertScrollRgn srgn = 
                    this.uiFactory.CreateVerticalScrollRect(this.head.rect, name);

                szr.Add(srgn, proportion, flags);
                return srgn;
            }

            public EleVertScrollRgn PushVertScrollRect(float proportion, LFlag flags, string name = "")
            {
                EleVertScrollRgn ret = this.AddVertScrollRect(proportion, flags, name);
                if (ret == null)
                    return null;

                this.stack.Push(this.head);

                this.head = new Entry(ret);
                return ret;
            }

            public EleGenToggle<ty> CreateToggle<ty>(string label)
                where ty : UnityEngine.UI.Toggle
            {
                if(this.head.rect == null)
                    return null;

                EleGenToggle<ty> tog = this.uiFactory.CreateToggle<ty>(this.head.rect, label);
                return tog;
            }

            public EleGenToggle<ty> AddToggle<ty>(string label, float proportion, LFlag flags)
                where ty : UnityEngine.UI.Toggle
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleGenToggle<ty> tog = this.uiFactory.CreateToggle<ty>(this.head.rect, label);
                szr.Add(tog, proportion, flags);
                return tog;
            }

            public EleToggle CreateToggle(string label)
            {
                if(this.head.rect == null)
                    return null;

                EleToggle tog = this.uiFactory.CreateToggle(this.head.rect, label);
                return tog;
            }

            public EleToggle AddToggle(string label, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleToggle tog = this.uiFactory.CreateToggle(this.head.rect, label);
                szr.Add(tog, proportion, flags);
                return tog;
            }

            public ElePropGrid PushPropGrid(float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                ElePropGrid epg = this.uiFactory.CreatePropertyGrid(this.head.rect);
                szr.Add(epg, proportion, flags);

                this.stack.Push(this.head);
                this.head = new Entry(this.head.rect, epg);

                return epg;
            }

            public ElePropGrid PushPropGrid(int fontSize, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                ElePropGrid epg = this.uiFactory.CreatePropertyGrid(this.head.rect, fontSize);
                szr.Add(epg, proportion, flags);

                this.stack.Push(this.head);
                this.head = new Entry(this.head.rect, epg);

                return epg;
            }

            public EleSeparator AddHorizontalSeparator(float proportion = 0.0f)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if (szr == null)
                    return null;

                EleSeparator sep = this.uiFactory.CreateHorizontalSeparator(this.head.rect);
                szr.Add(sep, proportion, LFlag.GrowHoriz);
                return sep;
            }

            public EleSpace AddSpace(float size, float proportion, LFlag flags)
            { 
                return this.AddSpace(new Vector2(size, size), proportion, flags);
            }

            public EleSpace AddHorizSpace(float width, float proportion, LFlag flags)
            { 
                return this.AddSpace(new Vector2(width, 0.0f), proportion, flags);
            }

            public EleSpace AddVertSpace(float height, float proportion, LFlag flags)
            { 
                return this.AddSpace(new Vector2(0.0f, height), proportion, flags);
            }

            public EleSpace AddSpace(Vector2 sz, float proportion, LFlag flags)
            {
                EleBaseSizer szr = this.head.GetSizer();
                if(szr == null)
                    return null;

                EleSpace space = new EleSpace(sz);
                szr.Add(space, proportion, flags);
                return space;
            }

            public bool Pop()
            { 
                if(stack.Count == 0)
                    return false;

                this.head = stack.Pop();

                return true;
            }
        }
    }
}
