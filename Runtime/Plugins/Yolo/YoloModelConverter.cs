using UnityEngine;
using Unity.InferenceEngine;

namespace XrAiYolo
{

    public class YoloModelConverter : MonoBehaviour
    {
        public ModelAsset _onnxModel;
        [SerializeField, Range(0, 1)] private float _iouThreshold = 0.6f;
        [SerializeField, Range(0, 1)] private float _scoreThreshold = 0.23f;
        [SerializeField] private string _filePath = "Assets/Resources/Model/yolo11n-seg-sentis.sentis";
    }
}