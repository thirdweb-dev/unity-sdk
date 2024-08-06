using UnityEngine;
using ZXing;
using ZXing.QrCode;

namespace WalletConnectUnity.Core.Utils
{
    public class QRCode
    {
        public static Texture2D EncodeTexture(string textForEncoding, int width = 512, int height = 512)
        {
            return EncodeTexture(textForEncoding, Color.black, Color.white, width, height);
        }

        public static Texture2D EncodeTexture(string textForEncoding, Color fgColor, Color bgColor, int width = 512, int height = 512)
        {
            var pixels = EncodePixels(textForEncoding, fgColor, bgColor, width, height);

            var texture = new Texture2D(width, height);
            texture.SetPixels32(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            texture.Compress(true);

            return texture;
        }

        public static Color32[] EncodePixels(string textForEncoding, int width = 512, int height = 512)
        {
            return EncodePixels(textForEncoding, Color.black, Color.white, width, height);
        }

        public static Color32[] EncodePixels(string textForEncoding, Color fgColor, Color bgColor, int width = 512, int height = 512)
        {
            var qrCodeEncodingOptions = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 4,
                QrVersion = 11
            };

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = qrCodeEncodingOptions,
                Renderer = new Color32Renderer
                {
                    Foreground = fgColor,
                    Background = bgColor
                }
            };

            return writer.Write(textForEncoding);
        }
    }
}