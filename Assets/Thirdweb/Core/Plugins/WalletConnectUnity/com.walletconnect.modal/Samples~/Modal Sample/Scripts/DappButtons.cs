using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Evm;

namespace WalletConnectUnity.Modal.Sample
{
    public class DappButtons : MonoBehaviour
    {
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Button _personalSignButton;
        [SerializeField] private Button _transactionButton;

        private void Awake()
        {
            WalletConnect.Instance.ActiveSessionChanged += (_, @struct) =>
            {
                _disconnectButton.interactable = true;
                _personalSignButton.interactable = true;
                _transactionButton.interactable = true;
            };
        }

        public void OnDisconnectButton()
        {
            Debug.Log("[WalletConnectModalSample] OnDisconnectButton");

            _disconnectButton.interactable = false;
            _personalSignButton.interactable = false;
            _transactionButton.interactable = false;

            WalletConnectModal.Disconnect();
        }

        public async void OnPersonalSignButton()
        {
            Debug.Log("[WalletConnectModalSample] OnPersonalSignButton");

            var session = WalletConnect.Instance.ActiveSession;
            var sessionNamespace = session.Namespaces;
            var address = WalletConnect.Instance.ActiveSession.CurrentAddress(sessionNamespace.Keys.FirstOrDefault())
                .Address;

            var data = new PersonalSign("Hello world!", address);

            try
            {
                var result = await WalletConnect.Instance.RequestAsync<PersonalSign, string>(data);
                Notification.ShowMessage(
                    $"Received response.\nThis app cannot validate signatures yet.\n\nResponse: {result}");
            }
            catch (WalletConnectException e)
            {
                Notification.ShowMessage($"Personal Sign Request Error: {e.Message}");
                Debug.Log($"[WalletConnectModalSample] Personal Sign Error: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        public async void OnTransactionButton()
        {
            Debug.Log("[WalletConnectModalSample] OnTransactionButton");

            var session = WalletConnect.Instance.ActiveSession;
            var sessionNamespace = session.Namespaces;
            var address = WalletConnect.Instance.ActiveSession.CurrentAddress(sessionNamespace.Keys.FirstOrDefault())
                .Address;

            var request = new EthSendTransaction(new Transaction
            {
                from = address,
                to = address,
                value = "0"
            });

            try
            {
                var result = await WalletConnect.Instance.RequestAsync<EthSendTransaction, string>(request);
                Notification.ShowMessage($"Done!\nResponse: {result}");
            }
            catch (WalletConnectException e)
            {
                Notification.ShowMessage($"Transaction Request Error: {e.Message}");
                Debug.Log($"[WalletConnectModalSample] Transaction Error: {e.Message}");
            }
        }
        
    }
}