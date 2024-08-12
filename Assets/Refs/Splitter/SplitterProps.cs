// <copyright file="SplitterProps.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/11/2020</date>
// <summary>
// The splitter aethetics and behaviour properties collection
// for the UIUtils.Splitter class.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIUtils
    {
        /// <summary>
        /// The splitter aethetics and behaviour properties collection
        /// for the UIUtils.Splitter class.
        /// </summary>
        [CreateAssetMenu(menuName = "PxPre/SplitterProps")]
        public class SplitterProps : ScriptableObject
        {
            /// <summary>
            /// The sprite to use for horizontal splitters.
            /// </summary>
            public Sprite spriteHoriz;

            /// <summary>
            /// The sprite to use for vertical splitters.
            /// </summary>
            public Sprite spriteVert;  // Only applies to things split vertically

            /// <summary>
            /// Optional sprite that appears in the middle of the thumb when split 
            /// horizontally
            /// </summary>
            public Sprite spriteThumbHoriz;

            /// <summary>
            /// Optional sprite that appears in the middle of the thumb when split 
            /// vertically.
            /// </summary>
            public Sprite spriteThumbVert;

            public Color thumbSpriteColor = Color.white;

            /// <summary>
            /// The dimensions of the sash. The x component is the size for horizontal
            /// splitters, and the y component is the size for vertical spliters. The x
            /// and the y components will be the same more-often-than-not.
            /// </summary>
            public Vector2 sashDim = new Vector2(10.0f, 10.0f);

            /// <summary>
            /// The modulating color of the sashes.
            /// </summary>
            public Color sashColor = Color.white;
        }
    }
}