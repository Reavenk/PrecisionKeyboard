using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WiringSelectedParamsEditor : EditorWindow
{
    [MenuItem("PxPre/SelectionEditor")]
    static void Initialize()
    { 
        EditorWindow.GetWindow<WiringSelectedParamsEditor>();
    }

    Application app;

    public float pushLowerAmt = 10.0f;
    public Vector2 moveAllVec = Vector2.zero;

    struct IntegrationFilePair
    { 
        public string srcFile;
        public string dstFile;

        public IntegrationFilePair(string srcFile, string dstFile)
        { 
            this.srcFile = srcFile;
            this.dstFile = dstFile;
        }
    }

    private void OnGUI()
    {
        this.app = 
            (Application)EditorGUILayout.ObjectField(
                this.app, 
                typeof(Application), 
                true, 
                GUILayout.ExpandWidth(true));

        if(this.app == null || this.app.Wirings == null || this.app.Wirings.Active == null)
            return;
        
        EditorGUILayout.HelpBox(
            "Controls to help with selected node alignment", 
            MessageType.Info);

        GNUIHost uih = this.app.wiringPane.GetSelectedNode();
        if(uih == null)
        { 
            EditorGUILayout.HelpBox("No selection", MessageType.Warning);
        }
        else
            this.OnGUI_OnHostSelection(uih);

        GUILayout.Space(20.0f);

        EditorGUILayout.HelpBox(
            "Integrate Wirings brings in hard coded locations to replace internal documents", 
            MessageType.Info);

        if(GUILayout.Button("Integrate Wirings") == true)
        {
            List<IntegrationFilePair> toIntegrate =
                new List<IntegrationFilePair>
                { 
                    new IntegrationFilePair("D:\\Repos\\WebKeys\\StartingInstr.phon",   "D:\\Repos\\WebKeys\\Assets\\Wiring\\StartingInstr.txt"),
                    new IntegrationFilePair("D:\\Repos\\WebKeys\\Tutorial.phon",        "D:\\Repos\\WebKeys\\Assets\\Wiring\\Resources\\WiringTutorial.phon.xml")
                };

            foreach(IntegrationFilePair ifp in toIntegrate)
                System.IO.File.Copy(ifp.srcFile, ifp.dstFile, true);
        }

        GUILayout.Space(100.0f);
        GUILayout.Box("Move All Nodes", GUILayout.ExpandWidth(true));
        this.moveAllVec = EditorGUILayout.Vector2Field("Movement", this.moveAllVec, GUILayout.ExpandWidth(true));
        if(GUILayout.Button("Move All", GUILayout.ExpandWidth( true)) == true)
            this.DoMoveAll(this.moveAllVec);
    }

    void OnGUI_OnHostSelection(GNUIHost uih)
    {
        Vector2 orig = uih.slave.cachedUILocation;
        Vector2 newpos = EditorGUILayout.Vector2Field("pos", orig, GUILayout.ExpandWidth(true));
        if(orig != newpos)
        { 
            uih.slave.cachedUILocation = newpos;
            uih.rectTransform.anchoredPosition = newpos;
        }

        foreach(ParamBase pb in uih.slave.nodeParams)
        {
            ParamText ptx = pb as ParamText;
            if(ptx != null)
            { 
                ptx.value = EditorGUILayout.TextField(ptx.name, ptx.value, GUILayout.ExpandWidth(true));
                continue;
            }

            ParamBool pbool = pb as ParamBool;
            if(pbool != null)
            { 
                pbool.value = EditorGUILayout.Toggle(pbool.name, pbool.value, GUILayout.ExpandWidth(true));
                continue;
            }

            ParamFloat pf = pb as ParamFloat;
            if(pf != null)
            { 
                pf.value = EditorGUILayout.FloatField(pf.name, pf.value, GUILayout.ExpandWidth(true));
                continue;
            }

            ParamInt pi = pb as ParamInt;
            if(pi != null)
            { 
                pi.value = EditorGUILayout.IntField(pi.name, pi.value, GUILayout.ExpandWidth(true));
                continue;
            }

            //ParamNickname pn = pb as ParamNickname;
            //if(pn != null)
            //{ 
            //    //continue;
            //}
            //
            //ParamEnum pe = pb as ParamEnum;
            //if(pe != null)
            //{ 
            //
            //    continue;
            //}
            //
            //ParamTimeLen ptm = pb as ParamTimeLen;
            //if(ptm != null)
            //{ 
            //
            //    continue;
            //}
        }

        if(GUILayout.Button("Set X 100", GUILayout.ExpandWidth(true)) == true)
        { 
            uih.slave.cachedUILocation.x = 100.0f;
            uih.rectTransform.anchoredPosition = uih.slave.cachedUILocation;
        }

        if(GUILayout.Button("Set Width 800", GUILayout.ExpandWidth(true)) == true)
        { 
            ParamFloat pf = uih.slave.GetFloatParam(LLDNBase.secretWidthProperty);
            if(pf != null)
                pf.value = 800.0f;
        }

        GUILayout.Space(10.0f);

        GUILayout.BeginHorizontal();
            if(GUILayout.Button("Dock Up", GUILayout.ExpandWidth(true)) == true)
                this.DockUp(uih, this.app.wiringPane._uiNodesForEditor);

            GUILayout.Space(10.0f);

            if(GUILayout.Button("Dock Left", GUILayout.ExpandWidth(true)) == true)
                this.DockLeft(uih, this.app.wiringPane._uiNodesForEditor);

            if(GUILayout.Button("Dock Right", GUILayout.ExpandWidth(true)) == true)
                this.DockRight(uih, this.app.wiringPane._uiNodesForEditor);
        GUILayout.EndHorizontal();
        

        if(GUILayout.Button("Move Down 10", GUILayout.ExpandWidth(true)) == true)
        { 
            uih.slave.cachedUILocation.y -= 10.0f;
            uih.rectTransform.anchoredPosition = uih.slave.cachedUILocation;
        }

        this.pushLowerAmt = 
            EditorGUILayout.FloatField("Push Lower Amt", this.pushLowerAmt, GUILayout.ExpandWidth(true));

        if(GUILayout.Button("Push Lower", GUILayout.ExpandWidth(true)) == true)
        { 
            const float pushLowerFudgeFactor = 5.0f;
            float rejectY = uih.slave.cachedUILocation.y - pushLowerFudgeFactor;

            foreach(GNUIHost h in this.app.wiringPane._uiNodesForEditor)
            { 
                if(h.slave.cachedUILocation.y > rejectY)
                    continue;

                h.slave.cachedUILocation.y -= this.pushLowerAmt;
                h.rectTransform.anchoredPosition = h.slave.cachedUILocation;
            }
        }
    }

    void DockUp(GNUIHost uih, IEnumerable<GNUIHost> allHosts)
    { 
        float xAim = 
            uih.slave.cachedUILocation.x + uih.rectTransform.sizeDelta.x * 0.5f;

        float rejectY = uih.slave.cachedUILocation.y;
        float ? lowestCanidate = null;

        foreach(GNUIHost h in allHosts)
        { 
            if(h == uih)
                continue;

            if(h.slave.cachedUILocation.y < rejectY)
                continue;

            if(
                xAim <= h.slave.cachedUILocation.x || 
                xAim > h.slave.cachedUILocation.x + h.rectTransform.sizeDelta.x)
            {
                continue;
            }

            float bottom = h.slave.cachedUILocation.y - h.rectTransform.sizeDelta.y;
            if(lowestCanidate.HasValue == false || bottom < lowestCanidate.Value)
                lowestCanidate = bottom;
        }
        if(lowestCanidate.HasValue == true)
        {
            uih.slave.cachedUILocation.y = lowestCanidate.Value;
            uih.rectTransform.anchoredPosition = uih.slave.cachedUILocation;
        }
    }

    void DockLeft(GNUIHost uih, IEnumerable<GNUIHost> allHosts)
    { 
        float xAim = 
            uih.slave.cachedUILocation.x + uih.rectTransform.sizeDelta.x * 0.5f;

        float rejectY = uih.slave.cachedUILocation.y;
        float ? lowestCanidate = null;
        float xLeft = 0.0f;

        foreach(GNUIHost h in allHosts)
        { 
            if(h == uih)
                continue;

            if(h.slave.cachedUILocation.y < rejectY)
                continue;

            if(
                xAim <= h.slave.cachedUILocation.x || 
                xAim > h.slave.cachedUILocation.x + h.rectTransform.sizeDelta.x)
            {
                continue;
            }

            float bottom = h.slave.cachedUILocation.y - h.rectTransform.sizeDelta.y;
            if(lowestCanidate.HasValue == false || bottom < lowestCanidate.Value)
            {
                xLeft = h.slave.cachedUILocation.x;
                lowestCanidate = bottom;
            }
        }
        if(lowestCanidate.HasValue == true)
        {
            uih.slave.cachedUILocation.x = xLeft;
            uih.rectTransform.anchoredPosition = uih.slave.cachedUILocation;
        }
    }

    void DockRight(GNUIHost uih, IEnumerable<GNUIHost> allHosts)
    { 
        float xAim = 
            uih.slave.cachedUILocation.x + uih.rectTransform.sizeDelta.x * 0.5f;

        float rejectY = uih.slave.cachedUILocation.y;
        float ? lowestCanidate = null;
        float xRight = 0.0f;

        foreach(GNUIHost h in allHosts)
        { 
            if(h == uih)
                continue;

            if(h.slave.cachedUILocation.y < rejectY)
                continue;

            if(
                xAim <= h.slave.cachedUILocation.x || 
                xAim > h.slave.cachedUILocation.x + h.rectTransform.sizeDelta.x)
            {
                continue;
            }

            float bottom = h.slave.cachedUILocation.y - h.rectTransform.sizeDelta.y;
            if(lowestCanidate.HasValue == false || bottom < lowestCanidate.Value)
            {
                xRight = h.slave.cachedUILocation.x + h.rectTransform.sizeDelta.x;
                lowestCanidate = bottom;
            }
        }
        if(lowestCanidate.HasValue == true)
        {
            uih.slave.cachedUILocation.x = xRight - uih.rectTransform.sizeDelta.x;
            uih.rectTransform.anchoredPosition = uih.slave.cachedUILocation;
        }
    }

    void DoMoveAll(Vector2 vec)
    { 
        foreach( GNUIHost uih in this.app.wiringPane._uiNodesForEditor)
        {
            uih.rectTransform.anchoredPosition += vec;
            uih.slave.cachedUILocation += vec;
        }
    }
}
