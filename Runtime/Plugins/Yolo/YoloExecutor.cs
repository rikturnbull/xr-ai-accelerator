using System;
using System.Collections;
using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;

namespace XrAiYolo
{
    public class YoloExecutor : MonoBehaviour
    {
        enum InferenceDownloadState
        {
            Running = 0,
            RequestingOutput0 = 1,
            RequestingOutput1 = 2,
            RequestingOutput2 = 3,
            RequestingOutput3 = 4,
            Success = 5,
            Error = 6,
            Cleanup = 7,
            Completed = 8
        }

        private int _layersPerFrame = 25;

        public bool IsModelLoaded { get; private set; } = false;

        private Worker _inferenceEngineWorker;
        private IEnumerator _schedule;
        private string[] _labels;

        private Tensor<float> _input;
        private Vector2Int _inputTextureSize;

        private Tensor _buffer;
        private Tensor<float> _output0BoxCoords;
        private Tensor<int> _output1LabelIds;
        private Tensor<float> _output2Masks;
        private Tensor<float> _output3MaskWeights;

        private InferenceDownloadState _downloadState = InferenceDownloadState.Running;
        private bool _started = false;
        private bool _isWaitingForReadbackRequest = false;
        private XrAiResult<XrAiBoundingBox[]> _result;

        public void LoadModel(YoloAssets yoloAssets)
        {
            // Load model
            Model model = ModelLoader.Load(yoloAssets.ModelAsset);

            // Create engine to run model
            _inferenceEngineWorker = new Worker(model, BackendType.CPU);

            // Run a inference with an empty input to load the model in the memory and not pause the main thread.
            Tensor input = TextureConverter.ToTensor(new Texture2D(640, 640), 640, 640, 3);
            _inferenceEngineWorker.Schedule(input);

            // Load labels
            _labels = yoloAssets.LabelsAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            IsModelLoaded = true;
        }

        public void RunInference(Texture inputTexture)
        {
            // If the inference is not running prepare the input
            if (!_started)
            {
                // Clean last input
                _input?.Dispose();

                // Check if we have a texture from the camera
                if (!inputTexture) return;

                // Set the input texture size
                _inputTextureSize = new Vector2Int(inputTexture.width, inputTexture.height);

                // Convert the texture to a Tensor and schedule the inference
                _input = TextureConverter.ToTensor(inputTexture, 640, 640, 3);
                _schedule = _inferenceEngineWorker.ScheduleIterable(_input);

                _downloadState = InferenceDownloadState.Running;
                _result = null;
                _started = true;
            }
        }

        public bool IsRunning() => _started;

        public XrAiResult<XrAiBoundingBox[]> GetResult()
        {
            return _result;
        }

        private void Update()
        {
            // Run the inference layer by layer to not block the main thread.
            if (!_started) return;
            if (_downloadState == InferenceDownloadState.Running)
            {
                int it = 0;
                while (_schedule.MoveNext()) if (++it % _layersPerFrame == 0) return;

                // If we reach here, all layers have been processed
                _downloadState = InferenceDownloadState.RequestingOutput0;
            }
            else
            {
                // Get the result once all layers are processed
                UpdateProcessInferenceResults();
            }
        }

        private void UpdateProcessInferenceResults()
        {
            switch (_downloadState)
            {
                case InferenceDownloadState.RequestingOutput0:
                    if (!_isWaitingForReadbackRequest)
                    {
                        _buffer = GetOutputBuffer(0);
                        InitiateReadbackRequest(_buffer);
                    }
                    else
                    {
                        if (_buffer.IsReadbackRequestDone())
                        {
                            _output0BoxCoords = _buffer.ReadbackAndClone() as Tensor<float>;
                            _isWaitingForReadbackRequest = false;

                            if (_output0BoxCoords.shape[0] > 0)
                            {
                                _downloadState = InferenceDownloadState.RequestingOutput1;
                            }
                            else
                            {
                                _downloadState = InferenceDownloadState.Error;
                            }
                            _buffer?.Dispose();
                        }
                    }
                    break;
                case InferenceDownloadState.RequestingOutput1:
                    if (!_isWaitingForReadbackRequest)
                    {
                        _buffer = GetOutputBuffer(1) as Tensor<int>;
                        InitiateReadbackRequest(_buffer);
                    }
                    else
                    {
                        if (_buffer.IsReadbackRequestDone())
                        {
                            _output1LabelIds = _buffer.ReadbackAndClone() as Tensor<int>;
                            _isWaitingForReadbackRequest = false;

                            if (_output1LabelIds.shape[0] > 0)
                            {
                                _downloadState = InferenceDownloadState.RequestingOutput2;
                            }
                            else
                            {
                                _downloadState = InferenceDownloadState.Error;
                            }
                            _buffer?.Dispose();
                        }
                    }
                    break;
                case InferenceDownloadState.RequestingOutput2:
                    if (!_isWaitingForReadbackRequest)
                    {
                        _buffer = GetOutputBuffer(2) as Tensor<float>;
                        InitiateReadbackRequest(_buffer);
                    }
                    else
                    {
                        if (_buffer.IsReadbackRequestDone())
                        {
                            _output2Masks = _buffer.ReadbackAndClone() as Tensor<float>;
                            _isWaitingForReadbackRequest = false;

                            if (_output2Masks.shape[0] > 0)
                            {
                                _downloadState = InferenceDownloadState.RequestingOutput3;
                            }
                            else
                            {
                                _downloadState = InferenceDownloadState.Error;
                            }
                            _buffer?.Dispose();
                        }
                    }
                    break;
                case InferenceDownloadState.RequestingOutput3:
                    if (!_isWaitingForReadbackRequest)
                    {
                        _buffer = GetOutputBuffer(3) as Tensor<float>;
                        InitiateReadbackRequest(_buffer);
                    }
                    else
                    {
                        if (_buffer.IsReadbackRequestDone())
                        {
                            _output3MaskWeights = _buffer.ReadbackAndClone() as Tensor<float>;
                            _isWaitingForReadbackRequest = false;

                            if (_output3MaskWeights.shape[0] > 0)
                            {
                                _downloadState = InferenceDownloadState.Success;
                            }
                            else
                            {
                                _downloadState = InferenceDownloadState.Error;
                            }
                            _buffer?.Dispose();
                        }
                    }
                    break;
                case InferenceDownloadState.Success:
                    List<XrAiBoundingBox> boundingBoxes = YoloBoxer.ToBoundBoxes(_labels, _output0BoxCoords, _output1LabelIds, _inputTextureSize.x, _inputTextureSize.y);
                    _result = XrAiResult.Success(boundingBoxes.ToArray());
                    _downloadState = InferenceDownloadState.Cleanup;
                    break;
                case InferenceDownloadState.Error:
                    _downloadState = InferenceDownloadState.Cleanup;
                    _result = null;
                    break;
                case InferenceDownloadState.Cleanup:
                    _downloadState = InferenceDownloadState.Completed;
                    _started = false;
                    _output0BoxCoords?.Dispose();
                    _output1LabelIds?.Dispose();
                    _output2Masks?.Dispose();
                    _output3MaskWeights?.Dispose();
                    break;
            }
        }

        private Tensor GetOutputBuffer(int outputIndex)
        {
            return _inferenceEngineWorker.PeekOutput(outputIndex);
        }

        private void InitiateReadbackRequest(Tensor pullTensor)
        {
            if (pullTensor.dataOnBackend != null)
            {
                pullTensor.ReadbackRequest();
                _isWaitingForReadbackRequest = true;
            }
            else
            {
                _downloadState = InferenceDownloadState.Error;
            }
        }

        private void OnDestroy()
        {
            if (_schedule != null)
            {
                StopCoroutine(_schedule);
            }
            _input?.Dispose();
            _inferenceEngineWorker?.Dispose();
        }
    }
}