using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace FileBrowse
    {
        public struct FileFilter
        {
            public static FileFilter [] ParseFilterString(string str)
            {
                List<FileFilter> ret = new List<FileFilter>();

                string [] parts = str.Split(new char [] {'|'});
                for(int i = 0; i + 1 < parts.Length; i += 2)
                { 
                    string label = parts[i + 0];
                    string filter = parts[i + 1];
                    string [] filtersplit = filter.Split(new char []{ ';' });

                    FileFilter ff = new FileFilter(label, filter, filtersplit);
                    ret.Add(ff);
                }

                return ret.ToArray();
            }

            public string label;
            public string filterString;
            public string [] filters;

            public FileFilter(string label, string filterString, string [] filters)
            { 
                this.label = label;
                this.filterString = filterString;
                this.filters = filters;
            }

            public string Ensure(string path)
            { 
                if(this.filters == null || this.filters.Length == 0)
                    return path;

                foreach(string ext in this.filters)
                { 
                    if(ext.Length == 0 || ext.StartsWith("*.") == false)
                        continue;

                    string extCheck = ext.Substring(1);
                    if(path.EndsWith(extCheck, System.StringComparison.OrdinalIgnoreCase) == false)
                        return path + extCheck;
                }

                return path;
            }
        }
    }
}
