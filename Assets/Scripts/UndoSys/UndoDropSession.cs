using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Undo
    {
        public class UndoDropSession : System.IDisposable
        {
            public bool collapseGroup = false;

            struct DropEntry
            { 
                public int counter;
                public PxPre.Undo.UndoGroup group;
            }

            static Dictionary<UndoSession, DropEntry> activeSessions = 
                new System.Collections.Generic.Dictionary<UndoSession, DropEntry>();

            public static bool HasActiveUndoSession(UndoSession us)
            { 
                return activeSessions.ContainsKey(us);
            }

            UndoSession session;

            public UndoDropSession(UndoSession session, string groupName)
            { 
                this.session = session;

                DropEntry de;
                if(activeSessions.TryGetValue(session, out de) == false)
                {
                    de = new DropEntry();
                    de.counter = 1;
                    de.group = new UndoGroup(groupName, true);

                    activeSessions.Add(session, de);
                    return;
                }

                ++de.counter;
                activeSessions[session] = de;
            }

            public PxPre.Undo.UndoGroup GetUndoGroup()
            { 
                DropEntry de;
                activeSessions.TryGetValue(this.session, out de);
                return de.group;
            }

            void System.IDisposable.Dispose()
            { 
                DropEntry de;
                if(activeSessions.TryGetValue(this.session, out de) == false)
                    return;

                --de.counter;
                if(de.counter == 0)
                { 
                    activeSessions.Remove(this.session);
                    UndoGroup ug = de.group;

                    if(ug.undos.Count > 0)
                    {
                        BaseUndo bu = ug;
                        if(ug.undos.Count == 1 && this.collapseGroup == true)
                            bu = ug.undos[0];

                        this.session.PushUndo(bu);
                    }
                }
                else
                    activeSessions[this.session] = de;
            }
        }
    }
}