using UnityEngine;
using Unity.InferenceEngine;

namespace XrAiAccelerator
{

    public class YoloModelConverter : MonoBehaviour
    {
        public ModelAsset _onnxModel;
        #pragma warning disable CS0414
        [SerializeField, Range(0, 1)] private float _iouThreshold = 0.6f;
        [SerializeField, Range(0, 1)] private float _scoreThreshold = 0.23f;
        [SerializeField] private string _filePath = "Assets/Resources/Model/yolo11n-seg-sentis.sentis";
        #pragma warning restore CS0414
    }
}