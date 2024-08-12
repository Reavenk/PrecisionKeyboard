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

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// Interface for a FPCM factory that either returned a scoped FPCM,
        /// or non-scoped FPCM.
        /// </summary>
        public interface IFPCMFactory
        {
            /// <summary>
            /// Request an FPCM of a requested size. This is an implemenation
            /// for IFPCMFactory that gives the FPCM scope.
            /// 
            /// When an FPCM is scoped, it is recorded as being assigned to a scope,
            /// and when the scope ends, all FPCMs allocated under that scope are forced
            /// to be re-entered back into the FPCMFactory.
            /// 
            /// This is a feature that allows allocating FPCMs without having to track them.
            /// To avoid giving scope to an FPCM, used GetGlobalFPCM()
            /// 
            /// </summary>
            /// <param name="samples">The number of samples of the FPCM.</param>
            /// <param name="zero">If true, zero out the array before returning it.</param>
            /// <returns>An FPCM of the correct size. If one is available, it will be
            /// de-owned and returned. If none are available, a new one will be allocated.</returns>
            FPCM GetFPCM(int samples, bool zero);

            FPCM GetZeroedFPCM(int samples, int start, int size);

            /// <summary>
            /// Request an FPCM of a requested size. This is an implementation
            /// for IFPCMFactor that does NOT give the FPCM scope.
            /// </summary>
            /// <param name="samples">The number of samples of the FPCM.</param>
            /// <param name="zero">If true, zero out the array before returning it.</param>
            /// <returns>An FPCM of the correct size. If one is available, it will be
            /// de-owned and returned. If none are available, a new one will be allocated.</returns>
            FPCM GetGlobalFPCM(int samples, bool zero);

            FPCM GetZeroedGlobalFPCM(int samples, int start, int size);
        }
    }
}