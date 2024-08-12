using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RTQuick
{
    public RectTransform rt;

    public static RTQuick CreateGameObjectWithImage(Transform parent, string name, out UnityEngine.UI.Image img)
    {
        img = CreateGameObjectGraphic<UnityEngine.UI.Image>(parent, name);
        return new RTQuick(img);
    }

    public static RTQuick CreateGameObjectWithImage(UnityEngine.UI.Graphic parent, string name, out UnityEngine.UI.Image img)
    {
        return CreateGameObjectWithImage(parent.transform, name, out img);
    }

    public static RTQuick CreateGameObjectWithText(Transform parent, string name, out UnityEngine.UI.Text txt)
    { 
        txt = CreateGameObjectGraphic<UnityEngine.UI.Text>(parent, name);
        return new RTQuick(txt);
    }

    public static RTQuick CreateGameObjectWithText(UnityEngine.UI.Graphic parent, string name, out UnityEngine.UI.Text txt)
    { 
        return CreateGameObjectWithText(parent.transform, name, out txt);
    }

    public void GetBehaviour<ty>(out ty o) where ty : Behaviour
    { 
        o = this.rt.GetComponent<ty>();
    }

    public void AddBehaviour<ty>(out ty o) where ty : Behaviour
    {
        o = this.rt.gameObject.AddComponent<ty>();
    }

    public void GetOrAddBehaviour<ty>(out ty o) where ty : Behaviour
    { 
        o = this.rt.GetComponent<ty>();

        if(o == null)
            this.rt.gameObject.AddComponent<ty>();
    }

    public static ty CreateGameObjectGraphic<ty>(Transform parent, string name)
        where ty : UnityEngine.UI.Graphic
    { 
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;

        return go.AddComponent<ty>();
    }

    public static ty CreateGameObjectGraphic<ty>(UnityEngine.UI.Graphic parent, string name)
        where ty : UnityEngine.UI.Graphic
    { 
        return CreateGameObjectGraphic<ty>(parent.transform, name);
    }

    public RTQuick(RectTransform rt)
    { 
        this.rt = rt;
    }

    public RTQuick(GameObject go)
    { 
        this.rt = go.GetComponent<RectTransform>();

        if(this.rt == null)
            this.rt = go.AddComponent<RectTransform>();
    }

    public RTQuick(UnityEngine.UI.Graphic g)
    { 
        this.rt = g.rectTransform;
    }

    public RTQuick ZeroOffsets()
    {
        this.rt.offsetMin = Vector2.zero;
        this.rt.offsetMax = Vector2.zero;

        return this;
    }

    public RTQuick TopLeftAnchors()
    { 
        this.rt.anchorMin = new Vector2(0.0f, 1.0f);
        this.rt.anchorMax = new Vector2(0.0f, 1.0f);

        return this;
    }

    public RTQuick BotLeftAnchors()
    {
        this.rt.anchorMin = new Vector2(0.0f, 0.0f);
        this.rt.anchorMax = new Vector2(0.0f, 0.0f);

        return this;
    }

    public RTQuick TopLeftPivot()
    { 
        this.rt.pivot = new Vector2(0.0f, 1.0f);

        return this;
    }

    public RTQuick TopLeftAnchorsPivot()
    {
        this.rt.anchorMin = new Vector2(0.0f, 1.0f);
        this.rt.anchorMax = new Vector2(0.0f, 1.0f);

        this.rt.pivot = new Vector2(0.0f, 1.0f);

        return this;
    }

    public RTQuick Pivot( Vector2 v)
    { 
        this.rt.pivot = v;

        return this;
    }

    public RTQuick Pivot(float x, float y)
    {
        this.rt.pivot = new Vector2(x, y);

        return this;
    }

    public RTQuick CenterPivot()
    { 
        this.rt.pivot = new Vector2(0.5f, 0.5f);

        return this;
    }

    public RTQuick ZeroAnchors()
    { 
        this.rt.anchorMin = Vector2.zero;
        this.rt.anchorMax = Vector2.zero;

        return this;
    }

    public RTQuick CenterAnchors()
    { 
        this.rt.anchorMin = new Vector2(0.5f, 0.5f);
        this.rt.anchorMax = new Vector2(0.5f, 0.5f);

        return this;
    }

    public RTQuick CenterAnchorsPivot()
    {
        this.rt.anchorMin = new Vector2(0.5f, 0.5f);
        this.rt.anchorMax = new Vector2(0.5f, 0.5f);

        this.rt.pivot = new Vector2(0.5f, 0.5f);

        return this;
    }

    public RTQuick ExpandParentFlush()
    { 
        this.rt.anchorMin = Vector2.zero;
        this.rt.anchorMax = Vector2.one;
        this.rt.offsetMin = Vector2.zero;
        this.rt.offsetMax = Vector2.zero;

        return this;
    }

    public RTQuick ExpandAnchors()
    { 
        this.rt.anchorMin = Vector2.zero;
        this.rt.anchorMax = Vector2.one;

        return this;
    }

    public RTQuick SizeDelta(float x, float y)
    { 
        this.rt.sizeDelta = new Vector2(x, y);

        return this;
    }

    public RTQuick SizeDelta(Vector2 v)
    {
        this.rt.sizeDelta = v;

        return this;
    }

    public RTQuick OffsetMin(Vector2 o)
    { 
        this.rt.offsetMin = o;
        return this;
    }

    public RTQuick OffsetMin(float x, float y)
    { 
        this.rt.offsetMin = new Vector2(x, y);
        return this;
    }

    public RTQuick OffsetMax(Vector2 o)
    { 
        this.rt.offsetMax = o;
        return this;
    }

    public RTQuick OffsetMax(float x, float y)
    { 
        this.rt.offsetMax = new Vector2(x, y);
        return this;
    }

    public RTQuick AnchPos(float x, float y)
    { 
        this.rt.anchoredPosition = new Vector2(x, y);
        return this;
    }

    public RTQuick AnchPos(Vector2 v)
    { 
        this.rt.anchoredPosition = v;
        return this;
    }

    public RTQuick AnchMin(float x, float y)
    { 
        this.rt.anchorMin = new Vector2(x, y);

        return this;
    }

    public RTQuick AnchMin(Vector2 v)
    {
        this.rt.anchorMin = v;

        return this;
    }

    public RTQuick AnchMax(float x, float y)
    {
        this.rt.anchorMax = new Vector2(x, y);

        return this;
    }

    public RTQuick AnchMax(Vector2 v)
    {
        this.rt.anchorMax = v;

        return this;
    }

    public RTQuick Identity()
    { 
        this.rt.localScale = Vector3.one;
        this.rt.localPosition = Vector3.zero;
        this.rt.localRotation = Quaternion.identity;

        return this;
    }
}

public static class RTQuickUtil
{ 
    public static UnityEngine.UI.Graphic Color(this UnityEngine.UI.Graphic gr, float r, float g, float b, float a)
    { 
        gr.color = new Color(r,g,b,a);
        return gr;
    }

    public static UnityEngine.UI.Graphic Color(this UnityEngine.UI.Graphic gr, float r, float g, float b)
    { 
        gr.color = new Color(r,g,b);
        return gr;
    }

    public static UnityEngine.UI.Graphic Color(this UnityEngine.UI.Graphic gr, Color c)
    { 
        gr.color = c;
        return gr;
    }

    public static UnityEngine.UI.Image Sprite(this UnityEngine.UI.Image img, Sprite sprite)
    { 
        img.sprite = sprite;
        return img;
    }

    public static UnityEngine.UI.Image ImageType(this UnityEngine.UI.Image img, UnityEngine.UI.Image.Type type)
    { 
        img.type = type;
        return img;
    }

    public static RTQuick RTQ(this RectTransform rt)
    { 
        return new RTQuick(rt);
    }

    public static RTQuick RTQ(this UnityEngine.UI.Graphic gr)
    { 
        return new RTQuick(gr);
    }

    public static RTQuick RTQ(this GameObject go)
    { 
        RectTransform rt = go.GetComponent<RectTransform>();

        if(rt == null)
            rt = go.AddComponent<RectTransform>();

        return new RTQuick(rt);
    }
}

