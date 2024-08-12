using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuizProgressRecord
{
    public UnityEngine.UI.Image plate;
    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Text incrementAnimText;

    public void Activate(bool toggle = true)
    { 
        this.plate.gameObject.SetActive(toggle);
    }

    public void Deactivate()
    { 
        this.Activate(false);
    }

    public void Reset()
    { 
        this.plate.transform.localScale = Vector3.one;
        this.icon.transform.localScale = Vector3.one;
        this.text.transform.localScale = Vector3.one;

        this.incrementAnimText.rectTransform.anchoredPosition = new Vector2(-30.0f, 0.0f);
        this.incrementAnimText.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }
}