using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class MatrixPayload
    {
        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;

        public Matrix4x4 ToUnityMatrix() => new(
            new Vector4(m00, m10, m20, m30),
            new Vector4(m01, m11, m21, m31),
            new Vector4(m02, m12, m22, m32),
            new Vector4(m03, m13, m23, m33)
        );
        
        public static MatrixPayload FromUnityMatrix(Matrix4x4 mat)
        {
            return new MatrixPayload
            {
                m00 = mat.m00, m01 = mat.m01, m02 = mat.m02, m03 = mat.m03,
                m10 = mat.m10, m11 = mat.m11, m12 = mat.m12, m13 = mat.m13,
                m20 = mat.m20, m21 = mat.m21, m22 = mat.m22, m23 = mat.m23,
                m30 = mat.m30, m31 = mat.m31, m32 = mat.m32, m33 = mat.m33
            };
        }
    }
    
    [Serializable]
    public class MatrixDataset
    {
        public List<MatrixPayload> matrices;
    }
    
    public class OffsetResult
    {
        public Matrix4x4 Offset;
        public int MatchedCount;
        
        public List<int> MatchedSpaceIndices;
    }

    [Serializable]
    public class ExportPayload
    {
        public int totalFound;
        public List<MatrixPayload> offsets;
    }
}