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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenStutterHold : GenBase
        {
            // The PCM stream
            GenBase input;
            int playSamples;
            int holdSamples;
            bool playing;

            int samplesLeft;

            public GenStutterHold(GenBase input, int playSamples, int holdSamples, bool playing)
                : base(0.0f, 0)
            {
                this.input = input;
                this.playSamples = playSamples;
                this.holdSamples = holdSamples;
                this.playing = playing;

                if(playing == true)
                    this.samplesLeft = playSamples;
                else
                    this.samplesLeft = holdSamples;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                while(size > 0)
                {
                    if(this.playing == true)
                    { 
                        int burn = Min(size, this.samplesLeft);

                        this.input.Accumulate(data, start, size, prefBuffSz, pcmFactory);

                        this.samplesLeft -= burn;
                        size -=burn;
                        start += burn;

                        if(this.samplesLeft == 0)
                        {
                            this.playing = false;
                            this.samplesLeft = this.holdSamples;
                        }
                    }
                    else
                    { 
                        int burn = Min(size, this.samplesLeft);
                        this.samplesLeft -= burn;
                        size -=burn;
                        start += burn;

                        if(this.samplesLeft == 0)
                        { 
                            this.playing = true;
                            this.samplesLeft = this.playSamples;
                        }
                    }
                }
            }

            public override PlayState Finished()
            {
                if (this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }
    }
}