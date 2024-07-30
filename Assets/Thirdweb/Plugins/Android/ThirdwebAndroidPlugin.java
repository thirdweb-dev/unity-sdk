package com.thirdweb.unity;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import androidx.browser.customtabs.CustomTabsIntent;
import com.unity3d.player.UnityPlayerActivity;

public class ThirdwebAndroidPlugin
{
    public static void OpenCustomTab(UnityPlayerActivity activity, String url) 
    {
        CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
        builder.setUrlBarHidingEnabled(true);
        CustomTabsIntent customTabsIntent = builder.build();
        customTabsIntent.launchUrl(activity, Uri.parse(url));
    }
}
