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
        /// Generates a PCM stream with a repeated constant value.
        /// </summary>
        public class GenConstant : GenBase
        {
            /// <summary>
            /// The constant value to stream out.
            /// </summary>
            float constant;

            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="constant">The constant value.</param>
            public GenConstant(float constant)
                : base(0.0f, 0)
            { 
                this.constant = constant;
            }

            public override void AccumulateImpl(float [] data, int start, int size, int prefBuffSz, FPCMFactoryGenLimit pcmFactory)
            {
                for (int i = start; i < start + size; ++i)
                    data[i] = constant;
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
