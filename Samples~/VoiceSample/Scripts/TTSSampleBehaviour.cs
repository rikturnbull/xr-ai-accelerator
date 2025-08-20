using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace XrAiAccelerator.Samples
{
    public class TTSSampleBehaviour : MonoBehaviour
    {
        [SerializeField] AudioSource _audioSource;
        [SerializeField] string _message = "Hello, this is a sample text-to-speech message.";

        private XrAiModelManager _modelManager;

        void Start()
        {
            _modelManager = XrAiModelManager.GetModelManager();
            StartCoroutine(SpeakMessageCoroutine());
        }

        private IEnumerator SpeakMessageCoroutine()
        {
            IXrAiTextToSpeech textToSpeech = XrAiFactory.LoadTextToSpeech("OpenAI");
            textToSpeech.Initialize(_modelManager.GetGlobalProperties("OpenAI")).Wait();

            Task task = textToSpeech.Execute(
                _message,
                _modelManager.GetWorkflowProperties("OpenAI", XrAiFactory.WORKFLOW_TEXT_TO_SPEECH),
                OnTextToSpeechCompleted
            );
            yield return new WaitUntil(() => task.IsCompleted);
        }

        private void OnTextToSpeechCompleted(XrAiResult<AudioClip> result)
        {
            if (result.IsSuccess)
            {
                _audioSource.clip = result.Data;
                _audioSource.Play();
            }
            else
            {
                Debug.LogError($"Text-to-speech failed: {result.ErrorMessage}");
            }
        }
    }
}