
using System;
using System.IO;
using UnityEngine;

namespace XrAiAccelerator
{
    public class XrAiSpeechToTextHelper : MonoBehaviour
    {
        private AudioClip _clip;
        private bool _isRecording = false;
        private int _recordingMax = 5;
        private string _encoding = "wav";
        private float _time = 0f;
        private string _device = null;
        private int _frequency = 16000;
        private Action<byte[]> _onRecordingComplete;

        public void StartRecording(Action<byte[]> onRecordingComplete = null, string device = null, string encoding = null, int? recordingMax = null, int? frequency = null)
        {
            if (_isRecording)
            {
                Debug.LogWarning("Recording is already in progress.");
                return;
            }

            _device = device ?? GetMicrophone();
            _encoding = encoding ?? "wav";
            _frequency = frequency ?? 16000;
            _isRecording = true;
            _onRecordingComplete = onRecordingComplete;
            _recordingMax = recordingMax ?? 5;
            _time = 0f;

            _clip = Microphone.Start(_device, false, _recordingMax, _frequency);
        }

        public void StopRecording()
        {
            _isRecording = false;
            if (_clip != null)
            {
                Microphone.End(_device);
                if (_encoding == "wav")
                {
                    byte[] wavData = ConvertToWAV(_clip);
                    _onRecordingComplete?.Invoke(wavData);
                    return;
                }
                byte[] audioData = ConvertToRawPCM(_clip);
                _onRecordingComplete?.Invoke(audioData);
            }
        }

        private string GetMicrophone()
        {
            string[] availableMicrophones = Microphone.devices;
            if (availableMicrophones.Length == 0)
            {
                Debug.LogError("No microphones found.");
                return null;
            }

            foreach (string microphone in availableMicrophones)
            {
                if(microphone.ToLower().Contains("oculus"))
                {
                    return microphone;
                }
            }
            return availableMicrophones[0];
        }

        private void Update()
        {
            if (_isRecording)
            {
                _time += Time.deltaTime;
                if (_time >= _recordingMax)
                {
                    StopRecording();
                }
            }
        }
        
        private static byte[] ConvertToWAV(AudioClip clip)
        {
            if (!clip)
            {
                Debug.LogError("SaveWav: AudioClip is null! Cannot save.");
                return null;
            }

            using var memoryStream = CreateMemoryStream();
            ConvertAndWrite(memoryStream, clip);
            WriteWavHeader(memoryStream, clip);
            return memoryStream.ToArray();
        }

        public static byte[] ConvertToRawPCM(AudioClip clip)
        {
            if (!clip)
            {
                Debug.LogError("ConvertToRawPCM: AudioClip is null! Cannot convert.");
                return null;
            }

            var samples = new float[clip.samples];
            clip.GetData(samples, 0);

            var intData = new short[samples.Length];
            var bytesData = new byte[samples.Length * 2];

            var rescaleFactor = 32767;
            
            for (var i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                BitConverter.GetBytes(intData[i]).CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        private static MemoryStream CreateMemoryStream()
        {
            var memoryStream = new MemoryStream();
            for (var i = 0; i < 44; i++)
            {
                memoryStream.WriteByte(0);
            }

            return memoryStream;
        }

        private static void ConvertAndWrite(MemoryStream memoryStream, AudioClip clip)
        {
            if (!clip)
            {
                Debug.LogError("SaveWav: AudioClip is null! Cannot convert.");
                return;
            }

            var samples = new float[clip.samples];
            clip.GetData(samples, 0);

            var intData = new short[samples.Length];
            var bytesData = new byte[samples.Length * 2];

            var rescaleFactor = 32767;
            for (var i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                BitConverter.GetBytes(intData[i]).CopyTo(bytesData, i * 2);
            }

            memoryStream.Write(bytesData, 0, bytesData.Length);
        }

        private static void WriteWavHeader(MemoryStream memoryStream, AudioClip clip)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(memoryStream.Length - 8), 0, 4);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(16), 0, 4);
            memoryStream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
            memoryStream.Write(BitConverter.GetBytes(clip.channels), 0, 2);
            memoryStream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4);
            memoryStream.Write(BitConverter.GetBytes((ushort)(clip.channels * 2)), 0, 2);
            memoryStream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            memoryStream.Write(BitConverter.GetBytes(clip.samples * clip.channels * 2), 0, 4);
        }
    }
}