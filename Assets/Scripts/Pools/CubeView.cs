using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Pools
{
    public enum CubeState
    {
        Space,       // gray - matrix from Space
        Model,       // blue - matrix from Model
        Candidate,   // yellow - current candidate(transformed model)
        Matched      // green - found match
    }
    
    public class CubeView : MonoBehaviour
    {
        [SerializeField] private Renderer cubeRenderer;

        private static readonly Color ColorSpace =  Color.gray;
        private static readonly Color ColorModel = Color.blue;
        private static readonly Color ColorCandidate = Color.yellow;
        private static readonly Color ColorMatched = Color.green;
        
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock _propBlock;

        private void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
        }
        
        public void ApplyMatrix(Matrix4x4 matrix)
        {
            transform.SetPositionAndRotation(
                matrix.GetColumn(3),
                matrix.rotation
            );
            
            Vector3 scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
            );
            transform.localScale = scale;
        }

        public void SetState(CubeState state)
        {
            Color target = state switch
            {
                CubeState.Space => ColorSpace,
                CubeState.Model => ColorModel,
                CubeState.Candidate => ColorCandidate,
                CubeState.Matched => ColorMatched,
                _ => ColorSpace
            };

            if (cubeRenderer == null) return;

            cubeRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(Color1, target);
            cubeRenderer.SetPropertyBlock(_propBlock);
        }
        
        public void PlayMatchAnimation()
        {
            transform.DOKill();
            transform
                .DOScale(transform.localScale * 1.6f, 0.15f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                    transform.DOScale(transform.localScale / 1.6f, 0.2f).SetEase(Ease.InBack));
        }
        
        public void OnSpawned()
        {
            gameObject.SetActive(true);
            transform.DOKill();
            transform.localScale = Vector3.one * 0.3f;
        }

        public void OnDespawned()
        {
            transform.DOKill();
            gameObject.SetActive(false);
        }
    }
}