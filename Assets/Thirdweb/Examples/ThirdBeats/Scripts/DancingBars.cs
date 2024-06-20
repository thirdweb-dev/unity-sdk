using System.Collections.Generic;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class DancingBars : MonoBehaviour
    {
        [SerializeField, Header("Audio")]
        private AudioSource TrackedAudio;

        [field: SerializeField, Header("UI")]
        private RectTransform[] Bars { get; set; }

        private readonly Dictionary<RectTransform, float> _scaleSpeeds = new();
        private readonly Dictionary<RectTransform, float> _targetScales = new();
        private readonly float[] _spectrumData = new float[64];
        private readonly float[] _emaSpectrumData = new float[64];

        private void Awake()
        {
            foreach (var bar in Bars)
            {
                _scaleSpeeds.Add(bar, Random.Range(2f, 4f));
                _targetScales.Add(bar, 1f);
            }
        }

        private void Update()
        {
            GetSpectrumDataExtension.GetSpectrumData(TrackedAudio, _spectrumData);
            SmoothSpectrumData();
            AnimateBars();
        }

        private void SmoothSpectrumData()
        {
            float smoothingFactor = 0.1f; // Exponential moving average factor
            for (int i = 0; i < _spectrumData.Length; i++)
            {
                _emaSpectrumData[i] = Mathf.Lerp(_emaSpectrumData[i], _spectrumData[i], smoothingFactor);
            }
        }

        private void AnimateBars()
        {
            for (int i = 0; i < Bars.Length; i++)
            {
                var bar = Bars[i];
                float scaleSpeed = _scaleSpeeds[bar];
                float intensity = _emaSpectrumData[i % _emaSpectrumData.Length] * 10f; // Adjust multiplier for more intensity
                float targetScale = Mathf.Lerp(1f, 7f, intensity); // Adjust max scale for desired effect

                // Apply punch effect
                if (targetScale > _targetScales[bar])
                {
                    _targetScales[bar] = targetScale;
                }
                else
                {
                    // Apply decay
                    _targetScales[bar] = Mathf.Lerp(_targetScales[bar], 1f, 0.2f); // Adjust decay speed for lingering effect
                }

                Vector3 newScale = Vector3.Lerp(bar.localScale, new Vector3(1, _targetScales[bar], 1), scaleSpeed * Time.deltaTime);
                bar.localScale = new Vector3(1, newScale.y, 1); // Only scale vertically
            }
        }
    }
}
