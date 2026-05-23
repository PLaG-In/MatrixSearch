using Pools;
using Services;
using UI;
using UnityEngine;
using Visualization;
using Zenject;

namespace Installers
{
    public class MainInstaller : MonoInstaller
    {
        [Header("Prefabs")]
        [SerializeField] private CubeView  cubePrefab;
        [SerializeField] private Transform cubePoolParent;
        [SerializeField] private int cubePoolInitialSize = 200;

        [Header("Views")]
        [SerializeField] private VisualizationView vizView;
        [SerializeField] private SearchUIView searchUIView;

        public override void InstallBindings()
        {
            Container.Bind<IDataLoaderService>()
                .To<DataLoaderService>()
                .AsSingle();

            Container.Bind<IMatrixSearchService>()
                .To<MatrixSearchService>()
                .AsSingle();

            Container.Bind<IExportService>()
                .To<JsonExportService>()
                .AsSingle();
            
            Container.Bind<VisualizationModel>()
                .AsSingle();
            
            Container.BindMemoryPool<CubeView, CubePool>()
                .WithInitialSize(cubePoolInitialSize)
                .FromComponentInNewPrefab(cubePrefab)
                .UnderTransform(cubePoolParent);
            
            Container.BindInstance(vizView).AsSingle();
            Container.BindInstance(searchUIView).AsSingle();
            
            Container.BindInterfacesAndSelfTo<VisualizationPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<SearchUIPresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}