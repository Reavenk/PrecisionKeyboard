using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace DropMenu
    {
        [RequireComponent(typeof(DropMenuSpawner))]
        public class DropMenuSingleton : MonoBehaviour
        {
            static DropMenuSingleton instance;
            public static DropMenuSingleton Instance {get{return instance; } }

            public static DropMenuSpawner MenuInst
            { get{return Instance.CachedMenu; } }

            private DropMenuSpawner menu;
            public DropMenuSpawner CachedMenu { get{return this.menu; } }

            private void Awake()
            {
                instance = this;
                this.menu = this.GetComponent<DropMenuSpawner>();
            }
        }
    }
}