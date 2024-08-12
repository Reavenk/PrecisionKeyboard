using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public class FileBrowser : MonoBehaviour
        {
            public enum NavigationType
            { 
                UpDirectory,
                BackHistory,
                ForwardHistory,
                Breadcrumb,
                AddressBar,
                SelectBrowser,
                Misc
            }

            public struct Extension
            { 
                public string formatName;
                public List<string> extensions;
            }

            public FileBrowseProp properties;

            public FileBrowseFileListRgn region;
            public FileBrowserPathRgn pathRgn;
            public FileBrowseSpawner spawner;

            List<FileBrowserArea> browserAreas = new List<FileBrowserArea>();

            bool singleClickFolder = true;
            bool singleClickFile = true;

            bool multifile = false;

            public Stack<string> directoryHistory = new Stack<string>();
            public Stack<string> directoryRedoHistory = new Stack<string>();

            public System.Action onCancel;
            public System.Action closeFn;
            public System.Action<string> onOK;
            public System.Action<string[]> onOKMulti;

            private string curDirectory = string.Empty;
            public string CurDirectory {get{return this.curDirectory; } }

            public bool mustExist = false;
            public bool saving = false;

            FileFilter [] fileFilters;
            int filterIndex = -1;

            public System.Action<NavigationType, string> onViewDirectory;

            public int BackwardsHistoryCount()
            { 
                return this.directoryHistory.Count;
            }

            public int ForwardsHistoryCount()
            { 
                return this.directoryRedoHistory.Count;
            }

            public int FilterIndex()
            { 
                return this.filterIndex;
            }

            public void SetFilter(FileFilter [] fileFilters, int index = 0)
            { 
            }

            public void SetFilter(string filterString, int index = 0)
            { 
                FileFilter [] filters = FileFilter.ParseFilterString(filterString);
                this.SetFilter(filters, index);
            }

            public bool SetFilterIndex(int index)
            { 
                if(index < 0  || index > this.fileFilters.Length)
                    return false;

                FileFilter ff = this.fileFilters[index];
                this.SetFilter(ff);

                return true;
            }

            public FileFilter GetFilter()
            { 
                if(this.filterIndex < 0 || this.filterIndex >= this.fileFilters.Length)
                    return new FileFilter("All Files (*.*)", "*.*", new string []{ "*.*" });

                return this.fileFilters[this.filterIndex];
            }

            void SetFilter(FileFilter ff)
            {
                foreach (FileBrowserArea fba in this.browserAreas)
                    fba.SetFilter(ff);
            }

            public bool SelectFile(string filepath, bool canReturn, bool append = false)
            {
                if(this.multifile == false)
                    append = false;

                foreach (FileBrowserArea fba in this.browserAreas)
                    fba.SelectFile(filepath, append);

                if(string.IsNullOrEmpty(filepath) == false)
                {
                    System.IO.FileAttributes fileSelAttr = 
                        System.IO.File.GetAttributes(filepath);
                
                    if( 
                        this.singleClickFolder == true && 
                        fileSelAttr.HasFlag(System.IO.FileAttributes.Directory) == true)
                    { 
                        this.ViewDirectory(filepath, NavigationType.SelectBrowser, true);
                    }
                    else if(this.singleClickFile == true && canReturn == true)
                    { 
                        this.ReturnFile(filepath);
                    }
                }

                return true;
            }

            public void SelectFiles(string[] filepaths, bool append = false)
            {
                if(filepaths.Length == 0)
                    return;

                if(this.multifile == false)
                    this.SelectFile(filepaths[0], true, false);

                foreach(FileBrowserArea fba in this.browserAreas)
                    fba.SelectFiles(filepaths, append);
            }

            public void HistoryGoBack()
            { 
                if(this.directoryHistory.Count == 0)
                    return;

                string str = this.directoryHistory.Pop();
                this.directoryRedoHistory.Push(this.curDirectory);
                
                this.ViewDirectory(str, NavigationType.BackHistory, false, false);
            }

            public void HistoryGoForward()
            {
                if(this.directoryRedoHistory.Count == 0)
                    return;

                string str = this.directoryRedoHistory.Pop();
                this.ViewDirectory(str, NavigationType.ForwardHistory, true, false);
            }

            public bool NavigateParent()
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.curDirectory);
                if(di.Exists == false)
                    return false;

                if(di.Parent == null)
                    return false;

                this.ViewDirectory(di.Parent.FullName, NavigationType.UpDirectory, true);
                return true;
            }


            public void ReturnFile(string file)
            { 
                if(this.mustExist == true)
                {
                    if(System.IO.File.Exists(file) == false)
                        return;
                }

                this.onOK?.Invoke(file);
                this.Close();
            }

            public void ReturnFiles(string [] files)
            { 
                if(this.mustExist == true)
                { 
                    List<string> retrans = new List<string>();
                    foreach(string f in files)
                    {
                        if (System.IO.File.Exists(f) == true)
                            retrans.Add(f);
                    }
                    files = retrans.ToArray();
                }

                if(files.Length == 0)
                    return;

                if(this.multifile == false)
                    ReturnFile(files[0]);

                this.onOKMulti?.Invoke(files);
                this.Close();
            }

            public void Close()
            { 
                GameObject.Destroy(this.gameObject);

                this.closeFn?.Invoke();
            }

            public bool OK()
            {
                if(this.multifile == true)
                {
                    foreach(FileBrowserArea fba in browserAreas)
                    { 
                        string [] rstr = fba.OnOKMulti();
                        if(rstr == null || rstr.Length == 0)
                            continue;

                        if(this.mustExist == false && this.saving == true)
                        {
                            FileFilter ff = this.GetFilter();
                            for(int i = 0; i < rstr.Length; ++i)
                                rstr[i] = ff.Ensure(rstr[i]);
                        }

                        this.onOKMulti?.Invoke(rstr);
                        this.Close();
                        return true;
                    }
                }
                else
                {
                    foreach (FileBrowserArea fba in browserAreas)
                    {
                        string str = fba.OnOK();
                        if(string.IsNullOrEmpty(str) == true)
                            continue;

                        if (this.mustExist == false && this.saving == true)
                        { 
                            FileFilter ff = this.GetFilter();
                            str = ff.Ensure(str);
                        }

                        this.onOK?.Invoke(str);
                        this.Close();
                        return true;
                    }
                }

                return false;
            }

            public void Cancel()
            { 
                this.Close();
                this.onCancel?.Invoke();
            }

            private bool ViewDirectory(string directory, NavigationType type, bool addHistory, bool clearForward)
            {
                if (System.IO.Directory.Exists(directory) == false)
                    return false;

                if (addHistory == true && string.IsNullOrEmpty(this.curDirectory) == false)
                    this.directoryHistory.Push(this.curDirectory);

                if(clearForward == true)
                    this.directoryRedoHistory.Clear();

                this.curDirectory = directory;

                foreach (FileBrowserArea fba in this.browserAreas)
                    fba.ViewDirectory(directory, this.GetFilter());

                this.spawner?.onNavigationEvent?.Invoke(type, directory);
                this.onViewDirectory?.Invoke(type, directory);

                return true;
            }

            public void RestartHistory()
            { 
                this.directoryHistory.Clear();
                this.directoryRedoHistory.Clear();
                this.ViewDirectory(this.curDirectory, NavigationType.Misc, false);
            }

            public bool ViewDirectory(string directory, NavigationType type, bool addHistory = true)
            {
                return this.ViewDirectory(directory, type, addHistory, true) == true;
            }

            public void Create(FileBrowseSpawner spawner, FileBrowseProp properties, string startingDir, string extensions, bool saving)
            {
                this.spawner = spawner;
                this.properties = properties;

                this.fileFilters = FileFilter.ParseFilterString(extensions);
                if(this.fileFilters.Length > 0)
                    this.filterIndex = 0;

                this.saving = saving;

                if( System.IO.Directory.Exists(startingDir) == false)
                {
                    // https://stackoverflow.com/questions/1143706/getting-the-path-of-the-home-directory-in-c
                    startingDir = UnityEngine.Application.persistentDataPath;
                }

                ////////////////////////////////////////////////////////////////////////////////

                GameObject goCrumb = new GameObject("Crumb");
                goCrumb.transform.SetParent(this.transform, false);

                FileBrowserCrumbRgn frCrumb = goCrumb.AddComponent< FileBrowserCrumbRgn>();
                RectTransform rtCrumb = frCrumb.gameObject.AddComponent<RectTransform>();
                rtCrumb.anchorMin = new Vector2(0.0f, 1.0f);
                rtCrumb.anchorMax = new Vector2(1.0f, 1.0f);
                rtCrumb.pivot = new Vector2(0.0f, 1.0f);
                rtCrumb.offsetMin = new Vector2(10.0f, -60.0f);
                rtCrumb.offsetMax = new Vector2(-10.0f, -10.0f);
                frCrumb.Create(properties, this);

                this.browserAreas.Add(frCrumb);

                ////////////////////////////////////////////////////////////////////////////////
                GameObject goFR = new GameObject("FilesRegion");
                goFR.transform.SetParent(this.transform, false);

                FileBrowseFileListRgn frRgn = goFR.AddComponent<FileBrowseFileListRgn>();
                RectTransform rtRgn = frRgn.gameObject.AddComponent<RectTransform>();
                rtRgn.anchorMin = Vector2.zero;
                rtRgn.anchorMax = Vector2.one;
                rtRgn.pivot = new Vector2(0.0f, 1.0f);
                rtRgn.offsetMin = new Vector2(10.0f, 40.0f);
                rtRgn.offsetMax = new Vector2(-10.0f, -60.0f);
                frRgn.Create(properties, this);

                this.region = frRgn;
                this.browserAreas.Add(frRgn);

                ////////////////////////////////////////////////////////////////////////////////
                GameObject goPath = new GameObject("FilesPath");
                goPath.transform.SetParent(this.transform, false);

                this.pathRgn = goPath.AddComponent<FileBrowserPathRgn>();
                RectTransform rtPath = this.pathRgn.gameObject.AddComponent<RectTransform>();
                rtPath.anchorMin = new Vector2(0.0f, 0.0f);
                rtPath.anchorMax = new Vector2(1.0f, 0.0f);
                rtPath.pivot = new Vector2(0.0f, 1.0f);
                rtPath.offsetMin = new Vector2(10.0f, 0.0f);
                rtPath.offsetMax = new Vector2(-10.0f, 40.0f);
                this.pathRgn.Create(properties, this);

                this.browserAreas.Add(this.pathRgn);

                ////////////////////////////////////////////////////////////////////////////////
                ///
                //GameObject goConfirm = new GameObject("Confirm");
                //goConfirm.transform.SetParent(this.transform);
                //goConfirm.transform.localScale = Vector3.one;
                //goConfirm.transform.localRotation = Quaternion.identity;
                //
                //FileBrowserConfirmRgn confirmRgn = goConfirm.AddComponent<FileBrowserConfirmRgn>();
                //RectTransform rtConf = confirmRgn.rectTransform;
                //rtConf.anchorMin = new Vector2(1.0f, 0.0f);
                //rtConf.anchorMax = new Vector2(1.0f, 0.0f);
                //rtConf.pivot = new Vector2(0.0f, 1.0f);
                //rtConf.offsetMin = new Vector2(-210.0f, 60.0f);
                //rtConf.offsetMax = new Vector2(-10.0f, 100.0f);
                //
                //confirmRgn.Create(this.properties, this);
                //this.browserAreas.Add(confirmRgn);


                this.ViewDirectory(startingDir, NavigationType.Misc);
            }
        }
    }
}