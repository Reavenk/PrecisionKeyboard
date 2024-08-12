using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class MeterValue
        {
            public static MeterValue DefaultMeter = new MeterValue();

            /// <summary>
            /// In order to try to take performance shortcuts, we're not going to check
            /// EVERY audio sample, but we need to check different distances in order to
            /// not do weird things at edge case frequencies (as often).
            /// </summary>
            public static int [] MeterSkips = 
                new int [] { 1, 2, 3, 4, 5, 8, 9, 16, 17, 32 };

            public float min;
            public float max;
        }

        public class GenMeter : GenBase
        {
            MeterValue meter;
            GenBase input;
            bool clearOnProcessing = true;

            public MeterValue Meter {get{return this.meter; } }

            public GenMeter(GenBase input)
                : base(0.0f, 0)
            { 
                this.input = input;
                this.meter = new MeterValue();
            }

            public GenMeter(GenBase input, MeterValue meter)
                : base(0.0f, 0)
            {
                this.input = input;
                this.meter = meter;
                this.clearOnProcessing = false;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                this.input.Accumulate(data, start, size, prefBuffSz, pcmFactory);
                
                if(this.clearOnProcessing == true)
                { 
                    this.meter.min = 0.0f;
                    this.meter.max = 0.0f;
                }

                int msIdx = 0;
                for(
                    int i = start; i < start + size; 
                    i += MeterValue.MeterSkips[msIdx%MeterValue.MeterSkips.Length], 
                    ++msIdx)
                { 
                    float val = data[i];

                    if(val > this.meter.max)
                        this.meter.max = val;
                    else if(val < this.meter.min)
                        this.meter.min = val;

                    ++msIdx;
                }
            }

            public override PlayState Finished()
            {
                if(this.input != null)
                    return this.input.Finished();

                return PlayState.Finished;
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                if(this.input != null)
                    lst.Add(this.input);
            }
        }
    }
}