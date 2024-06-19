using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;

namespace Thirdweb.Unity.Examples
{
    [RequireComponent(typeof(Button), typeof(Image))]
    public class Song : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [field: SerializeField]
        public AudioClip Clip { get; private set; } = null;

        [field: SerializeField]
        private SongStateColorDictionary StateColors = new();

        private SongState _currentState;
        private SongState _originalState;

        internal static Song _selectedSong;

        private AudioSource _musicSource;

        private void Awake()
        {
            _musicSource = GameObject.Find("MusicSource")?.GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                ThirdwebDebug.LogError("MusicSource not found in the scene.");
            }

            GetComponent<Button>().onClick.AddListener(SelectSong);
        }

        public void SetupSong(AudioClip clip, bool isAvailable)
        {
            _originalState = isAvailable ? SongState.Available : SongState.Unavailable;
            Clip = clip;
            SetState(_originalState);
        }

        public void ResetState()
        {
            SetState(_originalState);
        }

        private void SelectSong()
        {
            if (_currentState == SongState.Unavailable)
            {
                ThirdwebDebug.LogWarning("Cannot select unavailable song.");
                return;
            }

            if (_selectedSong != null)
            {
                _selectedSong.ResetState();
            }

            _selectedSong = this;
            SetState(SongState.Selected);
            MenuManager.Instance.OnSongSelected.Invoke();
        }

        private void SetState(SongState state)
        {
            _currentState = state;

            var textComponent = GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = $"{Clip.name} ({state})";
            }
            else
            {
                ThirdwebDebug.LogWarning("TMP_Text component not found.");
            }

            if (StateColors.TryGetValue(state, out Color color))
            {
                GetComponent<Image>().color = color;
            }
            else
            {
                ThirdwebDebug.LogWarning($"Color for state {state} not found in stateColors dictionary.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentState == SongState.Available)
            {
                PreviewSong();
                SetState(SongState.Selected);
            }
        }

        private void PreviewSong()
        {
            if (_musicSource != null && Clip != null)
            {
                _musicSource.clip = Clip;
                _musicSource.Play();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_currentState == SongState.Selected)
            {
                SetState(_originalState);
            }
        }

        [Serializable]
        private enum SongState
        {
            Available,
            Selected,
            Unavailable
        }

        [Serializable]
        private class SongStateColorDictionary : SerializableDictionaryBase<SongState, Color> { }
    }
}
