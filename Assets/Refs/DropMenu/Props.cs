using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace DropMenu
    {
        [CreateAssetMenu(menuName = "PxPre/DropMenuProps")]
        public class Props : ScriptableObject
        {
            [System.Serializable]
            public class Padding
            {
                public float top;
                public float left;
                public float right;
                public float bottom;
            }

            [System.Serializable]
            public class SelectableProperties
            {
                public Sprite spriteNormal;
                public Color color;
                public UnityEngine.UI.ColorBlock colorBlock;
                public UnityEngine.UI.SpriteState spriteState;
                public UnityEngine.UI.Selectable.Transition transition;

                public void Apply(UnityEngine.UI.Selectable sel, UnityEngine.UI.Image img)
                { 
                    img.color = this.color;
                    img.sprite = this.spriteNormal;
                    img.type = UnityEngine.UI.Image.Type.Sliced;

                    sel.colors = this.colorBlock;
                    sel.spriteState = this.spriteState;
                    sel.targetGraphic = img;
                    sel.transition = this.transition;
                }
            }

            public enum TextAlignment
            { 
                Left,
                Middle,
                Right,
                Default
            }

            /// <summary>
            /// The color and opacity of the modal plate hosting the dropdown menu.
            /// </summary>
            public Color modalPlateColor;

            /// <summary>
            /// If true, show the titles of menus
            /// </summary>
            public bool showTitles;

            public Padding outerPadding;

            // For now this is just a variable that is held and referenced,
            // but not directly used.
            public Color unselectedColor = Color.white;
            // For now this is just a variable that is held and referenced,
            // but not directly used.
            public Color selectedColor = new Color(0.8f, 1.0f, 0.8f);

            public Font titleFont;
            public Font entryFont;
            public Color entryFontColor = Color.black;
            public Padding entryPadding;
            public int entryFontSize = 14;
            public float minEntrySize = 20.0f;
            public Sprite submenuSpawnArrow;
            public Sprite entrySprite;

            public float iconTextPadding = 5.0f;
            public float textArrowPadding = 5.0f;

            public Sprite shadow;
            public Sprite plate;
            public Vector2 shadowOffset;
            public Color shadowColor = Color.black;

            public Sprite splitter;
            public Padding splitterPadding;
            public float minSplitter = 10.0f;

            public float childrenSpacing = 3.0f;

            public Sprite scrollbarPlate;
            public SelectableProperties overflowScrollbar;
            public float scrollbarWidth = 50.0f;
            public float scrollSensitivity = 50.0f;

            public TextAlignment defaultTextAlignment;

            public bool useGoBack = true;
            public Sprite goBackIcon;
            public int goBackFontSize;
            public string goBackMessage = "Go Back";

            public TextAnchor GetTextAnchorFromAlignment(TextAlignment alignment, bool canDefault)
            { 
                switch(alignment)
                { 
                    case TextAlignment.Left:
                        return TextAnchor.MiddleLeft;

                    case TextAlignment.Middle:
                        return TextAnchor.MiddleCenter;

                    case TextAlignment.Right:
                        return TextAnchor.MiddleRight;

                    default:
                        if(canDefault == false)
                            return TextAnchor.MiddleLeft;

                        return GetTextAnchorFromAlignment(this.defaultTextAlignment, false);
                }
            }
        }
    }
}