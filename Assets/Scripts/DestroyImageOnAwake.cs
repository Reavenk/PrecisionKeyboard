using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyImageOnAwake : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.UI.Image img = this.GetComponent<UnityEngine.UI.Image>();

        if(img != null)
            GameObject.Destroy(img);
    }
}
