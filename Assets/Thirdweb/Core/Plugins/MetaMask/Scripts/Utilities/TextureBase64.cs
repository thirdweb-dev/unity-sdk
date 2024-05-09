using System;
using UnityEngine;

namespace MetaMask.Scripts.Utilities
{
    public static class TextureBase64
    {
        public static string TextureToBase64(Texture2D texture2D)
        {
            // resize the texture
            if (texture2D.width > 64 || texture2D.height > 64)
            {
                var source = texture2D;
                var newWidth = 64;
                var newHeight = 64;
                
                RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
                rt.filterMode = source.filterMode;
                RenderTexture.active = rt;
                Graphics.Blit(source, rt);
                Texture2D nTex = new Texture2D(newWidth, newHeight);
                nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0,0);
                nTex.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);

                texture2D = nTex;
            }
            
            // Always copy the pixels to a in-memory texture
            // Since this texture may not support reading or encoding
            Texture2D newTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBAFloat, false);
            newTexture.SetPixels(0,0, texture2D.width, texture2D.height, texture2D.GetPixels(0, 0, texture2D.width, texture2D.height));
            newTexture.Apply();

            byte[] imageData = newTexture.EncodeToPNG();
            return Convert.ToBase64String(imageData);
        }
        
        public static Texture2D Base64ToTexture(string encodedData)
        {
            try
            {
                byte[] imageData = Convert.FromBase64String(encodedData);

                int width, height;
                GetImageSize(imageData, out width, out height);

                Texture2D texture = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
                texture.hideFlags = HideFlags.HideAndDontSave;
                //texture.filterMode = FilterMode.Point;
                texture.LoadImage(imageData, true);

                return texture;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
        
        private static void GetImageSize(byte[] imageData, out int width, out int height)
        {
            width = ReadInt(imageData, 3 + 15);
            height = ReadInt(imageData, 3 + 19);
        }
        
        private static int ReadInt(byte[] imageData, int offset)
        {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }
    }
}