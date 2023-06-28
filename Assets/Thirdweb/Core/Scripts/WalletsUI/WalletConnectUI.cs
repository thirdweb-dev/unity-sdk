// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;
// using WalletConnectSharp.Core.Models.Pairing;
// using WalletConnectSharp.Sign;
// using WalletConnectSharp.Sign.Models;
// using WalletConnectSharp.Sign.Models.Engine;
// using WalletConnectSharp.Storage;
// using WalletConnectSharp.Core;
// using ZXing;
// using ZXing.QrCode;
// using WalletConnect;

// namespace Thirdweb.Wallets
// {
//     public class WalletConnectUI : MonoBehaviour
//     {
//         public GameObject WalletConnectCanvas;
//         public Image QRCodeImage;
//         public Button DeepLinkButton;

//         public static WalletConnectUI Instance;

//         private void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//                 DontDestroyOnLoad(this.gameObject);
//             }
//             else
//             {
//                 Destroy(this.gameObject);
//                 return;
//             }
//         }

//         public async Task<string> Connect(string walletConnectProjectId, System.Numerics.BigInteger chainId)
//         {
//             WalletConnectCanvas.SetActive(true);

//             WalletConnectUnity.Instance.ProjectName = ThirdwebManager.Instance.SDK.session.Options.wallet?.appName;
//             WalletConnectUnity.Instance.ProjectId = walletConnectProjectId;
//             WalletConnectUnity.Instance.ClientMetadata = new Metadata()
//             {
//                 Description = ThirdwebManager.Instance.SDK.session.Options.wallet?.appDescription,
//                 Icons = ThirdwebManager.Instance.SDK.session.Options.wallet?.appIcons,
//                 Name = ThirdwebManager.Instance.SDK.session.Options.wallet?.appName,
//                 Url = ThirdwebManager.Instance.SDK.session.Options.wallet?.appUrl
//             };
//             WalletConnectUnity.Instance.BaseContext = "unity-game";

//             await WCSignClient.Instance.InitSignClient();

//             var dappConnectOptions = new ConnectOptions()
//             {
//                 RequiredNamespaces = new RequiredNamespaces()
//                 {
//                     {
//                         "eip155",
//                         new RequiredNamespace()
//                         {
//                             Methods = new[]
//                             {
//                                 "eth_sendTransaction",
//                                 // "eth_sendRawTransaction",
//                                 "personal_sign",
//                                 "eth_signTypedData_v4",
//                                 // "eth_accounts",
//                                 // "eth_chainId",
//                                 // "eth_getBalance"
//                             },
//                             Chains = new[] { $"eip155:{chainId}" },
//                             Events = new[] { "chainChanged", "accountsChanged", }
//                         }
//                     }
//                 }
//             };

//             var connectedData = await WCSignClient.Instance.SignClient.Connect(dappConnectOptions);

//             Debug.Log($"URI: {connectedData.Uri}");

//             var qrCodeAsTexture2D = GenerateQRTexture(connectedData.Uri);
//             QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
//             DeepLinkButton.onClick.RemoveAllListeners();
//             DeepLinkButton.onClick.AddListener(() => Application.OpenURL(connectedData.Uri));

//             await connectedData.Approval;

//             WalletConnectCanvas.SetActive(false);

//             return null;
//         }

//         private static Texture2D GenerateQRTexture(string text)
//         {
//             Texture2D encoded = new Texture2D(256, 256);
//             var color32 = EncodeToQR(text, encoded.width, encoded.height);
//             encoded.SetPixels32(color32);
//             encoded.Apply();
//             return encoded;
//         }

//         private static Color32[] EncodeToQR(string textForEncoding, int width, int height)
//         {
//             var writer = new BarcodeWriter
//             {
//                 Format = BarcodeFormat.QR_CODE,
//                 Options = new QrCodeEncodingOptions { Height = height, Width = width }
//             };
//             return writer.Write(textForEncoding);
//         }
//     }
// }
