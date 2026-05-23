using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using UnityEngine;

namespace Services
{
    public interface IExportService
    {
        UniTask<string> ExportOffsetsAsync(List<OffsetResult> results, string fileName = "offsets_result.json");
    }

    public class JsonExportService : IExportService
    {
        public async UniTask<string> ExportOffsetsAsync(List<OffsetResult> results, string fileName = "offsets_result.json")
        {
            string outputPath = Path.Combine(Application.persistentDataPath, fileName);

            var payload = new ExportPayload
            {
                totalFound = results.Count,
                offsets = new List<MatrixPayload>(results.Count)
            };

            foreach (var r in results)
                payload.offsets.Add(MatrixPayload.FromUnityMatrix(r.Offset));

            string json = await UniTask.RunOnThreadPool(() =>
                JsonConvert.SerializeObject(payload, Formatting.Indented));

            await UniTask.RunOnThreadPool(() => File.WriteAllText(outputPath, json));

            Debug.Log($"[Export] Saved {results.Count} offsets → {outputPath}");
            return outputPath;
        }
    }
}