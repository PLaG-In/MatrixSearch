using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Services
{
    public interface IDataLoaderService
    {
        UniTask<List<Matrix4x4>> LoadMatricesAsync(string fileName);
    }

    public class DataLoaderService : IDataLoaderService
    {
        public async UniTask<List<Matrix4x4>> LoadMatricesAsync(string fileName)
        {
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            
            string json = await UniTask.RunOnThreadPool(() => File.ReadAllText(path));

            return await UniTask.RunOnThreadPool(() => ParseMatrices(json));
        }
        
        private static List<Matrix4x4> ParseMatrices(string json) =>
            JArray.Parse(json)
                .Select(t => t.ToObject<MatrixPayload>())
                .Select(d => d.ToUnityMatrix())
                .ToList();
    }
}