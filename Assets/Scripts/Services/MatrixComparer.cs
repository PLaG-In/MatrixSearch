using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    public sealed class MatrixComparer : IEqualityComparer<Matrix4x4>
    {
        public static readonly MatrixComparer Instance = new();

        private const float Epsilon = 1e-4f;
        private const float HashScale = 1e3f;

        public bool Equals(Matrix4x4 a, Matrix4x4 b)
        {
            for (int i = 0; i < 16; i++)
            {
                if (Mathf.Abs(a[i] - b[i]) > Epsilon)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(Matrix4x4 m)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < 16; i++)
                    hash = hash * 31 + Mathf.RoundToInt(m[i] * HashScale);
                return hash;
            }
        }
    }
}