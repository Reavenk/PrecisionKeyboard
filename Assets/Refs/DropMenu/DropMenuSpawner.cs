using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace DropMenu
    {
        public class DropMenuSpawner : MonoBehaviour
        {
            public Props props;

            public System.Action<Node> onAction;
            //public System.Action<DropdownCreationReturn> onCreate;

            public System.Action<UnityEngine.UI.Image> onCreatedModalPlate;
            public System.Action<SpawnContext, SpawnContext.NodeContext> onSubMenuOpened;

            public RectTransform CreateMenu(Canvas canvas, Vector2 pos)
            { 
                return null;
            }

            public SpawnContext CreateDropdownMenu(Canvas canvas, Node rootNode, RectTransform rtInvokingRect)
            {
                SpawnContext sc = new SpawnContext(canvas, this);
                sc.CreateDropdownMenu(rootNode, rtInvokingRect);
                return sc;
            }

            private SpawnContext CreateDropdownMenu(Canvas canvas, Node rootNode, Vector3 loc)
            { 
                SpawnContext sc = new SpawnContext(canvas, this);
                sc.CreateDropdownMenu(rootNode, false, loc);
                return sc;
            }
        }
    }
}