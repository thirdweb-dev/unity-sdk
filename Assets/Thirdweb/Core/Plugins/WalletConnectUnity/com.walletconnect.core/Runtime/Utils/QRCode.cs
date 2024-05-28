using UnityEngine;
using ZXing;
using ZXing.QrCode;

namespace WalletConnectUnity.Core.Utils
{
    public class QRCode
    {
        public static Texture2D EncodeTexture(string textForEncoding, int width = 1024, int height = 1024)
        {
            var pixels = EncodePixels(textForEncoding, width, height);

            var texture = new Texture2D(width, height);
            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        public static Color32[] EncodePixels(string textForEncoding, int width = 1024, int height = 1024)
        {
            var qrCodeEncodingOptions = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 4,
                QrVersion = 11
            };

            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE, Options = qrCodeEncodingOptions };

            return writer.Write(textForEncoding);
        }
    }
}
