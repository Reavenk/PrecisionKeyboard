// MIT License
//
// Copyright(c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public enum PlayState
        { 
            /// <summary>
            /// It's on a schedule to eventually output PCM, but not yet.
            /// </summary>
            NotStarted,

            /// <summary>
            /// It's currently playing audio.
            /// </summary>
            Playing,

            /// <summary>
            /// It's already output its PCM and is now only going to give out
            /// 0.0 PCM
            /// </summary>
            Finished,

            /// <summary>
            /// For generators. They're always giving a constant tone out.
            /// </summary>
            Constant
        }

        /// <summary>
        /// The base class for all PCM streams. They're prefixed with the 
        /// word "gen" because they generate PCM data.
        /// </summary>
        public abstract class GenBase
        {
            // The value of pi.
            public const float fPie = 3.14159265359f;
            public const double dPie = 3.14159265359;
            //
            // The value of 2 * pi
            public  const float fTau = 6.28318530718f;
            public  const double dTau = 6.28318530718;

            public static float Max(float a, float b)
            { return a > b ? a : b; }

            public static float Min(float a, float b)
            { return a < b ? a : b; }

            public static int Max(int a, int b)
            { return a > b ? a : b;}

            public static int Min(int a, int b)
            { return a < b ? a : b; }

            public long it = 0;
            public long It { get { return this.it; } }

            /// <summary>
            /// The amount of time, in seconds, that the PCM stream has been 
            /// generating sample for.
            /// </summary>
            private double curTime;
            public double CurTime { get { return this.curTime; } }

            public double TimePerSample { get { return this.timePerSample; } }
            private double timePerSample;

            private int samplesPerSec;
            public int SamplesPerSec {get{return this.samplesPerSec; } }

            protected GenBase(double startTime, int samplesPerSec)
            { 
                this.samplesPerSec = samplesPerSec;

                this.curTime = startTime;
                this.it = (long)(samplesPerSec * startTime);
                this.timePerSample = 1.0 / samplesPerSec;
            }

            public abstract void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory);

            public void Accumulate(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                this.AccumulateImpl(data, start, size, prefBuffSz, pcmFactory);

                this.curTime += size * timePerSample;
                this.it += size;
            }

            public void Set(float [] data, int size, FPCMFactoryGenLimit pcmFactory)
            {
                for(int i = 0; i < size; ++i)
                    data[i] = 0.0f;

                FPCMFactoryGenLimit fgl = 
                    new FPCMFactoryGenLimit(pcmFactory, size);

                this.AccumulateImpl(data, 0, size, size, fgl);

                fgl.ReleaseScope();

                this.curTime += data.Length * timePerSample;
                this.it += data.Length;
            }

            public void ReaderCallback(float[] data)
            {
                IFPCMFactory pcmFactory = FPCMFactory.Instance;
                FPCMFactoryGenLimit fgl = new FPCMFactoryGenLimit(pcmFactory, data.Length);
                this.Set(data, data.Length, fgl);
            }

            public void SetPositionCallback(int position)
            { /* Do nothing */ }

            public abstract PlayState Finished();

            public static PlayState ResolveTwoFinished(GenBase a, GenBase b)
            {
                if(a == null && b == null)
                    return PlayState.Finished;

                if(a == null)
                    return b.Finished();

                if(b == null)
                    return a.Finished();

                PlayState psA = a.Finished();
                PlayState psB = b.Finished();

                if (psA == PlayState.Playing || psB == PlayState.Playing)
                    return PlayState.Playing;

                // If we're modulating and one is signaling it's just going to
                // only return 0.0 from now one, we're done.
                if (psA == PlayState.Finished || psB == PlayState.Finished)
                    return PlayState.Finished;

                if (psA == PlayState.NotStarted && psB == PlayState.NotStarted)
                    return PlayState.NotStarted;

                if (psA == PlayState.Constant || psB == PlayState.Constant)
                    return PlayState.Constant;

                return PlayState.Playing;
            }

            public void DeconstructHierarchy()
            {
                List<GenBase> hierarchy = this.ReportChildrenHierarchy();
                foreach (GenBase gb in hierarchy)
                    gb.Deconstruct();
            }

            public void ReleaseHierarchy()
            {
                List<GenBase> heirarchy = this.ReportChildrenHierarchy();
                foreach(GenBase gb in heirarchy)
                    gb.Release();
            }

            public List<GenBase> ReportChildrenHierarchy()
            { 
                List<GenBase> ret = new List<GenBase>();
                ret.Add(this);
                int idx = 0;
                while(idx < ret.Count)
                { 
                    ret[idx].ReportChildren(ret);
                    ++idx;
                }
                return ret;
            }

            /// <summary>
            /// Add all your children to the list so we can send a signal to the entire heirarchy.
            /// </summary>
            /// <param name="gb"></param>
            public abstract void ReportChildren(List<GenBase> lst);

            // Dissassembly anything before the object gets thrown away. While garbage collection 
            // will handle most things, we need this function to signal global allocations from the
            // FPCM factory to be released back.
            public virtual void Deconstruct() 
            { }

            // Called when the key is released.
            public virtual void Release() 
            { }
        }
    }
}