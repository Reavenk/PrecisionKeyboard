using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public class FileBrowseSpawner : MonoBehaviour
        {
            public FileBrowseProp properties;
            /*
            virtual RectTransform CreatePath()
            { 
            }

            virtual RectTransform CreateBreadcrumb()
            { 
            }

            virtual RectTransform CreateFileBrowseRegion()
            {
            }
            */

            public FileBrowser CreateFileBrowser(RectTransform parent, string extensions, bool saving)
            { 
                GameObject go = new GameObject("FileBrowser");
                go.transform.SetParent(parent);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                FileBrowser browser = go.AddComponent<FileBrowser>();
                browser.Create(this, this.properties, "", extensions, saving);
                return browser;
            }

            // The second parameter is the new full path
            public System.Action<FileBrowser.NavigationType, string> onNavigationEvent;
        }
    }
}