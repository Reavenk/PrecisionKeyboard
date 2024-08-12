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
        /// <summary>
        /// A linear attack envelope.
        /// </summary>
        public class GenLinAttackEmpty : GenBase
        {
            /// <summary>
            /// A recording on if the envelope has finished. Used as an optimization
            /// to know when to completly bypass the attack.
            /// </summary>
            bool passed = false;

            int totalAttackSamples;
            int itAttack = 0;
            int offset = 0;

            public GenLinAttackEmpty(int offsetSamples, int attackSamples)
               : base(0.0f, 0)
            {
                this.offset = offsetSamples;
                this.totalAttackSamples = attackSamples;

            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                if(this.passed == true)
                {
                    for(int i = start; i < start + size; ++i)
                        data[i] = 1.0f;

                    return;
                }

                if(this.offset > 0)
                { 
                    int burn = Min(size, this.offset);
                    start += burn;
                    size -= burn;
                    this.offset -= burn;

                    if(size <= 0)
                        return;
                }

                float at = this.itAttack;
                float tot = (float)this.totalAttackSamples;
                int sampCt = Min(size, totalAttackSamples - itAttack);

                for(int i = start; i < start + sampCt; ++i)
                { 
                    float v = at / tot;
                    at += 1.0f;

                    data[i] = v;
                }

                start += sampCt;
                size -= sampCt;
                this.itAttack += sampCt;

                if(this.itAttack >= this.totalAttackSamples)
                    this.passed = true;

                for(int i = start; i < start + size; ++i)
                    data[i] = 1.0f;
            }

            public override PlayState Finished()
            {
                return PlayState.Constant;
            }

            public override void ReportChildren(List<GenBase> lst)
            {}
        }
    }
}