using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasSingleton : MonoBehaviour
{
    public static Canvas canvas;
    private void Awake()
    {
        canvas = this.GetComponent<Canvas>();
    }
}
