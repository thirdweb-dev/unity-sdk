package com.walletconnect.unity

import android.content.Context
import android.content.Intent
import android.net.Uri

object Linker {
    @JvmStatic
    fun canOpenURL(context: Context, url: String): Boolean {
        val intent = Intent(Intent.ACTION_VIEW, Uri.parse(url))
        return intent.resolveActivity(context.packageManager) != null
    }
}
