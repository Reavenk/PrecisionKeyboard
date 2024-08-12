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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// PCM Factory. A collection of previously allocated, and currently free PCM
        /// data that can be re-allocated so that systems don't have to constantly
        /// request new memory when it can just reuse old memory.
        /// </summary>
        public class FPCMFactory : IFPCMFactory
        { 
            static FPCMFactory instance;
            static public FPCMFactory Instance 
            {
                get
                { 
                    if(instance == null)
                        instance = new FPCMFactory();

                    return instance;
                } 
            }

            /// <summary>
            /// The available PCMs to redistribute.
            /// The key int is the number of samples it contains
            /// The list value is all the FPCMs available with that many samples.
            /// </summary>
            private Dictionary<int, List<FPCM>> entries = 
                new Dictionary<int, List<FPCM>>();

            public FPCM GetFPCM(int samples)
            { 
                if(samples < 1)
                    return null;

                List<FPCM> lst;
                if(this.entries.TryGetValue(samples, out lst) == false)
                { 
                    FPCM newRet = new FPCM(this, new float[samples]);
                    return newRet;
                }

                int lastIdx = lst.Count - 1;
                FPCM ret = lst[lastIdx];
                lst.RemoveAt(lastIdx);

                if(lst.Count == 0)
                    this.entries.Remove(samples);

                return ret;
            }

            /// <summary>
            /// Add an FPCM back into the pool
            /// </summary>
            /// <param name="fpcm">The FPCM to add back.</param>
            /// <returns>If true, the FPCM was successfully added.</returns>
            public bool ReturnFPCM(FPCM fpcm)
            { 
                if(fpcm.buffer == null || fpcm.buffer.Length == 0)
                    return false;

                int samples = fpcm.buffer.Length;

                List<FPCM> lst;
                if(this.entries.TryGetValue(samples, out lst) == false)
                { 
                    lst = new List<FPCM>();
                    this.entries.Add(samples, lst);
                }

                lst.Add(fpcm);
                return true;
            }

            
            FPCM IFPCMFactory.GetFPCM(int samples, bool zero)
            {
                FPCM fpcm = this.GetFPCM(samples);

                if(zero == true)
                    fpcm.Zero();

                return fpcm;
            }

            FPCM IFPCMFactory.GetZeroedFPCM(int samples, int start, int size)
            {
                FPCM fpcm = this.GetFPCM(samples);
                fpcm.Zero(start, size);
                return fpcm;
            }

            
            FPCM IFPCMFactory.GetGlobalFPCM(int samples, bool zero)
            {
                FPCM fpcm = this.GetFPCM(samples);

                if(zero == true)
                    fpcm.Zero();

                return fpcm;
            }

            FPCM IFPCMFactory.GetZeroedGlobalFPCM(int samples, int start, int size)
            { 
                FPCM fpcm = this.GetFPCM(samples);
                fpcm.Zero(start, size);
                return fpcm;
            }
        }
    }
}
