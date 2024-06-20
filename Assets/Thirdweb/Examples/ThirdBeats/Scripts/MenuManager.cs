using System;
using UnityEngine;
using UnityEngine.Events;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Numerics;
using TMPro;

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
        private TMP_Text[] _balanceTexts;

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

        private void Start()
        {
            MusicGameManager.Instance.OnGameEnded.AddListener(ClaimTokens);
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
                    ThirdwebDebug.Log($"Personal Address: {address}");
                }
                else if (method == "Guest")
                {
                    // Guest
                    _wallet = await PrivateKeyWallet.Generate(client: ThirdwebManager.Instance.Client);
                    var address = await _wallet.GetAddress();
                    ThirdwebDebug.Log($"Personal Address: {address}");
                }
                else
                {
                    throw new Exception("Invalid login method");
                }

                _wallet = await SmartWallet.Create(client: ThirdwebManager.Instance.Client, personalWallet: _wallet, chainId: 37714555429, gasless: true);
                var finalAddress = await _wallet.GetAddress();
                ThirdwebDebug.Log($"Logged in as: {finalAddress}");

                OnLoggedIn.Invoke();
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error logging in: {e.Message}");
            }

            InvokeRepeating(nameof(UpdateBalance), 0f, 5f);
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

        private async void ClaimTokens(GameResult gameResult)
        {
            if (gameResult == null || gameResult.cancelled)
            {
                return;
            }

            try
            {
                if (_wallet == null)
                {
                    ThirdwebDebug.LogWarning("Wallet not found, skipping token update.");
                    return;
                }

                if (gameResult.score > 0)
                {
                    var multiplier = (gameResult.maxCombo > 10 ? gameResult.maxCombo : 10) / 10f;
                    var finalClaimAmount = (int)(gameResult.score * multiplier);
                    // DropERC20
                    var contract = await ThirdwebContract.Create(client: ThirdwebManager.Instance.Client, chain: 37714555429, address: "0x71EA4B7c2B5d67CcEe4C362203e9fc3e24BD56D3");

                    // Raw claim params
                    var receiver = await _wallet.GetAddress();
                    var quantity = finalClaimAmount;
                    var currency = Constants.NATIVE_TOKEN_ADDRESS;
                    var pricePerToken = BigInteger.Zero;
                    var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
                    var data = new byte[] { };

                    // Claim
                    var result = await ThirdwebContract.Write(_wallet, contract, "claim", 0, receiver, quantity, currency, pricePerToken, allowlistProof, data);

                    ThirdwebDebug.Log($"Added {finalClaimAmount} tokens to wallet. Transaction hash: {result.TransactionHash}");
                }
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error claiming tokens: {e.Message}");
            }
        }

        private async void UpdateBalance()
        {
            try
            {
                if (_wallet == null)
                {
                    ThirdwebDebug.LogWarning("Wallet not found, skipping balance update.");
                    return;
                }

                // DropERC20
                var contract = await ThirdwebContract.Create(client: ThirdwebManager.Instance.Client, chain: 37714555429, address: "0x71EA4B7c2B5d67CcEe4C362203e9fc3e24BD56D3");

                // Get balance
                var balance = await contract.ERC20_BalanceOf(await _wallet.GetAddress());

                // Update UI
                foreach (var text in _balanceTexts)
                {
                    text.text = $"$NOTES Balance: {balance}";
                }
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error updating balance: {e.Message}");
            }
        }
    }
}
