using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public abstract class FileBrowserArea : UnityEngine.EventSystems.UIBehaviour
        {
            public abstract void SelectFile(string file, bool append);
            public abstract void SelectFiles(string [] files, bool append);
            public abstract void DeselectFiles();

            public abstract void ViewDirectory(string directory, FileFilter filter);
            public abstract string OnOK();
            public abstract string [] OnOKMulti();

            public virtual void SetFilter(FileFilter ff){ }
        }
    }
}
