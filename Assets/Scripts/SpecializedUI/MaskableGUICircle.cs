using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskableGUICircle : UnityEngine.UI.MaskableGraphic
{
    public float radius = 10.0f;
    public float fade = 2.0f;
    public int segments = 12;

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    { 
        vh.Clear();

        vh.AddVert(Vector3.zero, this.color, Vector2.zero);

        int seg = Mathf.Max(this.segments, 3);
        float fseg = (float)seg;
        float ext = this.radius + this.fade;
        for (int i = 0; i < seg; ++i)
        { 
            float lam = (float)i / fseg;
            float rad = Mathf.PI * 2.0f * lam;

            float fx = Mathf.Cos(rad);
            float fy = Mathf.Sin(rad);

            vh.AddVert(
                new Vector3(fx * this.radius, fy * this.radius, 0.0f), 
                this.color, 
                Vector2.zero);
        }

        Color cExt = this.color;
        cExt.a = 0.0f;
        // A second pass that uses the extent (radius + fade)
        for (int i = 0; i < seg; ++i)
        {
            float lam = (float)i / fseg;
            float rad = Mathf.PI * 2.0f * lam;

            float fx = Mathf.Cos(rad);
            float fy = Mathf.Sin(rad);

            vh.AddVert(
                new Vector3(fx * ext, fy * ext, 0.0f),
                cExt,
                Vector2.zero);
        }

        // Add the inner triangle
        for (int i = 0; i < seg; ++i)
        { 
            vh.AddTriangle(
                0, 
                i + 1,  
                (i  + 1) % seg + 1);
        }

        // Add fade extent
        for (int i = 0; i < seg; ++i)
        {
            int i1 = i + 1;
            int i2 = (i + 1) % seg + 1;
            int o1 = i1 + seg;
            int o2 = i2 + seg;

            vh.AddTriangle(i1, o1, o2);
            vh.AddTriangle(i1, o2, i2);
        }
    }
}
