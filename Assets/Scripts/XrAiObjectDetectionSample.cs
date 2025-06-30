using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Siccity.GLTFUtility;

public class XrAiObjectDetectionSample : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private ParticleSystem _loadingParticles;

    private Task<XrAiObjectDetectorResult> _task;

    private void Start()
    {
        Invoke(nameof(ExecuteYolo11Model), 1f);
    }

    private void ExecuteYolo11Model()
    {
        IXrAiObjectDetector objectDetector = XrAiFactory.LoadObjectDetector("Yolo11", new System.Collections.Generic.Dictionary<string, string>
            {
                { "modelPath", "Assets/Resources/Yolo11/yolo11n-seg-sentis.sentis" },
                { "labelsPath", "Yolo11/yolo11n-labels" }
            });
        _task = objectDetector.Execute(_rawImage.texture as Texture2D);
    }

    private void Update()
    {
        if (_task != null)
        {
            if (!_task.IsCompleted) return;

            _loadingParticles.Stop();
            if (_task.IsFaulted)
            {
                Debug.LogError("Task failed: " + _task.Exception);
                _task = null;
                return;
            }
            XrAiObjectDetectorResult result = _task.Result;
            if (result == null)
            {
                Debug.LogError("No result returned from object detection.");
            }
            else
            {
                Debug.Log($"Detected {result.BoundingBoxes.Length} objects");
                XrAiObjectDetectorHelper.DrawBoxes(_rawImage.transform, result.BoundingBoxes);
            }
            _task = null;
        }
    }
}
