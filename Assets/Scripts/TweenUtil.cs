using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenUtil
{
    [System.Flags]
    public enum RestoreMode
    { 
        NoRestore               = 0,
        RestoreWorld            = 1 << 1,
        RestoreLocal            = 1 << 2,
        RestoreOffsets          = 1 << 3,
        RestoreAnchorAndDelta   = 1 << 4,
        Delete                  = 1 << 5,
        Alpha                   = 1 << 6
    }

    public struct AnimEffectEntry
    {
        public RectTransform rt;

        public Vector3      originalLocPos;
        public Quaternion   originalLocRot;
        public Vector3      originalLocScale;

        public Vector3      originalWorldPos;
        public Quaternion   originalWorldRot;

        public Vector2      origAnchorMin;
        public Vector2      origAnchorMax;
        public Vector2      origOffsetMin;
        public Vector2      origOffsetMax;
        public Vector2      origAnchoredPos;
        public Vector2      origSizeDelta;
        public Vector2      origPivot;

        public UnityEngine.UI.Graphic graphic;
        public float        origAlpha;

        public Coroutine coroutine;
        public RestoreMode restoreAfter;

        public AnimEffectEntry(RectTransform rt)
        { 
            this.rt = rt;
            this.originalLocPos = rt.localPosition;
            this.originalLocRot = rt.localRotation;
            this.originalLocScale = rt.localScale;

            this.originalWorldPos = rt.position;
            this.originalWorldRot = rt.rotation;

            this.origAnchorMin  = rt.anchorMin;
            this.origAnchorMax  = rt.anchorMax;
            this.origOffsetMin  = rt.offsetMin;
            this.origOffsetMax  = rt.offsetMax;
            this.origAnchoredPos= rt.anchoredPosition;
            this.origSizeDelta  = rt.sizeDelta;
            this.origPivot      = rt.pivot;

            this.graphic        = null;
            this.origAlpha      = 1.0f;

            this.restoreAfter = RestoreMode.RestoreLocal;
            this.coroutine = null;
        }

        public AnimEffectEntry(RectTransform rt, UnityEngine.UI.Graphic g, RestoreMode restore)
        { 
            this.rt = rt;
            this.originalLocPos = rt.localPosition;
            this.originalLocRot = rt.localRotation;
            this.originalLocScale = rt.localScale;

            this.originalWorldPos = rt.position;
            this.originalWorldRot = rt.rotation;

            this.origAnchorMin  = rt.anchorMin;
            this.origAnchorMax  = rt.anchorMax;
            this.origOffsetMin  = rt.offsetMin;
            this.origOffsetMax  = rt.offsetMax;
            this.origAnchoredPos= rt.anchoredPosition;
            this.origSizeDelta  = rt.sizeDelta;
            this.origPivot      = rt.pivot;

            this.graphic        = g;
            this.origAlpha      = (g != null) ? g.color.a : 1.0f;

            this.restoreAfter = restore;
            this.coroutine = null;
        }

        public bool Restore()
        {
            if (rt != null || rt.gameObject != null)
            {
                if((this.restoreAfter & RestoreMode.RestoreLocal) != 0)
                {
                    rt.localPosition = this.originalLocPos;
                    rt.localRotation = this.originalLocRot;
                    rt.localScale = this.originalLocScale;
                }
                else if((this.restoreAfter & RestoreMode.RestoreWorld) != 0)
                {
                    rt.position = this.originalWorldPos;
                    rt.rotation = this.originalWorldRot;
                    rt.localScale = this.originalLocScale; // Not much we can do about world scale
                }
                else if((this.restoreAfter & RestoreMode.RestoreOffsets) != 0)
                { 
                    rt.pivot = this.origPivot;
                    rt.anchorMin = this.origAnchorMin;
                    rt.anchorMax = this.origAnchorMax;
                    rt.offsetMin = this.origOffsetMin;
                    rt.offsetMin = this.origOffsetMax;
                }
                else if((this.restoreAfter & RestoreMode.RestoreAnchorAndDelta) != 0)
                {
                    rt.pivot = this.origPivot;
                    rt.anchorMin = this.origAnchorMin;
                    rt.anchorMax = this.origAnchorMax;
                    rt.anchoredPosition = this.origAnchoredPos;
                    rt.sizeDelta = this.origSizeDelta;
                }

                if((this.restoreAfter & RestoreMode.Alpha) != 0)
                { 
                    if(this.graphic != null)
                    { 
                        Color c = this.graphic.color;
                        this.graphic.color = new Color(c.r, c.g, c.b, this.origAlpha);
                    }
                }

                if ((this.restoreAfter & RestoreMode.Delete) != 0)
                {
                    GameObject.Destroy(rt.gameObject);
                    return false;
                }
            }
            return true;
        }
    }

    MonoBehaviour host;
    public System.Action<AnimEffectEntry> onStarted;
    public System.Action<AnimEffectEntry> onEnded;

    Dictionary<RectTransform, AnimEffectEntry> entries = new Dictionary<RectTransform, AnimEffectEntry>();
    delegate IEnumerator CoroutineFunction(RectTransform rt);

    public MonoBehaviour Host {get{return this.host; } }

    public TweenUtil(MonoBehaviour host)
    { 
        this.host = host;
    }

    void AddEntry(RectTransform rt, CoroutineFunction fn, RestoreMode restoreAfter = RestoreMode.RestoreLocal)
    { 
        if(this.RestoreEntry(rt) == false)
            return;

        AnimEffectEntry aee = new AnimEffectEntry(rt);
        aee.restoreAfter = restoreAfter;
        aee.coroutine = this.host.StartCoroutine(HostCoroutine(rt, fn));

        this.entries.Add(rt, aee);
        onStarted?.Invoke(aee);
    }

    void AddEntry(RectTransform rt, UnityEngine.UI.Graphic graphic, CoroutineFunction fn, RestoreMode restoreAfter = RestoreMode.RestoreLocal)
    { 
        if(this.RestoreEntry(rt) == false)
            return;

        AnimEffectEntry aee = new AnimEffectEntry(rt, graphic, restoreAfter);
        aee.restoreAfter = restoreAfter;
        aee.coroutine = this.host.StartCoroutine(HostCoroutine(rt, fn));

        this.entries.Add(rt, aee);
        onStarted?.Invoke(aee);
    }

    IEnumerator HostCoroutine(RectTransform rt, CoroutineFunction cf)
    {
        Coroutine cr = this.host.StartCoroutine(cf(rt));

        // This isn't registered or a formal entry, but used for state keeping
        // so incase HostCoroutine is interupted, the thing it's hosting has a chance
        // to be known about and stopped from any other listening managers.
        AnimEffectEntry aeeProxy = new AnimEffectEntry(rt);
        aeeProxy.coroutine = cr;
        this.onStarted?.Invoke(aeeProxy);

        yield return cr;
        this.onEnded?.Invoke(aeeProxy);
        
        this.RestoreEntry(rt);
    }

    ////////////////////////////////////////////////////////////////////////////////

    public void WobbleScale(
        RectTransform rt, 
        float scaleMin, 
        float scaleMax, 
        float duration, 
        float speed, 
        RestoreMode restoreAfter = RestoreMode.RestoreLocal)
    {
        this.AddEntry(
            rt, 
            (x)=>
            { 
                return this.WobbleScaleEnum(
                    x, 
                    rt.localScale, 
                    scaleMin, 
                    scaleMax, 
                    duration, 
                    speed);
            },
            restoreAfter);
    }

    IEnumerator WobbleScaleEnum(
        RectTransform rt, 
        Vector3 origScale, 
        float scaleMin, 
        float scaleMax, 
        float duration, 
        float speed)
    { 
        float startTime = Time.time;
        
        while(Time.time < startTime + duration)
        { 
            float timePassed = Time.time - startTime;
            float lambda = Mathf.Sin(timePassed * 2.0f * Mathf.PI * speed) * 0.5f + 0.5f;

            rt.localScale = origScale * Mathf.Lerp(scaleMin, scaleMax, lambda);

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    public void ScaleIn(RectTransform rt, float duration, RestoreMode restoreAfter = RestoreMode.RestoreLocal)
    {
        this.AddEntry(rt, (x) => { return this.ScaleIn(x, rt.localScale, duration); }, restoreAfter);
    }

    IEnumerator ScaleIn(RectTransform rt, Vector3 origScale, float duration)
    { 
        float startTime = Time.time;

        while(Time.time < startTime + duration)
        { 
            float timePassed = Time.time - startTime;
            float lambda = timePassed / duration;

            rt.localScale = Vector3.Lerp(Vector3.zero, origScale, lambda);
            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    public void SlidingAnchorFade(RectTransform rt, UnityEngine.UI.Graphic graphic, Vector2 offset, bool alphaIn, bool moveIn, float duration, RestoreMode restoreAfter = RestoreMode.RestoreLocal&RestoreMode.Alpha)
    {
        this.AddEntry(
            rt, 
            graphic,
            (x) => 
            { 
                return this.SlidingAnchorFade(
                    x, 
                    graphic, 
                    rt.anchoredPosition, 
                    offset, 
                    alphaIn, 
                    moveIn, 
                    duration); 
            },
            restoreAfter);
    }

    IEnumerator SlidingAnchorFade(RectTransform rt, UnityEngine.UI.Graphic graphic, Vector2 origAnchor, Vector2 offset, bool alphaIn, bool moveIn, float duration)
    { 
        float startTime = Time.time;

        float startingAlpha = 1.0f;
        if(graphic != null)
            startingAlpha = graphic.color.a;

        Vector2 startingPos;
        Vector2 endingPos;
        if(moveIn == true)
        { 
            startingPos = origAnchor - offset;
            endingPos = origAnchor;
        }
        else
        {
            startingPos = origAnchor;
            endingPos = origAnchor + offset;
        }

        float startAlpha;
        float endAlpha;
        if(alphaIn == true)
        {
            startAlpha = 0.0f;
            endAlpha = 1.0f;
        }
        else
        { 
            startAlpha = 1.0f;
            endAlpha = 0.0f;
        }

        while (Time.time < startTime + duration)
        {
            float timePassed = Time.time - startTime;
            float lambda = timePassed / duration;

            if(rt != null)
            {
                rt.anchoredPosition = 
                    Vector2.Lerp(startingPos, endingPos, lambda);
            }

            if(graphic != null)
            { 
                Color c = graphic.color;
                c.a = Mathf.Lerp(startAlpha, endAlpha, lambda);

                graphic.color = c;
            }

            yield return null;
        }

        if (rt != null)
            rt.anchoredPosition = endingPos;

        if (graphic != null)
        {
            Color c = graphic.color;
            c.a = endAlpha * startingAlpha;
            graphic.color = c;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    public void RectTransformLerpOffsets(
        RectTransform rt, 
        Vector2 ? newAnchorPos, 
        Vector2 ? newAnchorMin,
        Vector2 ? newAnchorMax,
        Vector2 ? newOffsetMin, 
        Vector2 ? newOffsetMax, 
        float duration,
        RestoreMode restoreAfter = RestoreMode.RestoreLocal)
    {
        this.AddEntry(
            rt,
            (x) =>
            {
                return this.RectTransformLerpOffsetsEnum(
                    rt,
                    newAnchorPos,
                    newAnchorMin,
                    newAnchorMax,
                    newOffsetMin,
                    newOffsetMax,
                    duration);
            },
            restoreAfter);
    }

    protected IEnumerator RectTransformLerpOffsetsEnum(
        RectTransform rt, 
        Vector2? newAnchorPos,
        Vector2? newAnchorMin,
        Vector2? newAnchorMax,
        Vector2? newOffsetMin,
        Vector2? newOffsetMax,
        float duration)
    { 
        Vector2 origAnchorPos = rt.anchoredPosition;
        Vector2 origAnchorMin = rt.anchorMin;
        Vector2 origAnchorMax = rt.anchorMax;
        Vector2 origOffsetMin = rt.offsetMin;
        Vector2 origOffsetMax = rt.offsetMax;

        float startTime = Time.time;
        while(Time.time < startTime + duration)
        {
            float timePassed = Time.time - startTime;
            float lambda = timePassed / duration;

            if(newAnchorPos.HasValue == true)
                rt.anchoredPosition = Vector2.Lerp( origAnchorPos, newAnchorPos.Value, lambda);

            if(newAnchorMin.HasValue == true)
                rt.anchorMin = Vector2.Lerp( origAnchorMin, newAnchorMin.Value, lambda);

            if(newAnchorMax.HasValue == true)
                rt.anchorMax = Vector2.Lerp( origAnchorMax, newAnchorMax.Value, lambda);

            if(newOffsetMin.HasValue == true)
                rt.offsetMin = Vector2.Lerp(origOffsetMin, newOffsetMin.Value, lambda);

            if(newOffsetMax.HasValue == true)
                rt.offsetMax = Vector2.Lerp(origOffsetMax, newOffsetMax.Value, lambda);

            yield return null;
        }

        if (newAnchorPos.HasValue == true)
            rt.anchoredPosition = newAnchorPos.Value;

        if (newAnchorMin.HasValue == true)
            rt.anchorMin = newAnchorMin.Value;

        if (newAnchorMax.HasValue == true)
            rt.anchorMax = newAnchorMax.Value;

        if (newOffsetMin.HasValue == true)
            rt.offsetMin = newOffsetMin.Value;

        if (newOffsetMax.HasValue == true)
            rt.offsetMax = newOffsetMax.Value;
    }

    ////////////////////////////////////////////////////////////////////////////////

    public bool RestoreEntry(RectTransform rt)
    {
        AnimEffectEntry aee;
        if (this.entries.TryGetValue(rt, out aee) == false)
            return true;

        bool ret = aee.Restore();

        this.onEnded?.Invoke(aee);
        this.host.StopCoroutine(aee.coroutine);
        this.entries.Remove(rt);

        return ret;
    }

    public void Clear(bool restore)
    { 
        if(restore == true)
        { 
            foreach(KeyValuePair<RectTransform, AnimEffectEntry> kvp in this.entries)
                kvp.Value.Restore();
        }
        this.entries.Clear();
    }
}
