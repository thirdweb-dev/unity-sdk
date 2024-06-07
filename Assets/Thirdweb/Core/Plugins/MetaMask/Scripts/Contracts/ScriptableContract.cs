using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using evm.net;
using evm.net.Factory;
using evm.net.Models;
using MetaMask.Models;

namespace MetaMask.Unity.Contracts
{
    public abstract class ScriptableContract<T> : ScriptableObject where T : class, IContract
    {
        #if ENABLE_MONO
        static ScriptableContract()
        {
            Contract.ContractFactory = new ImpromptuContractFactory();
        }
        #endif
        
        [Serializable]
        public class AddressByChain
        {
            public ChainId ChainId;
            public string Address;
        }

        public List<AddressByChain> ContractAddresses = new List<AddressByChain>();

        private MetaMaskWallet connectedProvider;
        private Dictionary<long, T> contractInstances = new Dictionary<long, T>();

        public T CurrentContract
        {
            get
            {
                if (connectedProvider == null)
                {
                    // We need to setup
                    var success = Setup();
                    if (!success || connectedProvider == null)
                        throw new InvalidOperationException("MetaMask is not currently connected");
                }

                var chainId = Convert.ToInt64(connectedProvider.SelectedChainId, 16);
                if (!contractInstances.ContainsKey(chainId))
                    throw new InvalidOperationException(
                        $"There is no contract instance setup for chainId {chainId}. " +
                        $"Chains available: {string.Join(',', contractInstances.Keys.Select(cid => $"0x{cid:X}"))}");

                return contractInstances[chainId];
            }
        }

        public bool HasAddressForSelectedChain
        {
            get
            {
                if (connectedProvider == null)
                {
                    // We need to setup
                    var success = Setup();
                    if (!success)
                        throw new InvalidOperationException("MetaMask is not currently connected");
                }

                var chainId = Convert.ToInt64(connectedProvider.SelectedChainId, 16);

                return contractInstances.ContainsKey(chainId);
            }
        }

        private bool Setup()
        {
            var metaMask = FindObjectOfType<MetaMaskUnity>();

            if (metaMask == null || metaMask.Wallet == null)
                return false;

            if (metaMask.Wallet.IsConnected)
            {
                SetupContract(metaMask.Wallet);
                return true;
            }
            else
            {
                metaMask.Wallet.Events.WalletAuthorized += (_, __) => SetupContract(metaMask.Wallet);
                return false;
            }
        }

        private void OnEnable()
        {
            Setup();
        }

        private void OnDisable()
        {
            contractInstances.Clear();
            connectedProvider = null;
        }

        private void OnValidate()
        {
            List<int> indexesToRemove = new List<int>();
            HashSet<ChainId> bucket = new HashSet<ChainId>();
            for (int i = 0; i < ContractAddresses.Count; i++)
            {
                if (bucket.Contains(ContractAddresses[i].ChainId))
                    indexesToRemove.Add(i);
                else
                    bucket.Add(ContractAddresses[i].ChainId);
            }

            indexesToRemove.RemoveAll(i => i == ContractAddresses.Count - 1);
            
            indexesToRemove.Reverse();
            foreach (var index in indexesToRemove)
            {
                ContractAddresses.RemoveAt(index);
            }
            
            indexesToRemove.Clear();
            bucket.Clear();
        }

        private void SetupContract(MetaMaskWallet provider)
        {
            try
            {
                connectedProvider = provider;
                foreach (var addressDetails in ContractAddresses)
                {
                    var instance = Contract.Attach<T>(connectedProvider, addressDetails.Address);
                    contractInstances.Add((long)addressDetails.ChainId, instance);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create contract instances");
                Debug.LogException(e);
            }
        }
    }
}