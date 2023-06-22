using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
// using WalletConnectSharp.Core.Models.Pairing;
// using WalletConnectSharp.Sign;
// using WalletConnectSharp.Sign.Models;
// using WalletConnectSharp.Sign.Models.Engine;
// using WalletConnectSharp.Storage;
using ZXing;
using ZXing.QrCode;

public class WalletConnectUI : MonoBehaviour
{
    public GameObject WalletConnectCanvas;
    public Image QRCodeImage;
    public Button DeepLinkButton;

    public static WalletConnectUI Instance;

    // private SignClientOptions _dappOptions;
    // private ConnectOptions _dappConnectOptions;
    // private WalletConnectSignClient _dappClient;
    // private ConnectedData _connectData;
    // private SessionStruct _sessionData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    public async Task<string> Connect(string walletConnectProjectId)
    {
        // WalletConnectCanvas.SetActive(true);

        // _dappOptions = new SignClientOptions()
        // {
        //     ProjectId = walletConnectProjectId,
        //     Metadata = new Metadata()
        //     {
        //         Description = ThirdwebManager.Instance.SDK.session.Options.wallet?.appDescription,
        //         Icons = ThirdwebManager.Instance.SDK.session.Options.wallet?.appIcons,
        //         Name = ThirdwebManager.Instance.SDK.session.Options.wallet?.appName,
        //         Url = ThirdwebManager.Instance.SDK.session.Options.wallet?.appUrl
        //     },
        //     Storage = new InMemoryStorage()
        // };

        // _dappConnectOptions = new ConnectOptions()
        // {
        //     RequiredNamespaces = new RequiredNamespaces()
        //     {
        //         {
        //             "eip155",
        //             new RequiredNamespace()
        //             {
        //                 Methods = new[] { "eth_sendTransaction", "eth_sendRawTransaction", "eth_signTransaction", "personal_sign", "eth_signTypedData_v4", "eth_accounts", "eth_chainId" },
        //                 Chains = new[] { "eip155:1" },
        //                 Events = new[] { "chainChanged", "accountsChanged", }
        //             }
        //         }
        //     }
        // };

        // _dappClient = await WalletConnectSignClient.Init(_dappOptions);
        // _connectData = await _dappClient.Connect(_dappConnectOptions);

        // Debug.Log($"URI: {_connectData.Uri}");

        // var qrCodeAsTexture2D = GenerateQRTexture(_connectData.Uri);
        // QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
        // DeepLinkButton.onClick.RemoveAllListeners();
        // DeepLinkButton.onClick.AddListener(() => Application.OpenURL(_connectData.Uri));

        // await _connectData.Approval;

        // WalletConnectCanvas.SetActive(false);

        // var accounts = await _dappClient.Request<object, string[]>("eth_accounts", null);
        // return accounts[0];
        return "";
    }

    private static Texture2D GenerateQRTexture(string text)
    {
        Texture2D encoded = new Texture2D(256, 256);
        var color32 = EncodeToQR(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }

    private static Color32[] EncodeToQR(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions { Height = height, Width = width }
        };
        return writer.Write(textForEncoding);
    }
}
