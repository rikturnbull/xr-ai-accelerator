using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class XrAiImageToTextSample : MonoBehaviour
{
    [SerializeField] private string _apiKey;
    [SerializeField] private string _model = "meta-llama/llama-4-scout-17b-16e-instruct";
    [SerializeField] private string _prompt = "What's in this image?";
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private ParticleSystem _loadingParticles;
    [SerializeField] private Transform _position;
    [SerializeField] private TMP_Text _resultText;

    private Task<XrAiTaskResult> _task;

    private void Start()
    {
        Invoke(nameof(ExecuteGroqVision), 1f);
    }

    private void ExecuteGroqVision()
    {
        IXrAiImageToText imageToText = XrAiFactory.LoadImageToText("Groq", new System.Collections.Generic.Dictionary<string, string>
            {
                { "apiKey", _apiKey }
            }
        );
        _task = imageToText.Execute(_rawImage.texture as Texture2D,
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "model", _model },
                { "prompt", _prompt }
            }
        );
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
            XrAiTaskResult result = _task.Result;
            if (result == null)
            {
                Debug.LogError("No result returned from image to text.");
                _task = null;
                return;
            }
            string answer = result.StringResult;
            _resultText.text = answer;
            Debug.Log($"Answer: {answer}");
            _task = null;
        }
    }
}
