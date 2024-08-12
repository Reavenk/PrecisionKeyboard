using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIL
    {
        public abstract class EleBaseSizer : Ele
        {
            bool active = true;

            public override bool Active { get { return this.active; } }

            public static void DoRectProcessing(ref Vector2 pos, ref Vector2 size, Vector2 entireRegion, LFlag finFlag)
            {
                if ((finFlag & LFlag.GrowHoriz) != 0)
                    size.x = entireRegion.x;
                else if ((finFlag & LFlag.AlignHorizCenter) != 0)
                    pos.x += (entireRegion.x - size.x) * 0.5f;
                else if ((finFlag & LFlag.AlignRight) != 0)
                    pos.x += entireRegion.x - size.x;

                if ((finFlag & LFlag.GrowVert) != 0)
                    size.y = entireRegion.y;
                else if ((finFlag & LFlag.AlignVertCenter) != 0)
                    pos.y += (entireRegion.y - size.y) * 0.5f;
                else if ((finFlag & LFlag.AlignBot) != 0)
                    pos.y += entireRegion.y - size.y;
            }

            public EleBaseSizer(){ }

            public EleBaseSizer(EleBaseRect rect, string name = "")
                : base(name)
            { 
                rect.SetSizer(this);
            }

            public EleBaseSizer(string name = "")
                : base(name)
            { }

            public EleBaseSizer(EleBaseSizer parent, float proportion, LFlag flags, string name = "")
                : base(name)
            { 
                parent.Add(this, proportion, flags);
            }

            public abstract void Add(Ele child, float proportion, LFlag flags);

            public abstract bool Remove(Ele child);

            public abstract bool HasEntry(Ele child);

            public void AddHorizontalSpace(float width, float proportion = 0.0f, LFlag flags = 0)
            {
                this.AddSpace(new Vector2(width, 0.0f), proportion, flags);
            }

            public void AddVerticalSpace(float height, float proportion = 0.0f, LFlag flags = 0)
            {
                this.AddSpace(new Vector2(0.0f, height), proportion, flags);
            }

            public void AddSpace(Vector2 space, float proportion = 0.0f, LFlag flags = 0)
            {
                this.Add(new EleSpace(space), proportion, flags);
            }

            public void AddSpace(float space, float proportion = 0.0f, LFlag flags = 0)
            {
                this.Add(new EleSpace(space), proportion, flags);
            }
        }
    }
}
