using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Undo
    {
        public class UndoGroup : BaseUndo
        {
            public List<BaseUndo> undos = 
                new List<BaseUndo>();

            public UndoGroup(string name, bool isUndo)
                : base(name, isUndo)
            { }

            public override string GetName()
            {
                if(string.IsNullOrEmpty(this.name) == false)
                    return this.name; 

                if(this.undos.Count > 0)
                    return this.undos[0].name;

                return "";
            }

            public override BaseUndo Undo()
            { 
                UndoGroup ug = new UndoGroup(this.name, this.isUndo);

                for(int i = this.undos.Count - 1; i >= 0; --i)
                { 
                    BaseUndo bu = this.undos[i].Undo();
                    ug.undos.Add(bu);
                }

                return ug;
            }

            public void AddUndo(BaseUndo undo)
            { 
                this.undos.Add(undo);
            }

        }
    }
}
