// <copyright file="Splitter.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/11/2020</date>
// <summary>
// A pane and splitter UI utility class.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIUtils
    { 
        /// <summary>
        /// A pane and splitter UI utility class.
        /// </summary>
        [RequireComponent(typeof(RectTransform))]
        public class Splitter : MonoBehaviour
        {
            /// <summary>
            /// The grain, aka: the significant side.
            /// </summary>
            public enum SplitGrain
            { 
                /// <summary>
                /// Splitting the panes horizontally.
                /// </summary>
                Horizontal,

                /// <summary>
                /// Splitting the panes verically.
                /// </summary>
                Vertical
            }

            /// <summary>
            /// The key structure for addressing sashes in this.rectSizes.
            /// </summary>
            private struct SashKey
            { 
                /// <summary>
                /// The top or left pane.
                /// </summary>
                RectTransform a;    

                /// <summary>
                /// The bottom or right pane.
                /// </summary>
                RectTransform b;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="a">The top or left pane.</param>
                /// <param name="b">The bottom or right pane.</param>
                public SashKey(RectTransform a, RectTransform b)
                { 
                    this.a = a;
                    this.b = b;
                }
            }

            [System.Serializable]
            public struct PropPlatformGroup
            {
                public List<UnityEngine.RuntimePlatform> platforms;
                public SplitterProps prop;
            }

            /// <summary>
            /// Cached RectTransform in the GameObject.
            /// </summary>
            private RectTransform rectTransform;

            /// <summary>
            /// The cached significant size for each pane.
            /// </summary>
            private Dictionary<RectTransform, float>  rectSizes = 
                new Dictionary<RectTransform, float>();

            /// <summary>
            /// The sash objects.
            /// </summary>
            private Dictionary<SashKey, UnityEngine.UI.Image> sashes = null;

            /// <summary>
            /// The minimum size a pane can be (if there's space). This applies to 
            /// both horizontal and vertical - since in the end, it can only be one
            /// of them for a splitter.
            /// </summary>
            public float minSize = 50.0f;

            /// <summary>
            /// The setting of if the splitter is splitting vertically or horizontally.
            /// </summary>
            public SplitGrain splitGrain = SplitGrain.Horizontal;
             
            public List<PropPlatformGroup> propsByPlatform;

            /// <summary>
            /// The sharable properties of the splitter. Cannot be null.
            /// </summary>
            public SplitterProps props;

            /// <summary>
            /// The panes being managed. This variable is also used during edit-time
            /// to specify starting panes.
            /// </summary>
            public List<RectTransform> panes;

            void Awake()
            { 
                if(this.propsByPlatform != null)
                { 
                    foreach(PropPlatformGroup ppg in this.propsByPlatform)
                    { 
                        if(ppg.platforms == null || ppg.prop == null)
                            continue;

                        if(ppg.platforms.Contains(UnityEngine.Application.platform) == true)
                        {
                            this.props = ppg.prop;
                            break;
                        }
                    }
                }

                this.rectTransform = this.GetComponent<RectTransform>();

                this.rectTransform.pivot = new Vector2(0.0f, 1.0f);

                // Add the starting panes, taking into account the starting sizes they
                // had when they were edited.
                if(this.panes != null)
                    this.SetPane(this.panes);
            }

            /// <summary>
            /// Set an ordered collection of panes to be managed by the splitter.
            /// </summary>
            /// <param name="ieRT">The ordered collection of panes.</param>
            public void SetPane(IEnumerable<RectTransform> ieRT)
            { 
                // If this is being called on Awake, we're creating this.panes from
                // this.panes. We'll accept that one-time overhead in order to avoid
                // having to create specialized code.
                this.panes = new List<RectTransform>(ieRT);

                int idx = this.GetVectorIndex();
                foreach (RectTransform rt in this.panes)
                {
                    Vector2 origSz = rt.rect.size;
                    PrepareAsChild(rt);
                    this.rectSizes.Add(rt, origSz[idx]);

                }

                // If we had previous sashes for other stuff, or for a different
                // ordering, they need to go. They'll be recreated in UpdateAlignment();
                this.ClearSashes();
                this.UpdateAlignment();
            }

            /// <summary>
            /// The index into a Vector2 of the component which is the grain.
            /// </summary>
            /// <returns></returns>
            public int GetVectorIndex()
            { 
                if(this.splitGrain == SplitGrain.Vertical)
                    return 1;

                return  0;
            }

            /// <summary>
            /// Get relevant indices of a Vector2 for the splitter's grain.
            /// </summary>
            /// <param name="main">The main grain. The same value GetVectorIndex() would return.</param>
            /// <param name="other">The orthoginal grain.</param>
            public void GetVectorIndices(out int main, out int other)
            { 
                if(this.splitGrain == SplitGrain.Vertical)
                { 
                    main = 1;
                    other = 0;
                }

                main = 0;
                other = 1;
            }

            /// <summary>
            /// Modify a RectTransform about to be managed as a pane,
            /// it has has the expections on its transform that are required to properly
            /// manage it.
            /// </summary>
            /// <param name="tr">The RectTransform that's going to managed as a pane.</param>
            private void PrepareAsChild(RectTransform tr)
            {
                tr.transform.SetParent(this.transform);
                tr.localPosition    = Vector3.zero;
                tr.localRotation    = Quaternion.identity;
                tr.localScale       = Vector3.one;

                tr.pivot = new Vector2(0.0f, 1.0f);
                tr.anchorMin = new Vector2(0.0f, 1.0f);
                tr.anchorMax = new Vector2(0.0f, 1.0f);
                tr.offsetMin = new Vector2(0.0f, 0.0f);
                tr.offsetMax = new Vector2(0.0f, 0.0f);
            }

            /// <summary>
            /// Delete all the sashes, and set the sash list as dirty.
            /// </summary>
            private void ClearSashes()
            { 
                if(this.sashes == null)
                    return;

                // Destroy them all
                foreach (KeyValuePair<SashKey, UnityEngine.UI.Image> kvp in this.sashes)
                    GameObject.Destroy(kvp.Value.gameObject);

                this.sashes = null;
            }

            /// <summary>
            /// Remake all the sash assets.
            /// </summary>
            /// <returns>True if the sashes were remade.</returns>
            /// <remarks>The function doesn't properly place them. It assumes the outside
            /// function that called RemakeSashes will handle that after knowing that the proper
            /// sash assets exist.</remarks>
            private bool RemakeSashes()
            { 
                if(this.sashes != null)
                    return false;

                this.sashes = new Dictionary<SashKey, UnityEngine.UI.Image>();
                for(int i = 0; i < this.panes.Count - 1; ++i)
                { 
                    SashKey sk = new SashKey(this.panes[i], this.panes[i + 1]);

                    GameObject goSplitter = new GameObject("_Splitter");
                    SplitterSash imgSplitter = goSplitter.AddComponent<SplitterSash>();
                    goSplitter.transform.SetParent(this.transform);

                    Sprite thumbSprite = null;
                    if(this.splitGrain == SplitGrain.Horizontal)
                    {
                        imgSplitter.sprite = this.props.spriteHoriz;
                        thumbSprite = this.props.spriteThumbHoriz;
                    }
                    else
                    {
                        imgSplitter.sprite = this.props.spriteVert;
                        thumbSprite = this.props.spriteThumbVert;
                    }

                    imgSplitter.type = UnityEngine.UI.Image.Type.Sliced;
                    this.PrepareAsChild(imgSplitter.rectTransform);

                    if(thumbSprite != null)
                    { 
                        GameObject goThumbSprite = new GameObject("ThumbSprite");
                        goThumbSprite.transform.SetParent(goSplitter.transform, false);
                        UnityEngine.UI.Image imgThumbSprite = goThumbSprite.AddComponent<UnityEngine.UI.Image>();
                        imgThumbSprite.sprite = thumbSprite;
                        imgThumbSprite.color = this.props.thumbSpriteColor;
                        RectTransform rtThumbSprite = imgThumbSprite.rectTransform;
                        rtThumbSprite.anchorMin = new Vector2(0.5f, 0.5f);
                        rtThumbSprite.anchorMax = new Vector2(0.5f, 0.5f);
                        rtThumbSprite.pivot = new Vector2(0.5f, 0.5f);
                        rtThumbSprite.anchoredPosition = Vector2.zero;
                        rtThumbSprite.sizeDelta = thumbSprite.rect.size;
                    }

                    imgSplitter.paneA = this.panes[i];
                    imgSplitter.paneB = this.panes[i + 1];
                    imgSplitter.parent = this;

                    this.sashes.Add(sk, imgSplitter);
                }

                return true;
            }

            /// <summary>
            /// Update all the panes and sashes. This is often done when the splitter's 
            /// dimensions have changed, or a new pane layout is created.
            /// </summary>
            public void UpdateAlignment()
            { 
                if(this.rectTransform == null)
                    return;

                Rect trect = this.rectTransform.rect;
                Vector2 thisDim = trect.size;

                if(this.rectSizes.Count == 0)
                    return;

                // Make sure the assets exist and are proper. 
                this.RemakeSashes();

                if(this.rectSizes.Count == 1)
                { 
                    RectTransform rt = null;
                    foreach(KeyValuePair< RectTransform, float> kvp in this.rectSizes)
                        rt = kvp.Key;

                    rt.anchoredPosition = new Vector2(0.0f, 0.0f);
                    rt.sizeDelta = thisDim;
                    return;
                }
                
                // Set idx to 0 to reference the x component (horizontal) in vectors.
                // Set idx to 1 to reference the y component (vertical) in vectors.
                int idx = this.GetVectorIndex();

                // The component for this splitter container's dimensions that we care about.
                float availTotalSize = thisDim[idx];
                float sashSpace = (this.rectSizes.Count - 1) * this.props.sashDim[idx];
                // all the layout space available excluding that space needed for sashes.
                float availWOSashes = availTotalSize - sashSpace;

                // The total amount needed for the minimum
                float totalMin = this.rectSizes.Count * this.minSize;
                float totalMinWSashes = totalMin + sashSpace;

                // How much extra space can be distributed
                float distribSpace = availWOSashes - totalMin;

                //The total space calculated to currently be taken up on the grain.
                float total = 0.0f;


                // If we don't have enough space to fit to even fit
                // everything and their min size, nothing gets to decide
                // on what portion of excess space (there there is none of)
                // to take. We leave total at 0 so equal space handling will
                // happen.
                if(availWOSashes > totalMin)
                {
                    // The current extra space being used up after the minimum
                    // size constraint is accounted for.
                    float excess = 0.0f;

                    // Figure out how much excess space there is to allocate after
                    // we make sure everything follows the constraint for the 
                    // min size.
                    foreach (RectTransform rtKey in this.panes)
                    {
                        float size = this.rectSizes[rtKey];
                        if(size < this.minSize)
                        { 
                            this.rectSizes[rtKey] = this.minSize;
                            size = this.minSize;
                        }
                        else
                            excess += size - this.minSize; 

                        total += size;
                    }

                    // Divy up the space based off any changes that might have been
                    // made to allow for the min size constraint
                    if (excess > 0.0f)
                    {
                        foreach(RectTransform rtKey in this.panes)
                        { 
                            float sz = this.rectSizes[rtKey];
                            if(sz > this.minSize)
                            {
                                float redistSize = 
                                    this.minSize + (sz - this.minSize) / excess * distribSpace;

                                this.rectSizes[rtKey] = redistSize;
                                    
                            }
                        }
                    }
                }

                if(total == 0.0f)
                { 
                    // Some weird collapsed state, or starting state.
                    // Just make everything equal
                    //
                    // We can't use the minsize, because we're most likely here because
                    // we didn't even have the space for minsize.
                    float avgSize = availWOSashes / total;
                    foreach(RectTransform rt in this.rectSizes.Keys)
                        this.rectSizes[rt] = avgSize;
                }
               
                int other = (idx == 0) ? 1 : 0;
                float sign = 1.0f;

                if(idx == 1)
                    sign = -1.0f;

                RectTransform lastRt = null;
                float incr = 0.0f;

                Vector2 sashDim = Vector2.zero;
                sashDim[idx] = this.props.sashDim[idx];
                sashDim[other] = thisDim[other];

                // After everything is finally calculated and the new correct
                // sizes are cached in this.rectSizes, enumerate through and
                // place everything moving from either left to right, or top
                // to bottom.
                foreach(RectTransform rtKey in this.panes)
                {
                    if(lastRt != null)
                    {
                        UnityEngine.UI.Image sash = sashes[new SashKey(lastRt, rtKey)];

                        Vector2 sashPos = Vector2.zero;
                        sashPos[idx] = sign * incr;

                        sash.rectTransform.anchoredPosition = sashPos;
                        sash.rectTransform.sizeDelta = sashDim;

                        incr += this.props.sashDim[idx];
                    }
                    Vector3 panePos = Vector3.zero;
                    panePos[idx] = sign * incr;
                    rtKey.anchoredPosition = panePos;

                    float paneSigSz = this.rectSizes[rtKey];
                    Vector2 paneDim = Vector3.zero;
                    paneDim[idx] = paneSigSz;
                    paneDim[other] = thisDim[other];
                    rtKey.sizeDelta = paneDim;

                    lastRt = rtKey;
                    incr += paneSigSz;
                }

            }

            /// <summary>
            /// Unity callback.
            /// </summary>
            public void OnRectTransformDimensionsChange()
            { 
                this.UpdateAlignment();
            }

            /// <summary>
            /// Notify the splitter that it should update the cached size
            /// based on the pane's RectTransform's current dimensions.
            /// </summary>
            /// <param name="rt"></param>
            public void UpdateSize(RectTransform rt)
            {
                if (rectSizes.ContainsKey(rt) == false)
                    return;

                rectSizes[rt] = rt.rect.size[this.GetVectorIndex()];
            }
        }
    }
}