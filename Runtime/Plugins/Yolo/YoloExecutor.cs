using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.InferenceEngine;
using UnityEngine;

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

        public async Task LoadModel(YoloAssets yoloAssets)
        {
            Model model = ModelLoader.Load(yoloAssets.ModelAsset);
            _inferenceEngineWorker = new Worker(model, _backendType);
            _labels = yoloAssets.LabelsAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            _isModelLoaded = true;
            await WarmUpModel();
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
                    onComplete?.Invoke(XrAiResult.Failure<XrAiBoundingBox[]>($"No output{i} available"));
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