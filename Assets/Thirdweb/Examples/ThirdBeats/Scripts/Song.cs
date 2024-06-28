using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections.Generic;

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
        internal static Dictionary<string, bool> _isBeingUnlocked = new();

        private AudioSource _musicSource;

        private Button _songButton;
        private TMP_Text _songText;
        private Image _songImage;
        private UnityAction _unlockAction;
        private BigInteger _price;

        private void Awake()
        {
            _musicSource = GameObject.Find("MusicSource")?.GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                ThirdwebDebug.LogError("MusicSource not found in the scene.");
            }

            _songButton = GetComponent<Button>();
            _songButton.onClick.AddListener(SelectSong);

            _songText = GetComponentInChildren<TMP_Text>();

            _songImage = GetComponent<Image>();
        }

        public void SetupSong(AudioClip clip, BigInteger price, UnityAction unlockAction)
        {
            _isBeingUnlocked.TryAdd(clip.name, false);
            _unlockAction = unlockAction;
            _price = price;
            _originalState = price == 0 || _isBeingUnlocked[clip.name] ? SongState.Unlocked : SongState.Locked;
            Clip = clip;
            SetState(_originalState);
        }

        public void ResetState()
        {
            SetState(_originalState);
        }

        private void SelectSong()
        {
            if (_currentState == SongState.Locked && !_isBeingUnlocked[Clip.name])
            {
                if (MenuManager.Instance.Balance < _price)
                {
                    ThirdwebDebug.LogWarning("Cannot unlock song. Not enough funds.");
                    return;
                }
                Debug.Log("Unlocking song in the background...");
                _unlockAction?.Invoke();
                _isBeingUnlocked[Clip.name] = true;
                _originalState = SongState.Unlocked;
            }

            if (_selectedSong != null)
            {
                _selectedSong.ResetState();
            }

            _selectedSong = this;
            SetState(SongState.Unlocked);
            MenuManager.Instance.OnSongSelected.Invoke();
        }

        private void SetState(SongState state)
        {
            _currentState = state;

            if (_songText != null)
            {
                var lengthInMinutes = Mathf.Floor(Clip.length / 60);
                var extraSeconds = Mathf.Floor(Clip.length % 60);
                if (SongState.Locked == state)
                {
                    _songText.text = Clip.name + $" ({_price.ToString().ToEth(0, true)} $NOTES)";
                }
                else if (SongState.Playing == state)
                {
                    _songText.text = Clip.name + " (Playing)";
                }
                else if (SongState.Unlocked == state)
                {
                    _songText.text = Clip.name + $" ({lengthInMinutes}:{extraSeconds:00})";
                }
            }
            else
            {
                ThirdwebDebug.LogWarning("TMP_Text component not found.");
            }

            if (StateColors.TryGetValue(state, out Color color))
            {
                _songImage.color = color;
            }
            else
            {
                ThirdwebDebug.LogWarning($"Color for state {state} not found in stateColors dictionary.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentState == SongState.Unlocked)
            {
                SetState(SongState.Playing);
                PreviewSong();
            }
        }

        private void PreviewSong()
        {
            if (_musicSource != null && Clip != null)
            {
                _musicSource.clip = Clip;
                _musicSource.loop = false;
                _musicSource.Play();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_currentState == SongState.Playing)
            {
                SetState(_originalState);
            }
        }

        [Serializable]
        private enum SongState
        {
            Unlocked,
            Playing,
            Locked
        }

        [Serializable]
        private class SongStateColorDictionary : SerializableDictionaryBase<SongState, Color> { }
    }
}
