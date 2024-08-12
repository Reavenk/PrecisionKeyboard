using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        [RequireComponent(typeof(FileBrowseSpawner))]
        public class FileBrowseSingleton : MonoBehaviour
        {
            static FileBrowseSpawner spawner;

            public static FileBrowseSpawner Spawner 
            {get{return spawner; } }

            private void Awake()
            {
                spawner = this.GetComponent<FileBrowseSpawner>();
            }
        }
    }
}
