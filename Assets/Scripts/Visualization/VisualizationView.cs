using System.Collections.Generic;
using Pools;
using UnityEngine;
using Zenject;

namespace Visualization
{
    public class VisualizationView : MonoBehaviour
    {
        [Header("Visualization settings")]
        [SerializeField] private int   maxSpaceCubes = 1000;
        [SerializeField] private float cubeScale = 0.25f;
        [SerializeField] private float candidateLifetime = 0.08f;

        private CubePool _pool;

        private readonly List<CubeView> _spaceCubes = new();
        private readonly List<CubeView> _modelCubes = new();
        private readonly List<CubeView> _candidateCubes = new();
        private readonly List<CubeView> _matchedCubes = new();

        [Inject]
        public void Construct(CubePool pool)
        {
            _pool = pool;
        }
        
        public void SpawnSpaceCubes(IReadOnlyList<Matrix4x4> spaceMatrices)
        {
            ClearList(_spaceCubes);

            int count = Mathf.Min(spaceMatrices.Count, maxSpaceCubes);
            int step  = Mathf.Max(1, spaceMatrices.Count / count);

            for (int i = 0; i < spaceMatrices.Count && _spaceCubes.Count < count; i += step)
            {
                var cube = _pool.Spawn();
                cube.ApplyMatrix(NormalizeScale(spaceMatrices[i], cubeScale));
                cube.SetState(CubeState.Space);
                _spaceCubes.Add(cube);
            }
        }
        
        public void SpawnModelCubes(IReadOnlyList<Matrix4x4> modelMatrices)
        {
            ClearList(_modelCubes);

            foreach (var m in modelMatrices)
            {
                var cube = _pool.Spawn();
                cube.ApplyMatrix(NormalizeScale(m, cubeScale * 1.5f));
                cube.SetState(CubeState.Model);
                _modelCubes.Add(cube);
            }
        }
        
        public void ShowCandidate(Matrix4x4 offset, IReadOnlyList<Matrix4x4> modelMatrices)
        {
            ClearList(_candidateCubes);

            int show = Mathf.Min(modelMatrices.Count, 20);
            int step = Mathf.Max(1, modelMatrices.Count / show);

            for (int i = 0; i < modelMatrices.Count && _candidateCubes.Count < show; i += step)
            {
                Matrix4x4 transformed = offset * modelMatrices[i];
                var cube = _pool.Spawn();
                cube.ApplyMatrix(NormalizeScale(transformed, cubeScale * 1.2f));
                cube.SetState(CubeState.Candidate);
                _candidateCubes.Add(cube);
            }
        }
        
        public void ShowMatch(Matrix4x4 offset, IReadOnlyList<Matrix4x4> modelMatrices)
        {
            foreach (var m in modelMatrices)
            {
                Matrix4x4 transformed = offset * m;
                var cube = _pool.Spawn();
                cube.ApplyMatrix(NormalizeScale(transformed, cubeScale * 1.8f));
                cube.SetState(CubeState.Matched);
                cube.PlayMatchAnimation();
                _matchedCubes.Add(cube);
            }
        }

        public void ClearCandidates() => ClearList(_candidateCubes);

        public void ClearAll()
        {
            ClearList(_spaceCubes);
            ClearList(_modelCubes);
            ClearList(_candidateCubes);
            ClearList(_matchedCubes);
        }
        
        private static Matrix4x4 NormalizeScale(Matrix4x4 m, float targetScale)
        {
            Vector3    pos = m.GetColumn(3);
            Quaternion rot = m.rotation;
            return Matrix4x4.TRS(pos, rot, Vector3.one * targetScale);
        }

        private void ClearList(List<CubeView> list)
        {
            foreach (var cube in list)
            {
                _pool.Despawn(cube);
            }

            list.Clear();
        }

        private void OnDestroy() => ClearAll();
    }
}