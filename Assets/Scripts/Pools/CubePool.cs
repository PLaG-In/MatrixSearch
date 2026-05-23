using Zenject;

namespace Pools
{
    public class CubePool : MonoMemoryPool<CubeView>
    {
        protected override void OnSpawned(CubeView item)
        {
            base.OnSpawned(item);
            item.OnSpawned();
        }

        protected override void OnDespawned(CubeView item)
        {
            item.OnDespawned();
            base.OnDespawned(item);
        }
    }
}