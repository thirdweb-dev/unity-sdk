using System;
using UnityEngine;
using UnityEngine.Events;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;

namespace Thirdweb.Unity.Examples
{
    public class MenuManager : MonoBehaviour
    {
        [field: SerializeField]
        private AudioClip MenuMusic;

        [field: SerializeField]
        private AudioSource Source;

        [field: SerializeField]
        private Transform SongContent;

        [field: SerializeField]
        private Song SongPrefab;

        [field: SerializeField]
        private List<AudioClip> MusicTracks;

        [field: SerializeField]
        internal UnityEvent OnLoggedIn;

        [field: SerializeField]
        internal UnityEvent OnSongSelected;

        private IThirdwebWallet _wallet;

        internal static MenuManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            ResetMenu();
            OnLoggedIn.AddListener(PopulateSongList);
        }

        public async void OnLogin(string method)
        {
            try
            {
                if (ThirdwebManager.Instance == null || ThirdwebManager.Instance.Client == null)
                {
                    throw new Exception("Missing ThirdwebManager Client, make sure you set it up in the scene correctly.");
                }

                if (method == "Google")
                {
                    // Google login
                    _wallet = await InAppWallet.Create(client: ThirdwebManager.Instance.Client, authprovider: AuthProvider.Google, storageDirectoryPath: Application.persistentDataPath);
                    // Note: simpler api than doing it directly with InAppWallet.LoginWithOauth
                    var address = await InAppWalletModal.Instance.Connect(_wallet as InAppWallet, authprovider: AuthProvider.Google);
                    ThirdwebDebug.Log($"Logged in! Address: {address}");
                }
                else if (method == "Guest")
                {
                    // Guest
                    _wallet = await PrivateKeyWallet.Generate(client: ThirdwebManager.Instance.Client);
                    var address = await _wallet.GetAddress();
                    ThirdwebDebug.Log($"Logged in! Address: {address}");
                }
                else
                {
                    throw new Exception("Invalid login method");
                }

                OnLoggedIn.Invoke();
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error logging in: {e.Message}");
            }
        }

        public void ResetMenu()
        {
            Source.clip = MenuMusic;
            Source.loop = true;
            Source.Play();
        }

        private void PopulateSongList()
        {
            try
            {
                foreach (Transform child in SongContent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var song in MusicTracks)
                {
                    var songInstance = Instantiate(SongPrefab, SongContent);
                    songInstance.SetupSong(clip: song, isAvailable: true);
                }
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error populating song list: {e.Message}");
            }
        }
    }
}
