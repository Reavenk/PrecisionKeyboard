// TODO: Check if can be deleted.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DlgMessage : MonoBehaviour
{
    public UnityEngine.UI.Text title;
    public UnityEngine.UI.Text message;
    public UnityEngine.UI.Text closeText;

    public System.Action onClose = null;

    public void OnButton_Close()
    {
        GameObject.Destroy(this.gameObject);

        if(this.onClose != null)
            this.onClose();
    }
}
