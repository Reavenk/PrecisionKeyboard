using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        [CreateAssetMenuAttribute(fileName = "UILFactory", menuName = "UIL Factory")]
        public class Factory : ScriptableObject
        {
            public SelectableInfo buttonStyle;
            public TextAttrib buttonFont;
            public Vector2 minButtonSize;
            public PadRect buttonPadding;

            public TextAttrib headerTextAttrib;
            public Sprite headerSprite;
            public Vector2 minHeaderSize;
            public PadRect headerPadding;

            public Sprite horizontalSplitterSprite;
            public Sprite verticalSplitterSprite;
            public Vector2 minSplitterSize;

            public ScrollInfo verticalScroll;
            public ScrollInfo horizScroll;
            public bool scrollRectShowBack = false;
            public float scrollRectSensitivity = 50.0f;

            public TextAttrib textTextAttrib;

            public Sprite inputSprite;
            public PadRect inputPadding;
            public TextAttrib inputFont;

            internal UnityEngine.Events.UnityAction<UnityEngine.UI.Button> onCreateButton = null;
            internal UnityEngine.Events.UnityAction<UnityEngine.UI.Toggle> onCreateCheckbox = null;
            internal UnityEngine.Events.UnityAction<UnityEngine.UI.Text> onCreateText = null;
            internal UnityEngine.Events.UnityAction<UnityEngine.UI.Slider> onCreateSlider = null;

            public Vector2 minSliderSize;
            public ScrollInfo horizSlider;
            public ScrollInfo vertSlider;

            public Sprite checkboxToggleSprite;
            public UnityEngine.UI.Image.Type checkboxType;
            public bool expandCheckboxGraphic;
            public PadRect checkboxPadding;
            public SelectableInfo checkboxStyle;
            public float checkboxWidth = 50.0f;
            public float checkboxMinHeight = 50.0f;
            public float checkboxContentSeperationWidth = 10.0f;


            public void ApplyButtonStyle(UnityEngine.UI.Button button, UnityEngine.UI.Image img, UnityEngine.UI.Text text, bool callback = false)
            {
                if(text != null)
                { 
                    this.buttonFont.Apply(text);

                    text.rectTransform.offsetMin += new Vector2(this.buttonPadding.left, this.buttonPadding.bot);
                    text.rectTransform.offsetMax += new Vector2(-this.buttonPadding.right, -this.buttonPadding.top);
                }

                this.buttonStyle.Apply(button, img);

                if(callback == true)
                    this.onCreateButton?.Invoke(button);
            }

            void ApplyButtonStyle<BtnTy>(EleGenButton<BtnTy> ele)
                where BtnTy : UnityEngine.UI.Button

            { 
                ele.border = this.buttonPadding;
                this.ApplyButtonStyle(ele.Button, ele.Plate, ele.text, true);
            }

            public EleGenButton<BtnTy> CreateButton<BtnTy>(EleBaseRect parent, string text, Vector2 size, string name = "")
                where BtnTy : UnityEngine.UI.Button
            {
                EleGenButton<BtnTy> ele =
                    new EleGenButton<BtnTy>(
                        parent,
                        this.buttonFont.font,
                        this.buttonFont.fontSize,
                        this.buttonFont.color,
                        text,
                        this.buttonStyle.normalSprite,
                        size,
                        name);

                ele.border = this.buttonPadding;

                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleButton CreateButton(EleBaseRect parent, string text, Vector2 size, string name = "")
            { 
                EleButton ele = 
                    new EleButton(
                        parent, 
                        this.buttonFont.font,
                        this.buttonFont.fontSize,
                        this.buttonFont.color,
                        text,
                        this.buttonStyle.normalSprite,
                        size, 
                        name);



                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleGenButton<BtnTy> CreateButton<BtnTy>(EleBaseRect parent, string text)
                where BtnTy : UnityEngine.UI.Button
            {
                EleGenButton<BtnTy> ele =
                    new EleGenButton<BtnTy>(
                        parent,
                        this.buttonFont.font,
                        this.buttonFont.fontSize,
                        this.buttonFont.color,
                        text,
                        this.buttonStyle.normalSprite);

                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleButton CreateButton(EleBaseRect parent, string text)
            {
                EleButton ele =
                    new EleButton(
                        parent,
                        this.buttonFont.font,
                        this.buttonFont.fontSize,
                        this.buttonFont.color,
                        text,
                        this.buttonStyle.normalSprite);

                this.ApplyButtonStyle(ele);
                return ele;
            }


            public EleButton CreateButton(EleBaseRect parent, Vector2 size, string name = "")
            { 
                EleButton ele = 
                    new EleButton(
                        parent,
                        this.buttonFont.font,
                        this.buttonFont.fontSize,
                        this.buttonFont.color,
                        null,
                        this.buttonStyle.normalSprite,
                        size,
                        name);

                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleButton CreateButton(EleBaseRect parent)
            {
                EleButton ele =
                    new EleButton(
                        parent,
                        this.buttonFont.font,
                        this.buttonFont.fontSize,
                        this.buttonFont.color,
                        null,
                        this.buttonStyle.normalSprite);

                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleButton CreateButton(EleBaseRect parent, Sprite sprite, Vector2 size, string name = "")
            {
                EleButton ele = 
                    new EleButton(
                        parent,
                        sprite,
                        size,
                        name);

                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleButton CreateButton(EleBaseRect parent, Sprite sprite)
            {
                EleButton ele =
                    new EleButton(
                        parent,
                        sprite);

                this.ApplyButtonStyle(ele);
                return ele;
            }

            public EleSlider CreateHorizontalSlider(EleBaseRect parent, Vector2 size, string name = "")
            { 
                EleSlider ele = new EleSlider(parent, this.horizSlider, size, name);
                ele.Slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
                onCreateSlider?.Invoke(ele.Slider);

                return ele;
            }

            public EleSlider CreateHorizontalSlider(EleBaseRect parent, string name = "")
            {
                return this.CreateHorizontalSlider(parent, this.minSliderSize, name);
            }

            public EleGenSlider<ty> CreateHorizontalSlider<ty>(EleBaseRect parent, Vector2 size, string name = "") where ty : UnityEngine.UI.Slider
            { 
                EleGenSlider<ty>ele  = new EleGenSlider<ty>(parent, this.horizSlider, size, name);
                ele.Slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
                onCreateSlider?.Invoke(ele.Slider);

                return ele;
            }

            public EleGenSlider<ty> CreateHorizontalSlider<ty>(EleBaseRect parent, string name = "") where ty : UnityEngine.UI.Slider
            { 
                return CreateHorizontalSlider<ty>(parent, this.minSliderSize, name);
            }

            public EleHeader CreateHeader(EleBaseRect parent, string text)
            { 
                EleHeader ele = 
                    new EleHeader(
                        parent, 
                        text, 
                        this.headerTextAttrib.font, 
                        this.headerTextAttrib.color, 
                        this.headerTextAttrib.fontSize, 
                        this.headerSprite, 
                        this.headerPadding);

                ele.border = this.headerPadding;
                ele.minSize = this.minHeaderSize;

                return ele;
            }

            public EleBoxSizer HorizontalSizer(EleBaseRect parent, string name = "")
            {
                EleBoxSizer bs = new EleBoxSizer(parent, Direction.Horiz, name);
                return bs;
            }

            public EleBoxSizer VerticalSizer(EleBaseRect parent, string name = "")
            { 
                EleBoxSizer bs = new EleBoxSizer(parent, Direction.Vert, name);
                return bs;
            }

            public EleBoxSizer HorizontalSizer(EleBaseSizer parent, float proportion, LFlag flags)
            {
                EleBoxSizer bs = new EleBoxSizer(parent, Direction.Horiz, proportion, flags);
                return bs;
            }

            public EleBoxSizer VerticalSizer(EleBaseSizer parent, float proportion, LFlag flags)
            {
                EleBoxSizer bs = new EleBoxSizer(parent, Direction.Vert, proportion, flags);
                return bs;
            }

            public EleGridSizer GridSizer(EleBaseRect parent, int cols, string name = "")
            { 
                EleGridSizer gs = new EleGridSizer(parent, cols, name);
                return gs;
            }

            public EleGridSizer GridSizer(EleBaseSizer parent, int cols, float proportion, LFlag flags)
            { 
                EleGridSizer gs = new EleGridSizer(parent, cols, proportion, flags);
                return gs;
            }

            public EleSeparator CreateSeparator(EleBaseRect parent, Vector2 size)
            { 
                EleSeparator ele = 
                    new EleSeparator(
                        parent, 
                        this.horizontalSplitterSprite, 
                        size, 
                        "");

                return ele;
            }

            public EleSeparator CreateHorizontalSeparator(EleBaseRect parent)
            { 
                EleSeparator ele = 
                    new EleSeparator(
                        parent, 
                        this.horizontalSplitterSprite, 
                        LFlag.GrowHoriz);

                ele.minSize = this.minSplitterSize;

                return ele;
            }

            public EleSeparator CreateVerticalSeparator(EleBaseRect parent)
            { 
                EleSeparator ele = 
                    new EleSeparator(
                        parent, 
                        this.horizontalSplitterSprite, 
                        LFlag.GrowVert);

                ele.minSize = this.minSplitterSize;

                return ele;
            }

            public EleText CreateText(EleBaseRect parent, string text, bool wrap, Vector2 size, string name = "")
            { 
                EleText ele = 
                    new EleText(
                        parent, 
                        text,
                        wrap, 
                        this.textTextAttrib.font, 
                        this.textTextAttrib.color, 
                        this.textTextAttrib.fontSize, 
                        size, 
                        name);

                return ele;
            }

            public void ApplyTextStyle(UnityEngine.UI.Text txt)
            { 
                txt.font = this.textTextAttrib.font;
                txt.color = this.textTextAttrib.color;
                txt.fontSize = this.textTextAttrib.fontSize;
            }

            public void ApplyTextStyle(UnityEngine.UI.Text txt, int fontSize)
            { 
                this.ApplyTextStyle(txt);
                txt.fontSize = fontSize;
            }

            public void ApplyTextStyle(UnityEngine.UI.Text txt, int fontSize, string text)
            { 
                this.ApplyTextStyle(txt);
                txt.fontSize = fontSize;
                txt.text = text;
            }

            public EleText CreateText(EleBaseRect parent, string text, bool wrap)
            {
                EleText ele = 
                    new EleText(
                        parent,
                        text,
                        wrap,
                        this.textTextAttrib.font,
                        this.textTextAttrib.color,
                        this.textTextAttrib.fontSize);

                return ele;
            }

            public EleText CreateText(EleBaseRect parent, string text, int fontSize, bool wrap)
            {
                EleText ele =
                    new EleText(
                        parent,
                        text,
                        wrap,
                        this.textTextAttrib.font,
                        this.textTextAttrib.color,
                        this.textTextAttrib.fontSize);

                ele.text.fontSize = fontSize;

                return ele;
            }

            public EleImg CreateImage(EleBaseRect parent, Sprite sprite, Vector2 size, string name = "")
            { 
                EleImg ret = new EleImg(parent, sprite, size, name);
                return ret;
            }
            
            public EleImg CreateImage(EleBaseRect parent, Sprite sprite, string name = "")
            { 
                EleImg ret = new EleImg(parent, sprite);
                return ret;
            }

            public ElePropGrid CreatePropertyGrid(EleBaseRect parent)
            { 
                ElePropGrid epg = new ElePropGrid(parent, this.textTextAttrib);
                return epg;
            }

            public ElePropGrid CreatePropertyGrid(EleBaseRect parent, int fontSize)
            {
                ElePropGrid epg = new ElePropGrid(parent, this.textTextAttrib, fontSize);
                return epg;
            }

            public EleVertScrollRgn CreateVerticalScrollRect(EleBaseRect parent, string name = "")
            { 
                EleVertScrollRgn evsr = 
                    new EleVertScrollRgn(
                        parent, 
                        this.horizScroll, 
                        this.verticalScroll,
                        this.scrollRectShowBack,
                        this.scrollRectSensitivity, 
                        name);

                return evsr;
            }

            public EleGenVertScrollRgn<RectTy, ScrollTy> CreateGenVerticalScrollRect<RectTy, ScrollTy>(EleBaseRect parent, string name = "")
                where RectTy : UnityEngine.UI.ScrollRect
                where ScrollTy : UnityEngine.UI.Scrollbar
            { 
                EleGenVertScrollRgn<RectTy, ScrollTy> evsr = 
                    new EleGenVertScrollRgn<RectTy, ScrollTy>(
                        parent, 
                        this.horizScroll, 
                        this.verticalScroll,
                        this.scrollRectShowBack,
                        this.scrollRectSensitivity, 
                        name);

                return evsr;
            }

            public EleInput CreateInput(EleBaseRect parent, bool multiline = false)
            { 
                EleInput inp = 
                    new EleInput(
                        parent, 
                        "", 
                        this.inputFont.font, 
                        this.inputFont.color, 
                        this.inputFont.fontSize, 
                        multiline, 
                        this.inputSprite,
                        this.inputPadding,
                        new Vector2(-1.0f, -1.0f));

                return inp;
            }

            public EleToggle CreateToggle(EleBaseRect parent, string label, Vector2 size, string name = "")
            {
                size.x = Mathf.Max(this.checkboxWidth, size.x);
                size.y = Mathf.Max(this.checkboxMinHeight, size.y);

                EleToggle etog =
                    new EleToggle(
                        parent,
                        string.IsNullOrEmpty(label) ? null : this.textTextAttrib,
                        label,
                        this.checkboxStyle,
                        this.checkboxWidth,
                        this.checkboxToggleSprite,
                        this.checkboxType,
                        this.checkboxPadding,
                        size,
                        this.checkboxContentSeperationWidth);

                this.onCreateCheckbox?.Invoke(etog.toggle);

                return etog;
            }

            public EleToggle CreateToggle(EleBaseRect parent, string label, string name = "")
            { 
                return this.CreateToggle(parent, label, Vector2.zero, name);
            }

            public EleGenToggle<ty> CreateToggle<ty>(EleBaseRect parent, string label, Vector2 size, string name = "")
                where ty : UnityEngine.UI.Toggle
            {
                size.x = Mathf.Max(this.checkboxWidth, size.x);
                size.y = Mathf.Max(this.checkboxMinHeight, size.y);

                EleGenToggle<ty> egt = 
                    new EleGenToggle<ty>(
                        parent, 
                        string.IsNullOrEmpty(label) ? null : this.textTextAttrib,
                        label,
                        this.checkboxStyle,
                        this.checkboxWidth,
                        this.checkboxToggleSprite,
                        this.checkboxType,
                        this.checkboxPadding,
                        size,
                        this.checkboxContentSeperationWidth);

                this.onCreateCheckbox?.Invoke(egt.toggle);

                return egt;
            }

            public EleGenToggle<ty> CreateToggle<ty>(EleBaseRect parent, string label, string name = "")
                where ty : UnityEngine.UI.Toggle
            { 
                return CreateToggle<ty>(parent, label, Vector2.zero, name);
            }

            //public EleImg CreateHorizontalSpacer(Ele parent, int flags, Vector2 size)
            //{ 
            //}
            //
            //public EleImg CreateVerticalSpacer(Ele parent, int flags, Vector2 size)
            //{ 
            //}
            //
            //public EleScrollView CreateScrollView(Ele parent)
            //{ 
            //}
            //
            //public static CreateBoxSizer(Ele parent)
            //{ 
            //}
        }
    }
}