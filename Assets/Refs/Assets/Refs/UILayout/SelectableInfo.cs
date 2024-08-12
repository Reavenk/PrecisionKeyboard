using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        [System.Serializable]
        public class SelectableInfo
        {
            public UnityEngine.UI.Selectable.Transition transition =
                UnityEngine.UI.Selectable.Transition.ColorTint;

            public Sprite normalSprite;
            public UnityEngine.UI.SpriteState spriteState;

            public Color normalColor;
            public UnityEngine.UI.ColorBlock colorStates =
                UnityEngine.UI.ColorBlock.defaultColorBlock;

            public void Apply(UnityEngine.UI.Selectable sel, UnityEngine.UI.Image img)
            {
                sel.transition = this.transition;

                sel.spriteState = this.spriteState;
                sel.colors = this.colorStates;

                UnityEngine.UI.Graphic graphic = sel.targetGraphic;
                if (graphic != null)
                    graphic.color = this.normalColor;

                if (img != null)
                {
                    img.type = UnityEngine.UI.Image.Type.Sliced;
                    img.color = this.normalColor;
                    img.sprite = this.normalSprite;
                }
            }
        }
    }
}