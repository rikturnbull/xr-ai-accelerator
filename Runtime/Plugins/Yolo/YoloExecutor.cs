using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;

namespace XrAiAccelerator
{
    public class YoloExecutor
    {
        private int _layersPerFrame = 25;

        private bool _isModelLoaded = false;

        private Worker _inferenceEngineWorker;
        private IEnumerator _schedule;
        private string[] _labels;

        private Tensor<float> _input;
        private Vector2Int _inputTextureSize;

        private Tensor[] _outputs = new Tensor[4]; // Store all outputs

        private BackendType _backendType = BackendType.CPU; // Default backend type

        private const string MODEL_FILE_NAME = "yolo11n-seg.sentis";
        private const string LABELS_FILE_NAME = "yolo11n-labels.txt";

#if UNITY_EDITOR
        [InitializeOnLoad]
        public static class PackageStreamingAssetsCopier
        {
            static PackageStreamingAssetsCopier()
            {
                Assembly packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(YoloExecutor).Assembly);
                if(packageInfo == null)
                {
                    Debug.LogWarning("Could not find package info for YoloExecutor, copy StreamingAssets manually.");
                    return
                }

                string source = Path.Combine(packageInfo.resolvedPath, "StreamingAssets");                
                string dest = "Assets/StreamingAssets";

                if (!Directory.Exists(dest))
                {
                    Directory.CreateDirectory(dest);
                }
                if (Directory.Exists(source))
                {
                    foreach (var file in new[] { MODEL_FILE_NAME, LABELS_FILE_NAME })
                    {
                        if (!File.Exists(Path.Combine(dest, file)))
                        {
                            Debug.Log($"Copying {file} from {source} to {dest}");
                            FileUtil.CopyFileOrDirectory(Path.Combine(source, file), Path.Combine(dest, file));
                        }
                    }
                }
            }
        }
#endif

        public async Task LoadModel()
        {
            Task<byte[]> modelDataTask = LoadStreamingAssetAsync<byte[]>(MODEL_FILE_NAME);
            Task<string> labelContentTask = LoadStreamingAssetAsync<string>(LABELS_FILE_NAME);

            byte[] modelData = await modelDataTask;
            string labelContent = await labelContentTask;

            using MemoryStream modelStream = new(modelData);
            Model model = ModelLoader.Load(modelStream);

            // Model model = ModelLoader.Load(yoloAssets.ModelAsset);
            _inferenceEngineWorker = new Worker(model, _backendType);
            _labels = labelContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            _isModelLoaded = true;
            await WarmUpModel();
        }

        private async Task<T> LoadStreamingAssetAsync<T>(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

            using UnityWebRequest request = UnityWebRequest.Get(filePath);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load {fileName}: {request.error}");
                throw new System.Exception($"Failed to load {fileName}: {request.error}");
            }

            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)request.downloadHandler.data;
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)request.downloadHandler.text;
            }
            else
            {
                throw new ArgumentException($"Unsupported type {typeof(T)}. Only byte[] and string are supported.");
            }
        }

        public async Task WarmUpModel()
        {
            if (!_isModelLoaded) return;

            // Create dummy input for warmup
            var dummyInput = new Tensor<float>(new TensorShape(1, 3, 640, 640));
            var schedule = _inferenceEngineWorker.ScheduleIterable(dummyInput);

            int it = 0;
            while (schedule.MoveNext())
            {
                if (++it % _layersPerFrame == 0)
                {
                    await Task.Yield();
                }
            }

            dummyInput.Dispose();
        }

        public async Task Execute(Texture inputTexture, Action<XrAiResult<XrAiBoundingBox[]>> onComplete)
        {
            // Error if model is not loaded
            if (!_isModelLoaded)
            {
                Debug.LogError("Sentis: Model is not loaded");
                onComplete?.Invoke(XrAiResult.Failure<XrAiBoundingBox[]>("Model not loaded"));
                return;
            }

            // Clean last input and outputs
            _input?.Dispose();
            CleanupOutputs();

            // Set the input texture size
            _inputTextureSize = new Vector2Int(inputTexture.width, inputTexture.height);

            // Create a new tensor and convert the texture using the new API
            _input = new Tensor<float>(new TensorShape(1, 3, 640, 640));
            TextureConverter.ToTensor(inputTexture, _input);

            _schedule = _inferenceEngineWorker.ScheduleIterable(_input);

            // Run the inference layer by layer to not block the main thread
            int it = 0;
            while (_schedule.MoveNext())
            {
                if (++it % _layersPerFrame == 0)
                {
                    await Task.Yield(); // Wait one frame
                }
            }

            // Process all outputs using simple loop
            for (int i = 0; i < _outputs.Length; i++)
            {
                await ReadOutput(i, (tensor) => _outputs[i] = tensor);

                if (_outputs[i] == null || _outputs[i].shape[0] == 0)
                {
                    onComplete?.Invoke(XrAiResult.Success(Array.Empty<XrAiBoundingBox>()));
                    CleanupOutputs();
                    return;
                }
            }

            // Convert to bounding boxes using array indexing
            try
            {
                List<XrAiBoundingBox> boundingBoxes = YoloBoxer.ToBoundBoxes(
                    _labels,
                    _outputs[0] as Tensor<float>,  // BoxCoords
                    _outputs[1] as Tensor<int>,    // LabelIds
                    _inputTextureSize.x,
                    _inputTextureSize.y
                );
                onComplete?.Invoke(XrAiResult.Success(boundingBoxes.ToArray()));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onComplete?.Invoke(XrAiResult.Failure<XrAiBoundingBox[]>(ex.Message));
            }

            // Cleanup
            CleanupOutputs();
        }

        private void CleanupOutputs()
        {
            for (int i = 0; i < _outputs.Length; i++)
            {
                _outputs[i]?.Dispose();
                _outputs[i] = null;
            }
        }

        private async Task ReadOutput(int outputIndex, Action<Tensor> callback)
        {
            Tensor buffer = _inferenceEngineWorker.PeekOutput(outputIndex);

            if (buffer?.dataOnBackend == null)
            {
                callback(null);
                return;
            }

            // Request readback
            buffer.ReadbackRequest();

            // Wait for readback to complete
            while (!buffer.IsReadbackRequestDone())
            {
                await Task.Yield(); // Yield until the readback is done
            }

            // Return the cloned tensor via callback
            callback(buffer.ReadbackAndClone());
        }
    }
}