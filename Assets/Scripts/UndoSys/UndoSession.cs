using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Undo
    {
        public class UndoSession 
        {
            List<BaseUndo> undos = new List<BaseUndo>();
            Stack<BaseUndo> redos = new Stack<BaseUndo>();

            HashSet<IUndoReceiver> undoReceivers = new HashSet<IUndoReceiver>();

            int stackNum = 0;

            int maxStack = 1000;

            public UndoSession(int maxStack)
            { 
                this.maxStack = Mathf.Max(maxStack, 10);
            }

            public bool IsDirty()
            { 
                return this.stackNum != 0;
            }

            public void ResetDirtyCounter()
            { 
                this.stackNum = 0;
            }

            public void ClearSession()
            { 
                this.stackNum = 0;

                this.undos.Clear();
                this.redos.Clear();
            }

            public bool AddReceiver(IUndoReceiver iur)
            { 
                if(iur == null)
                    return false;

                return this.undoReceivers.Add(iur);
            }

            public bool RemoveReceiver(IUndoReceiver iur)
            { 
                return this.undoReceivers.Remove(iur);
            }

            public bool HasUndos()
            { 
                return this.undos.Count > 0;
            }

            public bool HasRedos()
            {
                return this.redos.Count > 0;
            }

            public bool Undo()
            { 
                if(this.undos.Count == 0)
                    return false;

                int lastIdx = this.undos.Count - 1;
                BaseUndo bu = this.undos[lastIdx].Undo();
                this.undos.RemoveAt(lastIdx);

                this.redos.Push(bu);

                --this.stackNum;

                return true;
            }

            public bool Redo()
            { 
                if(this.redos.Count == 0)
                    return false;

                BaseUndo bu = this.redos.Pop().Undo();
                this.undos.Add(bu);

                ++this.stackNum;

                return true;
            }

            public void PushUndo(BaseUndo undo, bool clearRedos = true)
            { 
                if(clearRedos == true)
                    this.redos.Clear();

                ++this.stackNum;

                this.undos.Add(undo);

                if(this.undos.Count > this.maxStack)
                { 
                    int removeAmt = this.undos.Count - this.maxStack;
                    this.undos.RemoveRange(0, removeAmt);
                }
            }

            public string GetTopUndoName()
            { 
                if(this.undos.Count == 0)
                    return string.Empty;

                return this.undos[this.undos.Count - 1].GetName();
            }

            public string GetTopRedoName()
            { 
                if(this.redos.Count == 0)
                    return string.Empty;

                return this.redos.Peek().GetName();
            }
        }
    }
}
