using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb.Pay;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;

namespace Thirdweb.Unity.Examples
{
    [Serializable]
    public enum DropType
    {
        DropERC20,
        DropERC721,
        DropERC1155
    }

    [RequireComponent(typeof(Button))]
    public class Prefab_Pay : MonoBehaviour
    {
        [field: SerializeField, Header("Drop Settings")]
        private string DropAddress;

        [field: SerializeField]
        private DropType DropType;

        [field: SerializeField]
        private string ChainId;

        [field: SerializeField]
        private string Quantity;

        [field: SerializeField, Header("DropERC1155 Settings")]
        private string TokenIdIfErc1155;

        [field: SerializeField, Header("OnRamp Settings")]
        private string FiatCurrency = "USD";

        [field: SerializeField]
        private bool TestMode = true;

        [field: SerializeField, Header("UI Settings")]
        private TMP_Text BalanceText;

        private ThirdwebContract _dropContract;
        private BigInteger _chainId;
        private BigInteger? _tokenId;
        private ThirdwebClient _client;
        private IThirdwebWallet _wallet;
        private bool _initialized;
        private bool _isPurchasing;

        private void Start()
        {
            // Or pass/create your own wallet
            FindObjectOfType<Prefab_ConnectWallet>().OnConnected.AddListener(Initialize);
        }

        private async void Initialize(IThirdwebWallet wallet)
        {
            try
            {
                // Use the connected wallet
                _wallet = wallet;

                // Check if wallet is active
                if (_wallet == null)
                {
                    ThirdwebDebug.LogWarning("Prefab_Pay: No active wallet found.");
                    _initialized = false;
                    return;
                }

                // If purchase initialized, return
                if (_initialized)
                {
                    return;
                }

                // Initialize variables
                _client = ThirdwebManager.Instance.Client;
                _chainId = !string.IsNullOrEmpty(ChainId) ? BigInteger.Parse(ChainId) : throw new Exception("Prefab_Pay: ChainId is required.");
                _tokenId = string.IsNullOrEmpty(TokenIdIfErc1155) ? null : BigInteger.Parse(TokenIdIfErc1155);
                if (DropType == DropType.DropERC1155 && _tokenId == null)
                {
                    ThirdwebDebug.LogError("Prefab_Pay: Token ID is required for DropERC1155.");
                    return;
                }
                _dropContract = await ThirdwebContract.Create(_client, DropAddress, _chainId);
                _initialized = true;

                // Add click listener
                GetComponent<Button>().onClick.AddListener(OnClick);

                // Update balance
                InvokeRepeating(nameof(UpdateBalance), 0, 5);

                ThirdwebDebug.Log("Prefab_Pay: Initialization Successful.");
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogError($"Prefab_Pay: Initialization Error: {e.Message}");
                _initialized = false;
            }
        }

        private async void UpdateBalance()
        {
            if (!_initialized)
                return;

            try
            {
                var address = await _wallet.GetAddress();
                var balance =
                    DropType == DropType.DropERC20
                        ? await _dropContract.ERC20_BalanceOf(address)
                        : DropType == DropType.DropERC721
                            ? await _dropContract.ERC721_BalanceOf(address)
                            : await _dropContract.ERC1155_BalanceOf(address, _tokenId.Value);
                BalanceText.text = $"Current Balance: {balance.ToString().ToEth(DropType == DropType.DropERC20 ? 4 : 0)}";
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogError($"Prefab_Pay: Error fetching balance: {e.Message}");
            }
        }

        private async void OnClick()
        {
            if (!_initialized)
                return;

            if (_isPurchasing)
            {
                ThirdwebDebug.LogWarning("Prefab_Pay: Purchase already in progress.");
                return;
            }

            _isPurchasing = true;

            try
            {
                // Get claim condition
                var claimCondition =
                    DropType == DropType.DropERC20
                        ? await _dropContract.DropERC20_GetActiveClaimCondition()
                        : DropType == DropType.DropERC721
                            ? await _dropContract.DropERC721_GetActiveClaimCondition()
                            : await _dropContract.DropERC1155_GetActiveClaimCondition(_tokenId.Value);

                // Fetch claim condition currency address
                var currency = claimCondition.Currency;

                ThirdwebDebug.Log($"Prefab_Pay: Claim condition fetched. Currency: {currency}");

                // Check if currency is native token
                var isNativeCurrency = currency == Thirdweb.Constants.NATIVE_TOKEN_ADDRESS;

                var dropERC20Decimals = DropType == DropType.DropERC20 ? await _dropContract.ERC20_Decimals() : 0;

                // Calculate final claim quantity
                var finalQuantity = BigInteger.One;
                switch (DropType)
                {
                    case DropType.DropERC20:
                        finalQuantity = BigInteger.Parse(Quantity.ToWei()).AdjustDecimals(fromDecimals: 18, toDecimals: dropERC20Decimals);
                        break;
                    case DropType.DropERC721:
                        finalQuantity = 1;
                        break;
                    case DropType.DropERC1155:
                        finalQuantity = BigInteger.Parse(Quantity);
                        break;
                }

                ThirdwebDebug.Log($"Prefab_Pay: Final Quantity: {finalQuantity}");

                // Calculate final price
                var price = claimCondition.PricePerToken * finalQuantity;
                price = DropType == DropType.DropERC20 ? price / BigInteger.Pow(10, dropERC20Decimals) : price;

                ThirdwebDebug.Log($"Prefab_Pay: Final Price: {price}");

                // Check if user has sufficient balance
                var currentBalance = await _wallet.GetBalance(chainId: _chainId, erc20ContractAddress: isNativeCurrency ? null : currency);

                // We ignore the case where the price is 0 but the user also doesn't have enough for gas
                // This example assumes the user has enough for gas if price is 0, use Smart Wallets to avoid thinking about gas!
                if (currentBalance < price)
                {
                    ThirdwebDebug.Log("Prefab_Pay: Insufficient funds, initiating top-up.");

                    // Quote parameters
                    var quoteParams = new BuyWithFiatQuoteParams(
                        fromCurrencySymbol: FiatCurrency,
                        toAddress: await _wallet.GetAddress(),
                        toChainId: _chainId.ToString(),
                        toTokenAddress: currency,
                        toAmountWei: price.ToString(),
                        isTestMode: TestMode
                    );

                    ThirdwebDebug.Log("Prefab_Pay: Quote Params: " + JsonConvert.SerializeObject(quoteParams, Formatting.Indented));

                    // Fetch onramp quote
                    var quote = await ThirdwebPay.GetBuyWithFiatQuote(_client, quoteParams);

                    ThirdwebDebug.Log("Prefab_Pay: Quote: " + JsonConvert.SerializeObject(quote, Formatting.Indented));

                    // Open onramp link
                    var onRampLink = ThirdwebPay.BuyWithFiat(quote);
                    Application.OpenURL(onRampLink);

                    // Check if we are onramping directly to the target token or need to swap post-onramp
                    var isSwapRequiredPostOnramp = IsSwapRequiredPostOnramp(quote);

                    // Wait for onramp to complete
                    var currentOnRampStatus = OnRampStatus.NONE;
                    while (currentOnRampStatus != OnRampStatus.ON_RAMP_TRANSFER_COMPLETED && Application.isPlaying)
                    {
                        var fullStatus = await ThirdwebPay.GetBuyWithFiatStatus(_client, quote.IntentId);
                        currentOnRampStatus = Enum.Parse<OnRampStatus>(fullStatus.Status);
                        await Task.Delay(5000);

                        if (currentOnRampStatus == OnRampStatus.ON_RAMP_TRANSFER_FAILED || currentOnRampStatus == OnRampStatus.PAYMENT_FAILED)
                        {
                            throw new Exception($"Onramp flow could not be completed. Reason: {currentOnRampStatus}");
                        }

                        ThirdwebDebug.Log($"Prefab_Pay: Onramp status: {currentOnRampStatus}...");
                    }

                    if (!Application.isPlaying)
                    {
                        return;
                    }

                    ThirdwebDebug.Log("Prefab_Pay: Onramp successful.");

                    // Swap post-onramp if required
                    if (isSwapRequiredPostOnramp)
                    {
                        ThirdwebDebug.Log("Prefab_Pay: Swap required post onramp, initiating swap.");

                        // Fetch swap quote
                        var swapQuote = await ThirdwebPay.GetBuyWithCryptoQuote(
                            _client,
                            new BuyWithCryptoQuoteParams(
                                fromAddress: quote.ToAddress,
                                fromChainId: quote.OnRampToken.Token.ChainId,
                                fromTokenAddress: quote.OnRampToken.Token.TokenAddress,
                                toTokenAddress: quote.ToToken.TokenAddress,
                                toChainId: quote.ToToken.ChainId,
                                toAmount: quote.EstimatedToAmountMin,
                                maxSlippageBPS: quote.MaxSlippageBPS,
                                intentId: quote.IntentId
                            )
                        );

                        // Initiate swap transaction(s)
                        var swapTxHash = await ThirdwebPay.BuyWithCrypto(_wallet, swapQuote);
                        ThirdwebDebug.Log($"Prefab_Pay: Swap successful. Transaction receipt: {swapTxHash}");

                        // Wait for swap to complete
                        var currentSwapStatus = SwapStatus.NONE;
                        while (currentSwapStatus != SwapStatus.COMPLETED && Application.isPlaying)
                        {
                            var fullStatus = await ThirdwebPay.GetBuyWithCryptoStatus(_client, swapTxHash);
                            currentSwapStatus = Enum.Parse<SwapStatus>(fullStatus.Status);
                            await Task.Delay(2000);

                            if (currentSwapStatus == SwapStatus.FAILED)
                            {
                                throw new Exception($"Post-OnRamp Swap flow could not be completed. Reason: {currentSwapStatus}");
                            }

                            ThirdwebDebug.Log($"Prefab_Pay: Swap status: {currentSwapStatus}...");
                        }

                        if (!Application.isPlaying)
                        {
                            return;
                        }

                        ThirdwebDebug.Log("Prefab_Pay: Swap successful.");
                    }
                }
                else
                {
                    ThirdwebDebug.Log("Prefab_Pay: Sufficient funds, initiating buy.");
                }

                ThirdwebDebug.Log("Prefab_Pay: Initiating final transaction...");

                var tx =
                    DropType == DropType.DropERC20
                        ? await _dropContract.DropERC20_Claim(_wallet, await _wallet.GetAddress(), Quantity)
                        : DropType == DropType.DropERC721
                            ? await _dropContract.DropERC721_Claim(_wallet, await _wallet.GetAddress(), BigInteger.Parse(Quantity))
                            : await _dropContract.DropERC1155_Claim(_wallet, await _wallet.GetAddress(), _tokenId.Value, BigInteger.Parse(Quantity));
                ThirdwebDebug.Log($"Prefab_Pay: Buy successful. Transaction receipt: {JsonConvert.SerializeObject(tx)}");
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogError($"Prefab_Pay: Error buying: {e}");
            }
            finally
            {
                _isPurchasing = false;
            }
        }

        // TODO: Make this ThirdwebPay static method
        private bool IsSwapRequiredPostOnramp(BuyWithFiatQuoteResult buyWithFiatQuote)
        {
            var sameChain = buyWithFiatQuote.ToToken.ChainId == buyWithFiatQuote.OnRampToken.Token.ChainId;
            var sameToken = buyWithFiatQuote.ToToken.TokenAddress == buyWithFiatQuote.OnRampToken.Token.TokenAddress;
            return !(sameChain && sameToken);
        }
    }
}
