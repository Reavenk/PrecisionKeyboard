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
        /// A wrapper on top of another IFPCMFactor (most likely a FPCMFactory) that
        /// implements keeping track of scope.
        /// </summary>
        public class FPCMScopedFactory : IFPCMFactory
        {
            public readonly IFPCMFactory parent;

            public List<FPCM> allocated = 
                new List<FPCM>();

            public FPCMScopedFactory(IFPCMFactory parent)
            { 
                this.parent = parent;
            }

            public void ReleaseScope()
            { 
                foreach(FPCM fpcm in this.allocated)
                    fpcm.Release();

                this.allocated.Clear();
            }

            FPCM IFPCMFactory.GetFPCM(int samples, bool zero)
            { 
                FPCM ret = this.parent.GetFPCM(samples, zero);
                if(ret == null)
                    return null;

                this.allocated.Add(ret);
                return ret;
            }

            FPCM IFPCMFactory.GetZeroedFPCM(int samples, int start, int size)
            { 
                FPCM ret = this.parent.GetZeroedFPCM(samples, start, size);
                if(ret == null)
                    return null;

                this.allocated.Add(ret);
                return ret;
            }

            FPCM IFPCMFactory.GetGlobalFPCM(int samples, bool zero)
            { 
                return this.parent.GetGlobalFPCM(samples, zero);
            }

            FPCM IFPCMFactory.GetZeroedGlobalFPCM(int samples, int start, int size)
            { 
                return this.parent.GetZeroedGlobalFPCM(samples, start, size);
            }
        }
    }
}
