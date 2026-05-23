using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

namespace Services
{
    public interface IMatrixSearchService
    {
        UniTask<List<OffsetResult>> FindOffsetsAsync(
            IReadOnlyList<Matrix4x4> model,
            IReadOnlyList<Matrix4x4> space,
            IProgress<SearchProgress> onProgress,
            CancellationToken ct = default);
    }

    public readonly struct SearchProgress
    {
        public readonly int  currentSpaceIndex;
        public readonly int  totalSpace;
        public readonly int  foundSoFar;
        
        public readonly Matrix4x4 candidateOffset;

        public float Progress01 => totalSpace > 0 ? (float)currentSpaceIndex / totalSpace : 0f;

        public SearchProgress(int current, int total, int found, Matrix4x4 candidate)
        {
            currentSpaceIndex = current;
            totalSpace = total;
            foundSoFar = found;
            candidateOffset   = candidate;
        }
    }
    
    public class MatrixSearchService : IMatrixSearchService
    {
        private const int BatchSize = 50;

        public async UniTask<List<OffsetResult>> FindOffsetsAsync(
            IReadOnlyList<Matrix4x4> model,
            IReadOnlyList<Matrix4x4> space,
            IProgress<SearchProgress> onProgress,
            CancellationToken ct = default)
        {
            if (model == null || model.Count == 0)
                throw new ArgumentException("Model empty");
            if (space == null || space.Count == 0)
                throw new ArgumentException("Space empty");
            
            await UniTask.SwitchToThreadPool();

            var results = new List<OffsetResult>();
            
            var spaceSet = new HashSet<Matrix4x4>(space, MatrixComparer.Instance);
            
            Matrix4x4 m0    = model[0];
            Matrix4x4 m0inv = m0.inverse;

            bool m0Invertible = !float.IsNaN(m0inv.m00) && !float.IsInfinity(m0inv.m00);
            if (!m0Invertible)
                throw new InvalidOperationException("model[0] is irreversible");

            int found = 0;
            
            for (int i = 0; i < space.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                Matrix4x4 candidate = space[i] * m0inv;
                
                bool valid = true;
                var matchedIndices = new List<int>();

                for (int mi = 0; mi < model.Count; mi++)
                {
                    Matrix4x4 transformed = candidate * model[mi];
                    
                    if (spaceSet.Contains(transformed))
                    {
                        matchedIndices.Add(FindSpaceIndex(space, transformed));
                    }
                    else
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    found++;
                    results.Add(new OffsetResult
                    {
                        Offset = candidate,
                        MatchedCount = model.Count,
                        MatchedSpaceIndices = matchedIndices
                    });
                }
                
                if (i % BatchSize == 0)
                {
                    var progress = new SearchProgress(i, space.Count, found, candidate);
                    
                    await UniTask.SwitchToMainThread();
                    onProgress?.Report(progress);
                    await UniTask.SwitchToThreadPool();
                }
            }
            
            await UniTask.SwitchToMainThread();
            onProgress?.Report(new SearchProgress(space.Count, space.Count, found, Matrix4x4.identity));

            return results;
        }
        
        private static int FindSpaceIndex(IReadOnlyList<Matrix4x4> space, Matrix4x4 target)
        {
            for (int i = 0; i < space.Count; i++)
            {
                if (MatrixComparer.Instance.Equals(space[i], target))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}