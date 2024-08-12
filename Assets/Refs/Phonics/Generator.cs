using UnityEngine;

namespace PxPre
{ 
    namespace Phonics
    { 
        public static class Generator
        {
            public const float pi = 3.14159265359f;
            public const float tau = pi * 2.0f;

            

            public static void ZeroBuffer(float [] rf, int start, int len)
            { 
                System.Array.Clear(rf, start, len);
            }

            public static float SetSine(float [] rf, int start, int len, float time, float freq, float amp, int sampsSec)
            { 
                // Multiply tau for 1 hertz.
                // Multiply by frequency because that's requested as a paramter
                float sincr = 1.0f / sampsSec;
                float s = time;
                for(int i = start; i < start + len; ++i)
                { 
                    rf[i] = Mathf.Sin(s * freq * tau) * amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetSine(float[] rf, int start, int len, float phase, float time, float freq, int sampsSec, float env0, float env1, float env2, float env3)
            {
                float fincr = 1.0f / sampsSec;
                float sincr = fincr * freq * tau;
                float s = time + phase;
                float f = s;
                for (int i = start; i < start + len; ++i)
                {
                    float f2 = f * f;
                    float f3 = f2 * f;
                    float amp = env0 + f * env1 + f2 * env2 + f3 * env3;

                    rf[i] = Mathf.Sin(s) * amp;
                    s += sincr;
                    f += fincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float AddSine(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float sincr = 1.0f / sampsSec * freq * tau;
                float s = time + phase;
                for (int i = start; i < start + len; ++i)
                {
                    rf[i] += Mathf.Sin(s) * amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetThSquare(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float s = (time + phase) * freq * 2.0f;
                float sincr = (1.0f / sampsSec * freq) * 2.0f; 
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)s;
                    rf[i] = ((cross & 1) != 0) ? -amp : amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetThSquare(float[] rf, int start, int len, float phase, float time, float freq, int sampsSec, float env0, float env1, float env2, float env3)
            {
                float s = (time + phase) * freq * 2.0f;
                float f = s;

                float fincr = 1.0f / sampsSec;
                float sincr = fincr * freq * 2.0f;

                for (int i = start; i < start + len; ++i)
                {
                    float f2 = f * f;
                    float f3 = f2 * f;
                    float amp = env0 + f * env1 + f2 * env2 + f3 * env3;

                    int cross = (int)s;
                    rf[i] = ((cross & 1) != 0) ? -amp : amp;

                    s += sincr;
                    f += fincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float AddThSquare(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float sincr = 1.0f / sampsSec * freq * tau;
                float s = time + phase;
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)(s * freq * 2.0f);
                    rf[i] += ((cross & 1) != 0) ? -amp : amp;
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float SetTri(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float tamp = amp * 2.0f;
                float sincr = (1.0f / sampsSec * freq) * 2.0f;
                float s = (time + phase) * sampsSec * freq * 2.0f;
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)s;
                    float lambda = s - cross;

                    if((cross & 1) != 0)
                        rf[i] = -amp + tamp * lambda;
                    else
                        rf[i] = amp - tamp * lambda;
                    
                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }

            public static float AddTri(float [] rf, int start, int len, float phase, float time, float freq, float amp, int sampsSec)
            {
                float tamp = amp * 2.0f;
                float sincr = (1.0f / sampsSec * freq) * 2.0f;
                float s = (time + phase) * sampsSec * freq * 2.0f;
                for (int i = start; i < start + len; ++i)
                {
                    int cross = (int)s;
                    float lambda = s - cross;

                    if ((cross & 1) != 0)
                        rf[i] += -amp + tamp * lambda;
                    else
                        rf[i] += amp - tamp * lambda;

                    s += sincr;
                }
                return time + (float)len / (float)sampsSec;
            }
        }
    }
}