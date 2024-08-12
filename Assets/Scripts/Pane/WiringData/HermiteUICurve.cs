using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
public class HermiteUICurve : UnityEngine.UI.MaskableGraphic
{
    int subdivisions = 12;

    public Vector2 pointA = Vector2.zero;
    public Vector2 pointB = Vector2.zero;
    public Vector2 tangentA = Vector2.zero;
    public Vector2 tangentB = Vector2.zero;

    public float rad = 3.0f;
    
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    { 
        vh.Clear();

        List<Vector2> samps = new List<Vector2>();

        Color32 c32 = this.color;

        for(int i = 0; i < subdivisions; ++i)
        { 
            float t = (float)(i + 0) / (float)(subdivisions - 1);
            Vector2 pt = CurveMath.GetHermiteWeightsV2(t, pointA, pointB, tangentA, tangentB);
            samps.Add(pt);
        }
        for(int i = 0; i < subdivisions; ++i)
        { 
            if(i == 0)
            { 
                Vector2 pt1 = samps[0];
                Vector2 pt2 = samps[1];
                Vector2 def = (pt2 - pt1).normalized;
                float tmp = def.y;
                def.y = def.x;
                def.x = -tmp;
                def *= this.rad;

                vh.AddVert(pt1 + def, c32, new Vector2(0.0f, 0.0f));
                vh.AddVert(pt1 - def, c32, new Vector2(0.0f, 1.0f));

            }
            else if( i == subdivisions - 1)
            {
                Vector2 pt1 = samps[i - 1];
                Vector2 pt2 = samps[i];
                Vector2 def = (pt2 - pt1).normalized;
                float tmp = def.y;
                def.y = def.x;
                def.x = -tmp;
                def *= this.rad;

                vh.AddVert(pt2 + def, c32, new Vector2(1.0f, 0.0f));
                vh.AddVert(pt2 - def, c32, new Vector2(1.0f, 1.0f));
            }
            else
            {
                Vector2 pt1 = samps[i - 1];
                Vector2 pt2 = samps[i];
                Vector2 pt3 = samps[i + 1];
                Vector2 def1 = (pt2 - pt1).normalized;
                Vector2 def2 = (pt3 - pt2).normalized;
                Vector2 defAvg = (def1 + def2).normalized;

                float invDot = 1.0f / Vector2.Dot(def1, def2);

                float tmp = defAvg.y;
                defAvg.y = defAvg.x;
                defAvg.x = -tmp;
                defAvg *= this.rad * invDot;

                float u = (float)i / (float)(subdivisions - 1);

                vh.AddVert(pt2 + defAvg, c32, new Vector2(u, 0.0f));
                vh.AddVert(pt2 - defAvg, c32, new Vector2(u, 1.0f));
            }
        }

        int vertPasses = (subdivisions - 1) * 2;
        for (int i = 0; i < vertPasses; i += 2)
        {
            vh.AddTriangle(i, i + 2, i + 3);
            vh.AddTriangle(i, i + 3, i + 1);
        }
    }
}
