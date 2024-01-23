using System;
using Org.BouncyCastle.Bcpg;
using UnityEngine;

namespace WalletConnectUnity.Core.Utils
{
    public class DeviceUtils
    {
        public static DeviceType GetDeviceType()
        {
#if UNITY_IOS
            return UnityEngine.iOS.Device.generation.ToString().Contains("iPad")
                ? DeviceType.Tablet
                : DeviceType.Phone;
#elif UNITY_ANDROID

#if !UNITY_EDITOR
            try
            {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var resources = currentActivity.Call<AndroidJavaObject>("getResources");
                var configuration = resources.Call<AndroidJavaObject>("getConfiguration");

                var screenWidthDp = configuration.Get<int>("screenWidthDp");

                // Tablets typically have a screen width of 600dp or higher
                return screenWidthDp >= 600 ? DeviceType.Tablet : DeviceType.Phone;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return DeviceType.Phone;
            }
#else
            return DeviceType.Phone;
#endif

#endif

#if UNITY_WEBGL
            return DeviceType.Web;
#endif

            return DeviceType.Desktop;
        }
    }

    public enum DeviceType
    {
        Desktop,
        Phone,
        Tablet,
        Web,
    }
}