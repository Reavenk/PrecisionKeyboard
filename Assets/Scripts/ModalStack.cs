using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalStack : MonoBehaviour
{
    public static List<GameObject> stack = 
        new List<GameObject>();

    public System.Action onPop;

    public static void CollapseStack()
    { 
        List<GameObject> gos = stack;
        stack = new List<GameObject>();

        foreach(GameObject go in gos)
            GameObject.Destroy(go);
    }

    public static void AddToStack(GameObject go, System.Action onPop = null)
    { 
        ModalStack ms = go.AddComponent<ModalStack>();
        ms.onPop = onPop;
    }

    public static bool Pop()
    { 
        if(stack.Count == 0)
            return false;

        int rmIdx = stack.Count - 1;
        GameObject back = stack[rmIdx];
        stack.RemoveAt(rmIdx);

        if(back != null)
        { 
            ModalStack ms = back.GetComponent<ModalStack>();
            if( ms != null)
                ms.onPop?.Invoke();
        }

        GameObject.Destroy(back);
        return true;
    }

    public void Awake()
    {
        UnityEngine.EventSystems.EventSystem cur = UnityEngine.EventSystems.EventSystem.current;
        cur.SetSelectedGameObject(null);

        stack.Add(this.gameObject);        
    }

    private void OnDestroy()
    {
        // While there really should only ever be 1 entry,
        // RemoveAll shouldn't add much overhead and should
        // cover our bases in case the stack is misused and
        // dupes are added.
        stack.RemoveAll((x)=>{ return x == this.gameObject;});
    }
}
