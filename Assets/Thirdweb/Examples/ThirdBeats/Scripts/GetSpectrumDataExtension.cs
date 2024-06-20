#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public static class GetSpectrumDataExtension
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool StartSampling(string name, int bufferSize, int sampleRate);

        [DllImport("__Internal")]
        private static extern bool ProvideAudioData(string name, float[] audioData, int bufferSize, int sampleRate);

        [DllImport("__Internal")]
        private static extern bool GetSamples(string name, float[] freqData, int size);
#endif

        private static Dictionary<string, float[]> audioDataCache = new Dictionary<string, float[]>();
        private static Dictionary<string, float[]> spectrumDataCache = new Dictionary<string, float[]>();
        private static Dictionary<string, long> lastUpdateTime = new Dictionary<string, long>();

        public static void GetSpectrumData(this AudioSource _audioSource, float[] sample)
        {
            if (_audioSource == null || _audioSource.clip == null || !_audioSource.isPlaying)
            {
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            var name = _audioSource.clip.name;
            int bufferSize = sample.Length;
            int sampleRate = _audioSource.clip.frequency;

            if (!StartSampling(name, bufferSize, sampleRate))
            {
                Debug.LogWarning("Failed to start sampling.");
                return;
            }

            float[] audioData;
            if (!audioDataCache.TryGetValue(name, out audioData))
            {
                audioData = new float[_audioSource.clip.samples];
                _audioSource.clip.GetData(audioData, 0);
                audioDataCache[name] = audioData;
            }

            long currentTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Throttle updates to every 500ms to reduce load
            if (lastUpdateTime.ContainsKey(name) && currentTime - lastUpdateTime[name] < 500)
            {
                if (spectrumDataCache.TryGetValue(name, out var cachedSpectrumData))
                {
                    System.Array.Copy(cachedSpectrumData, sample, bufferSize);
                }
                return;
            }
            lastUpdateTime[name] = currentTime;

            if (!ProvideAudioData(name, audioData, audioData.Length, sampleRate))
            {
                Debug.LogWarning("Failed to provide audio data.");
                return;
            }

            if (!GetSamples(name, sample, sample.Length))
            {
                Debug.LogWarning("Failed to get samples.");
                return;
            }

            // Cache the spectrum data
            if (!spectrumDataCache.ContainsKey(name))
            {
                spectrumDataCache[name] = new float[bufferSize];
            }
            System.Array.Copy(sample, spectrumDataCache[name], bufferSize);

#else
            _audioSource.GetSpectrumData(sample, 0, FFTWindow.BlackmanHarris);
#endif
        }
    }
}
