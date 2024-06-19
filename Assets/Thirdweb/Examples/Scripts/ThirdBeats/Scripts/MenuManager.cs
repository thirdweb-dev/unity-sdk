using System;
using UnityEngine;
using UnityEngine.Events;

namespace Thirdweb.Unity.Examples
{
    public class MenuManager : MonoBehaviour
    {
        [field: SerializeField]
        private UnityEvent OnLoggedIn;

        [field: SerializeField]
        private AudioClip menuMusic;

        [field: SerializeField]
        private AudioSource audioSource;

        private IThirdwebWallet _wallet;

        private void Awake()
        {
            ResetMenu();
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
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
