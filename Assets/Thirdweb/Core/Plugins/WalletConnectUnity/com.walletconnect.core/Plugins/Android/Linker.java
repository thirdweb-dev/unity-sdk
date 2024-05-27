package com.walletconnect.unity;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import com.unity3d.player.UnityPlayerActivity;

public class Linker {
    public static boolean canOpenURL(UnityPlayerActivity activity, String url){
        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
        return intent.resolveActivity(activity.getPackageManager()) != null;
    }
}