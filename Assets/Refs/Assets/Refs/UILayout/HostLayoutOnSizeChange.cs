// <copyright file=".cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>05/22/2020</date>
// <summary>A utility class binds to a UILayout element and triggers
// a layout whenever the parent RectTransform has its dimensions changed.</summary>

// TODO: Move the UIL class

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public class HostLayoutOnSizeChange : UnityEngine.EventSystems.UIBehaviour
        {
            PxPre.UIL.Ele ele;
            PxPre.UIL.EleHost host;

            public bool destroyOnEscape = true;

            bool doubleTake = true;

            protected override void OnRectTransformDimensionsChange()
            { 
                RectTransform rt = this.GetComponent<RectTransform>();
                Vector2 v2 = rt.rect.size;

                Rect rect = rt.rect;

                for(int i = 0; i < 2; ++i)
                {
                    bool collapsable =  i != 0;
                    Vector2 sz = Vector2.zero;
                    if(this.host != null)
                    {
                        Dictionary<PxPre.UIL.Ele, Vector2> cached = 
                            new Dictionary<PxPre.UIL.Ele, Vector2>();

                        sz = this.host.LayoutInRT(collapsable);
                    }
                    else if(this.ele != null)
                    {
                        Dictionary<PxPre.UIL.Ele, float> widths = new Dictionary<PxPre.UIL.Ele, float>();
                        Dictionary<PxPre.UIL.Ele, Vector2> cached = new Dictionary<PxPre.UIL.Ele, Vector2>();

                        sz = this.ele.Layout(cached, widths, Vector2.zero, Vector2.zero, v2);
                    }

                    if(doubleTake == false || sz.y <= rect.height)
                        break;
                }
            }

            private void Update()
            {
                if(this.destroyOnEscape == true)
                {
                    if(Input.GetKeyDown(KeyCode.Escape) == true)
                        GameObject.Destroy(this.gameObject);
                }
            }

            public static void Attach(GameObject go, PxPre.UIL.Ele ele, bool destroyOnEscape = false)
            {
                HostLayoutOnSizeChange uilosc = go.AddComponent<HostLayoutOnSizeChange>();
                uilosc.ele = ele;
                uilosc.destroyOnEscape = destroyOnEscape;
            }

            public static void AttachHost(GameObject go, PxPre.UIL.EleHost host, bool destroyOnEscape = false)
            {
                HostLayoutOnSizeChange uilosc = go.AddComponent<HostLayoutOnSizeChange>();
                uilosc.host = host;
                uilosc.destroyOnEscape = destroyOnEscape;
            }
        }
    }
}
