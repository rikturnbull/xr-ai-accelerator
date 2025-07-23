
using System;
using System.IO;
using UnityEngine;

public class XrAiSpeechToTextHelper : MonoBehaviour
{
    private AudioClip _clip;
    private bool _isRecording = false;
    private int _recordingMax = 5;
    private float _time = 0f;
    private Action<byte[]> _onRecordingComplete;

    public void StartRecording(string device, Action<byte[]> onRecordingComplete = null, int recordingMax = 5)
    {
        if (_isRecording)
        {
            Debug.LogWarning("Recording is already in progress.");
            return;
        }
        _time = 0f;
        _isRecording = true;
        _clip = null;
        _recordingMax = recordingMax;
        _onRecordingComplete = onRecordingComplete;

        Microphone.Start(device, true, recordingMax, 44100);
        _clip = Microphone.Start(device, false, recordingMax, 44100);
    }

    public void StopRecording()
    {
        _isRecording = false;
        if (_clip != null)
        {
            Microphone.End(Microphone.devices[0]);

            byte[] audioData = Convert(_clip);
            _onRecordingComplete?.Invoke(audioData);
        }
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
    
    private static byte[] Convert(AudioClip clip)
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