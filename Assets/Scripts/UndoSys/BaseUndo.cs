using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Undo
    {
        public abstract class BaseUndo
        {
            public readonly string name;
            public readonly bool isUndo;

            public BaseUndo(string name, bool isUndo)
            { 
                this.name = name;
                this.isUndo = isUndo;
            }

            public abstract BaseUndo Undo();

            public virtual string GetName()
            { return this.name; }
        }
    }
}