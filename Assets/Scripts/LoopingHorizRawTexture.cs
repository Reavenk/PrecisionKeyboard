using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingHorizRawTexture : UnityEngine.UI.RawImage
{
    public float minVal = 0.0f;
    public float maxVal = 1.0f;

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    {
        vh.Clear();
    }
}
