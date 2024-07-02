using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

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
        private string _notesContractAddress = "0x71EA4B7c2B5d67CcEe4C362203e9fc3e24BD56D3";
        private string _songContractAddress = "0xa80256cB3A08a42a3D4a0Ef941A83a681c065b7D";
        private ThirdwebContract _notesContract;
        private ThirdwebContract _songContract;
        private List<NFT> _songNfts;
        private bool _isReady;
        private BigInteger _balance;

        internal BigInteger Balance => _balance;

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

        private async void Start()
        {
            MusicGameManager.Instance.OnGameEnded.AddListener(ClaimTokens);

            var notesContractTask = ThirdwebContract.Create(client: ThirdwebManager.Instance.Client, chain: 37714555429, address: _notesContractAddress);
            var songContractTask = ThirdwebContract.Create(client: ThirdwebManager.Instance.Client, chain: 37714555429, address: _songContractAddress);

            _notesContract = await notesContractTask;
            _songContract = await songContractTask;
            _songNfts = await _songContract.ERC1155_GetAllNFTs();
            _isReady = true;
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
                    if (!await _wallet.IsConnected())
                    {
                        _ = await (_wallet as InAppWallet).LoginWithOauth(
                            isMobile: Application.isMobilePlatform,
                            browserOpenAction: (url) => Application.OpenURL(url),
                            mobileRedirectScheme: "com.thirdweb.unitysdk://",
                            browser: new CrossPlatformUnityBrowser()
                        );
                    }
                }
                else if (method == "Guest")
                {
                    // Guest
                    _wallet = await PrivateKeyWallet.Generate(client: ThirdwebManager.Instance.Client);
                }
                else
                {
                    throw new Exception("Invalid login method");
                }

                _wallet = await SmartWallet.Create(personalWallet: _wallet, chainId: 37714555429, gasless: true);
                var finalAddress = await _wallet.GetAddress();
                ThirdwebDebug.Log($"Logged in as: {finalAddress}");

                // Approve max tokens to the song contract if allowance is 0
                var allowance = await _notesContract.ERC20_Allowance(finalAddress, _songContractAddress);
                if (allowance == 0)
                {
                    ApproveAsync(BigInteger.Pow(2, 96) - 1);
                }

                InvokeRepeating(nameof(UpdateBalance), 0f, 5f);

                // wait until _isReady
                while (!_isReady)
                {
                    await Task.Delay(100);
                }

                OnLoggedIn.Invoke();
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error logging in: {e.Message}");
            }
        }

        private async void ApproveAsync(BigInteger amount)
        {
            try
            {
                if (_wallet == null)
                {
                    ThirdwebDebug.LogWarning("Wallet not found, skipping approval.");
                    return;
                }

                var result = await _notesContract.ERC20_Approve(_wallet, _songContractAddress, amount);
                ThirdwebDebug.Log($"Approved {amount} tokens to song contract. Transaction hash: {result.TransactionHash}");
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error approving tokens: {e.Message}");
            }
        }

        public void ResetMenu()
        {
            Source.clip = MenuMusic;
            Source.loop = true;
            Source.Play();
        }

        private async void PopulateSongList()
        {
            try
            {
                foreach (Transform child in SongContent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var song in MusicTracks)
                {
                    var price = BigInteger.Zero;
                    bool isOwned = true;
                    // Find related nft if any
                    NFT nft = _songNfts.FirstOrDefault(nft => nft.Metadata.Name.ToLower() == song.name.ToLower());
                    if (nft.Metadata.Id != null && nft.Metadata.AnimationUrl != null)
                    {
                        // Check balance
                        var balance = await _songContract.ERC1155_BalanceOf(await _wallet.GetAddress(), BigInteger.Parse(nft.Metadata.Id));
                        isOwned = balance > 0;
                        if (!isOwned)
                        {
                            var claimCondition = await _songContract.DropERC1155_GetActiveClaimCondition(BigInteger.Parse(nft.Metadata.Id));
                            price = claimCondition.PricePerToken;
                        }
                        else
                        {
                            price = BigInteger.Zero;
                        }
                    }

                    var songInstance = Instantiate(SongPrefab, SongContent);
                    songInstance.SetupSong(clip: song, price: price, unlockAction: price == 0 ? null : () => UnlockSongSync(price, BigInteger.Parse(nft.Metadata.Id)));
                }
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogException(e);
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

                    // Claim
                    var result = await _notesContract.DropERC20_Claim(_wallet, await _wallet.GetAddress(), finalClaimAmount.ToString());
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

                // Get balance
                _balance = await _notesContract.ERC20_BalanceOf(await _wallet.GetAddress());

                // Update UI
                foreach (var text in _balanceTexts)
                {
                    text.text = $"$NOTES Balance: {_balance.ToString().ToEth(0, true)}";
                }
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogError($"Error updating balance: {e.Message}");
            }
        }

        public void UnlockSongSync(BigInteger price, BigInteger tokenId)
        {
            StartCoroutine(UnlockSong(price, tokenId));
        }

        private IEnumerator UnlockSong(BigInteger price, BigInteger tokenId)
        {
            if (_wallet == null)
            {
                ThirdwebDebug.LogWarning("Wallet not found, skipping song unlock.");
                yield break;
            }

            // Check if they have enough notes
            var getAddressTask = _wallet.GetAddress();
            yield return new WaitUntil(() => getAddressTask.IsCompleted);

            if (getAddressTask.IsFaulted)
            {
                ThirdwebDebug.LogError("Error getting wallet address.");
                yield break;
            }

            var address = getAddressTask.Result;

            var getBalanceTask = _notesContract.ERC20_BalanceOf(address);
            yield return new WaitUntil(() => getBalanceTask.IsCompleted);

            if (getBalanceTask.IsFaulted)
            {
                ThirdwebDebug.LogError("Error getting balance.");
                yield break;
            }

            var balance = getBalanceTask.Result;

            if (balance < price)
            {
                ThirdwebDebug.LogWarning($"Not enough tokens to unlock song. Balance: {balance}, Price: {price}");
                yield break;
            }

            var claimTask = _songContract.DropERC1155_Claim(_wallet, address, tokenId, 1);
            yield return new WaitUntil(() => claimTask.IsCompleted);

            if (claimTask.IsFaulted)
            {
                ThirdwebDebug.LogError("Error claiming song.");
                yield break;
            }

            var result = claimTask.Result;

            ThirdwebDebug.Log($"Claim Successful: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        }
    }
}
