using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine;

namespace Thirdweb.Examples
{
    public class Prefab_SmartWallet : MonoBehaviour
    {
        private readonly string randomAddress = "0x22b79AD6c6009525933ac2FF40bC9F30dF14Ecfb";

        public async void AddAdmin()
        {
            try
            {
                var result = await ThirdwebManager.Instance.SDK.Wallet.AddAdmin(randomAddress);
                Debugger.Instance.Log("[AddAdmin] Sucess", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[AddAdmin] Error", e.Message);
            }
        }

        public async void RemoveAdmin()
        {
            try
            {
                var result = await ThirdwebManager.Instance.SDK.Wallet.RemoveAdmin(randomAddress);
                Debugger.Instance.Log("[RemoveAdmin] Sucess", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[RemoveAdmin] Error", e.Message);
            }
        }

        public async void CreateSessionKey()
        {
            try
            {
                // Create session key granting restricted smart wallet access to the local wallet for 1 day
                var contractsAllowedForInteraction = new List<string>() { "0x450b943729Ddba196Ab58b589Cea545551DF71CC" }; // contracts the local wallet is allowed to interact with
                var permissionEndTimestamp = Utils.GetUnixTimeStampNow() + 86400; // 1 day from now
                var result = await ThirdwebManager.Instance.SDK.Wallet.CreateSessionKey(
                    signerAddress: randomAddress,
                    approvedTargets: contractsAllowedForInteraction,
                    nativeTokenLimitPerTransactionInWei: "0",
                    permissionStartTimestamp: "0",
                    permissionEndTimestamp: permissionEndTimestamp.ToString(),
                    reqValidityStartTimestamp: "0",
                    reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
                );
                Debugger.Instance.Log("[CreateSessionKey] Sucess", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[CreateSessionKey] Error", e.Message);
            }
        }

        [System.Serializable]
        public class SignerPermissionRequestWebGL
        {
            public string signer;
            public byte isAdmin;
            public List<string> approvedTargets;
            public BigInteger nativeTokenLimitPerTransaction;
            public BigInteger permissionStartTimestamp;
            public BigInteger permissionEndTimestamp;
            public BigInteger reqValidityStartTimestamp;
            public BigInteger reqValidityEndTimestamp;
            public string uid;
        }

        public async void PreSignSessionKeyTxAsUserOpForLaterBroadcastingThroughThirdwebEngine()
        {
            try
            {
                // Get smart account predicted address
                var accountAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();

                // Treat it as a contract, abi can be full or just contain setPermissionsForSigner for this use case
                var accountContract = ThirdwebManager.Instance.SDK.GetContract(
                    accountAddress,
                    "[{\"type\": \"function\",\"name\": \"setPermissionsForSigner\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_req\",\"components\": [{\"type\": \"address\",\"name\": \"signer\",\"internalType\": \"address\"},{\"type\": \"uint8\",\"name\": \"isAdmin\",\"internalType\": \"uint8\"},{\"type\": \"address[]\",\"name\": \"approvedTargets\",\"internalType\": \"address[]\"},{\"type\": \"uint256\",\"name\": \"nativeTokenLimitPerTransaction\",\"internalType\": \"uint256\"},{\"type\": \"uint128\",\"name\": \"permissionStartTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"uint128\",\"name\": \"permissionEndTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"uint128\",\"name\": \"reqValidityStartTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"uint128\",\"name\": \"reqValidityEndTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"bytes32\",\"name\": \"uid\",\"internalType\": \"bytes32\"}],\"internalType\": \"struct IAccountPermissions.SignerPermissionRequest\"},{\"type\": \"bytes\",\"name\": \"_signature\",\"internalType\": \"bytes\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"}]"
                );

                // Setup the request using the correct types and values
                var contractsAllowedForInteraction = new List<string>() { "0x450b943729Ddba196Ab58b589Cea545551DF71CC" };
                var request = new Contracts.Account.ContractDefinition.SignerPermissionRequest()
                {
                    Signer = "0x22b79AD6c6009525933ac2FF40bC9F30dF14Ecfb",
                    IsAdmin = 0,
                    ApprovedTargets = contractsAllowedForInteraction,
                    NativeTokenLimitPerTransaction = 0,
                    PermissionStartTimestamp = 0,
                    PermissionEndTimestamp = Utils.GetUnixTimeStampNow() + 86400,
                    ReqValidityStartTimestamp = 0,
                    ReqValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    Uid = Guid.NewGuid().ToByteArray()
                };

                Debug.Log(JsonConvert.SerializeObject(request));

                // Sign the typed data related to session keys
                var signature = await EIP712.GenerateSignature_SmartAccount(
                    ThirdwebManager.Instance.SDK,
                    "Account",
                    "1",
                    await ThirdwebManager.Instance.SDK.Wallet.GetChainId(),
                    await ThirdwebManager.Instance.SDK.Wallet.GetAddress(),
                    request
                );

                var requestWebGL = new SignerPermissionRequestWebGL()
                {
                    signer = request.Signer,
                    isAdmin = request.IsAdmin,
                    approvedTargets = request.ApprovedTargets,
                    nativeTokenLimitPerTransaction = request.NativeTokenLimitPerTransaction,
                    permissionStartTimestamp = request.PermissionStartTimestamp,
                    permissionEndTimestamp = request.PermissionEndTimestamp,
                    reqValidityStartTimestamp = request.ReqValidityStartTimestamp,
                    reqValidityEndTimestamp = request.ReqValidityEndTimestamp,
                    uid = Utils.ToBytes32HexString(request.Uid)
                };

                // Prepare the transaction
                var tx = await accountContract.Prepare("setPermissionsForSigner", Utils.IsWebGLBuild() ? requestWebGL : request, signature.HexStringToByteArray());

                // Set gas limit to avoid any potential estimation/simulation namely in WebGL
                tx.SetGasLimit("1500000");

                // Sign the transaction, since a smart wallet is connected this returns a stringified and hexified user op ready for bundling
                var signedTx = await tx.Sign();

                // You can use engine's send-signed-user-op endpoint to broadcast or directly call eth_sendUserOperation on a bundler
                Debugger.Instance.Log("[Pre-Sign Session Key User Op] Sucess", signedTx.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Pre-Sign Session Key User Op] Error", e.Message);
            }
        }
    }
}
