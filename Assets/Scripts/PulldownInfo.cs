using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulldownInfo : MonoBehaviour
{
    /// <summary>
    /// A cached version of the Application in case we want chevron pulldown to automatically
    /// be set up.
    /// </summary>
    public Application appRef;

    /// <summary>
    /// Not used by any effects, but chances are a pulldown will involve a menu or popup which
    /// will want to know the RectTransform of the invoking button.
    /// </summary>
    public RectTransform rootPlate;

    /// <summary>
    /// The icon to perform a dropdown effect if it brings up a dropdown menu and animates the
    /// result being set afterwards.
    /// </summary>
    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Image pulldownChevy;
    public UnityEngine.UI.Button button;

    public bool autoInstallChevyEffect = true;

    private void Start()
    {
        if(
            this.autoInstallChevyEffect == true && 
            this.pulldownChevy != null && 
            appRef != null)
        { 
            UnityEngine.UI.Button btn = this.GetComponent<UnityEngine.UI.Button>();
            if(btn != null)
                btn.onClick.AddListener( ()=>{ this.DoChevyDown(this.appRef); });
        }
    }

    public bool SetInfo(Application app, Sprite newSprite, bool checkFirst)
    { 
        if(app == null)
            app = this.appRef;

        if(this.icon == null)
            return false;

        if(checkFirst == true && icon.sprite == newSprite)
            return false;

        this.icon.sprite = newSprite;
        app.DoDropdownIconUpdate(this.icon);

        return true;

    }

    public bool SetInfo(Application app, string newText, bool checkFirst)
    {
        if (app == null)
            app = this.appRef;

        if (this.text == null)
            return false;

        if(checkFirst == true && this.text.text == newText)
            return false;

        this.text.text = newText;
        app.DoDropdownTextUpdate(this.text);
        return true;
    }

    public bool SetInfo(Application app, Sprite newSprite, string newText, bool checkSpriteFirst)
    {
        if (app == null)
            app = this.appRef;

        if (checkSpriteFirst == true && this.icon != null && this.icon.sprite == newSprite)
            return false;

        if(this.icon != null)
        { 
            this.icon.sprite = newSprite;
            app.DoDropdownIconUpdate(this.icon);
        }

        if(this.text != null)
        { 
            this.text.text = newText;
            app.DoDropdownTextUpdate(this.text);
        }

        return true;
    }

    public void SetInfoAnimate(Application app)
    {
        if (app == null)
            app = this.appRef;

        if (this.icon != null)
            app.DoDropdownIconUpdate(this.icon);

        if (this.text != null)
            app.DoDropdownTextUpdate(this.text);

    }

    public static void DoChevyDown(TweenUtil tu, UnityEngine.UI.Image pulldownChevy)
    { 
        // Chances are even if we move the graphic, it won't be that visible because a menu could
        // spawn in front of it. So instead of doing any kind of juggling, we just create a temporary
        // throw-away duplicate in front of it all.
        GameObject goChevy = new GameObject();
        // Match a duplicate exactly
        goChevy.transform.SetParent(pulldownChevy.transform.parent, false);
        UnityEngine.UI.Image img = goChevy.AddComponent<UnityEngine.UI.Image>();
        img.sprite = pulldownChevy.sprite;
        img.type = pulldownChevy.type;
        img.color = pulldownChevy.color;
        RectTransform rtDst = img.rectTransform;
        RectTransform rtSrc = pulldownChevy.rectTransform;
        rtDst.anchoredPosition  = rtSrc.anchoredPosition;
        rtDst.sizeDelta         = rtSrc.sizeDelta;
        rtDst.pivot             = rtSrc.pivot;
        rtDst.anchorMin         = rtSrc.anchorMin;
        rtDst.anchorMax         = rtSrc.anchorMax;
        rtDst.localScale        = rtSrc.localScale;
        
        rtDst.SetParent( CanvasSingleton.canvas.transform, true);
        
        Application.DoChevyIconEffect(tu, img, true);

        // Start the tweening of the source after so it's no tampered with when copying its
        // properties.
        tu.SlidingAnchorFade(
            pulldownChevy.rectTransform, 
            pulldownChevy, 
            new Vector2(0.0f, -20.0f), 
            true, 
            true, 
            0.3f, 
            TweenUtil.RestoreMode.RestoreLocal);
    }

    public void DoChevyDown(Application app)
    {
        if (app == null)
            app = this.appRef;

        if (this.pulldownChevy == null)
            return;

        DoChevyDown(app.uiFXTweener, this.pulldownChevy);
    }
}
