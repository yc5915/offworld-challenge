//MathS class from Unity Community: http://wiki.unity3d.com/index.php?title=SpeedLerp
using UnityEngine;

namespace Offworld.SystemCore
{
    public static class MathUtilities
    {
        public const float cEPSILON = 1E-6f;
        private static readonly float[] gammaToLinear = new float [256];

        static MathUtilities()
        {
            for (int i = 0; i < gammaToLinear.Length; i++)
                gammaToLinear[i] = new Color(i / 255.0f, 0, 0, 0).r;
        }

        public static float Interpolate(float from, float to, float percentage)
        {
            return from * (1 - percentage) + to * percentage;
        }

        public static float Lerp(float from, float to, float value)
        {
            if (value < 0.0f)
                return from;
            else if (value > 1.0f)
                return to;
            return (to - from) * value + from;
        }
        
        public static float LerpUnclamped(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }
        
        //public static float InverseLerp(float from, float to, float value)
        //{
        //    float result = InverseLerpUnclamped(from, to, value);
        //    return Mathf.Clamp01(result);
        //}
        
        //public static float InverseLerpUnclamped(float from, float to, float value)
        //{
        //    if (Mathf.Approximately(from, to)) //prevent divide-by-zero
        //        return 0.0f;

        //    return (value - from) / (to - from);
        //}
        
        public static float SmoothStep(float from, float to, float value)
        {
            if (value < 0.0f)
                return from;
            else if (value > 1.0f)
                return to;
            value = value * value * (3.0f - 2.0f * value);
            return (1.0f - value) * from + value * to;
        }
        
        public static float SmoothStepUnclamped(float from, float to, float value)
        {
            value = value * value * (3.0f - 2.0f * value);
            return (1.0f - value) * from + value * to;
        }

        //http://webhome.cs.uvic.ca/~blob/courses/485c/notes/pdf/5-interpolation.pdf
        //accelerates from [0..easeInTime], constant velocity from [easeInTime..easeOutTime], decelerate from [easeOutTime..1]
        public static float EaseInOut(float easeInTime, float easeOutTime, float time)
        {
            float t = time;
            float t1 = easeInTime;
            float t2 = easeOutTime;
            float v0 = 2 / (1 + t2 - t1); /* constant velocity attained */ 
            float d = 0;
            if (t<t1)
            { 
                d = v0*t*t/(2*t1); 
            } 
            else 
            { 
                d = v0*t1/2; 
                if (t<=t2)
                { 
                    d += (t-t1)*v0; 
                } 
                else 
                { 
                    d += (t2-t1)*v0;
                    d += (t-t*t/2-t2+t2*t2/2)*v0/(1-t2);
                } 
            } 
            return d; 
        }
        
        public static float SuperLerp(float from, float to, float from2, float to2, float value)
        {
            if (from2 < to2)
            {
                if (value < from2)
                    value = from2;
                else if (value > to2)
                    value = to2;
            } else
            {
                if (value < to2)
                    value = to2;
                else if (value > from2)
                    value = from2;  
            }
            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
        
        public static float SuperLerpUnclamped(float from, float to, float from2, float to2, float value)
        {
            return (to - from) * ((value - from2) / (to2 - from2)) + from;
        }
        
        public static Color ColorLerp(Color c1, Color c2, float value)
        {
            if (value > 1.0f)
                return c2;
            else if (value < 0.0f)
                return c1;
            return new Color(c1.r + (c2.r - c1.r) * value, 
                              c1.g + (c2.g - c1.g) * value, 
                              c1.b + (c2.b - c1.b) * value, 
                              c1.a + (c2.a - c1.a) * value);
        }

        //public static Vector2 Vector2Lerp(Vector2 v1, Vector2 v2, float value)
        //{
        //    if (value > 1.0f)
        //        return v2;
        //    else if (value < 0.0f)
        //        return v1;
        //    return new Vector2(v1.x + (v2.x - v1.x) * value, 
        //                        v1.y + (v2.y - v1.y) * value);       
        //}

        //public static Vector2 Vector2LerpUnclamped(Vector2 v1, Vector2 v2, Vector2 value)
        //{
        //    return new Vector2(LerpUnclamped(v1.x, v2.x, value.x), LerpUnclamped(v1.y, v2.y, value.y));
        //}
        
        public static Vector3 Vector3Lerp(Vector3 v1, Vector3 v2, float value)
        {
            if (value > 1.0f)
                return v2;
            else if (value < 0.0f)
                return v1;
            return new Vector3(v1.x + (v2.x - v1.x) * value, 
                                v1.y + (v2.y - v1.y) * value, 
                                v1.z + (v2.z - v1.z) * value);
        }
        
        //public static Vector4 Vector4Lerp(Vector4 v1, Vector4 v2, float value)
        //{
        //    if (value > 1.0f)
        //        return v2;
        //    else if (value < 0.0f)
        //        return v1;
        //    return new Vector4(v1.x + (v2.x - v1.x) * value, 
        //                        v1.y + (v2.y - v1.y) * value, 
        //                        v1.z + (v2.z - v1.z) * value,
        //                        v1.w + (v2.w - v1.w) * value);
        //}

        public static bool InRange(int value, int minimum, int maximum)
        {
            return ((value >= minimum) && (value <= maximum));
        }

        public static bool InRange(float value, float minimum, float maximum)
        {
            return ((value >= minimum) && (value <= maximum));
        }

        public static bool InRange(Vector3 value, Vector3 minimum, Vector3 maximum)
        {
            return InRange(value.x, minimum.x, maximum.x) &&
                   InRange(value.y, minimum.y, maximum.y) &&
                   InRange(value.z, minimum.z, maximum.z);
        }

        public static Vector3 Clamp(Vector3 inVector, Vector3 minVector, Vector3 maxVector)
        {
            Vector3 result = inVector;
            result.x = Mathf.Clamp(inVector.x, minVector.x, maxVector.x);
            result.y = Mathf.Clamp(inVector.y, minVector.y, maxVector.y);
            result.z = Mathf.Clamp(inVector.z, minVector.z, maxVector.z);
            return result;
        }

        public static bool EpsilonEquals(float lhs, float rhs, float epsilon = cEPSILON)
        {
            return (Mathf.Abs(lhs - rhs) <= epsilon);
        }

        //public static Color GammaToLinear(Color32 color)
        //{
        //    return new Color(gammaToLinear[color.r], gammaToLinear[color.g], gammaToLinear[color.b], color.a / 255.0f);
        //}

        //modified from http://orion.math.iastate.edu/reu/2001/voronoi/halton_sequence.html
        public static float HaltonSequence(int index, int prime)
        {
            index++; //0-based to 1-based
            float result = 0;
            float half = 1.0f / prime;
            while (index > 0)
            {
                int digit = index % prime;
                result += digit * half;
                index /= prime;
                half /= prime;
            }

            return result;
        }

        //public static Vector2 HaltonSequence2D(int index)
        //{
        //    return new Vector2(HaltonSequence(index, 2), HaltonSequence(index, 3));
        //}

        public static float Frac(float value)
        {
            return (value - Mathf.Floor(value));
        }

        //public static Vector2 Frac(Vector2 value)
        //{
        //    return new Vector2(Frac(value.x), Frac(value.y));
        //}

        //public static Vector4 PlaneToVector(Plane plane)
        //{
        //    return new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        //}

        //public static Vector2 Vector3To2(Vector3 input)
        //{
        //    return new Vector2(input.x, input.y);
        //}

        //wraps to [-180, 180)
        public static float WrapAngle(float degrees, float target = 0.0f)
        {
            degrees -= target;
            float normalized = Frac(degrees / 360 + 0.5f);
            degrees = 360 * (normalized - 0.5f);
            degrees += target;
            return degrees;
        }

        //wraps to [0..size)
        public static int WrapInt(int index, int size)
        {
            return (index % size + size) % size; //handles both positives and negatives
        }

        public static int Pow(int value, int exponent)
        {
            if (exponent < 0) //error
                return 0;

            int result = 1;
            for (int i=0; i<exponent; i++)
                result *= value;
            return result;
        }

        //public static Vector3 GetTranslation(this Matrix4x4 lhs)
        //{
        //    return lhs.MultiplyPoint3x4(Vector3.zero);
        //}

        //public static Rect Multiply(this Rect lhs, Vector2 rhs)
        //{
        //    return new Rect(lhs.x * rhs.x, lhs.y * rhs.y, lhs.width * rhs.x, lhs.height * rhs.y);
        //}

        public static Color SetAlpha(this Color lhs, float value)
        {
            lhs.a = value;
            return lhs;
        }

        public static Vector3 SetX(this Vector3 lhs, float value)
        {
            lhs.x = value;
            return lhs;
        }

        public static Vector3 SetY(this Vector3 lhs, float value)
        {
            lhs.y = value;
            return lhs;
        }

        public static Vector3 SetZ(this Vector3 lhs, float value)
        {
            lhs.z = value;
            return lhs;
        }
    }
}