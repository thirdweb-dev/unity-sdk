using System;
using MetaMask.Unity;
using UnityEngine;

namespace MetaMask.Scripts.Utilities
{
    public class HideUntilMetaMaskReady : MonoBehaviour
    {
        private void Start()
        {
            if (MetaMaskSDK.Instance.Wallet == null)
            {
                MetaMaskSDK.Instance.MetaMaskUnityInitialized += InstanceOnMetaMaskUnityInitialized;
            }
            else
            {
                SetupWalletEvents();
            }
        }

        private void SetupWalletEvents()
        {
            MetaMaskSDK.Instance.Wallet.WalletAuthorized += WalletOnWalletAuthorized;
            MetaMaskSDK.Instance.Wallet.WalletDisconnected += WalletOnWalletDisconnected;
        }

        private void InstanceOnMetaMaskUnityInitialized(object sender, EventArgs e)
        {
            SetupWalletEvents();
        }

        private void WalletOnWalletDisconnected(object sender, EventArgs e)
        {
            gameObject.SetActive(false);
        }

        private void WalletOnWalletAuthorized(object sender, EventArgs e)
        {
            gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            var metamask = MetaMaskSDK.Instance;

            if (metamask == null) return;

            var shouldShow = metamask.Wallet is { IsConnected: true, IsAuthorized: true };
            
            gameObject.SetActive(shouldShow);
        }
    }
}