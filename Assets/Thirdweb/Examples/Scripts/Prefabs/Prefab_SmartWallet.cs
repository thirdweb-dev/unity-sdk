using System;
using System.Collections.Generic;
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
                var result = await ThirdwebManager.Instance.SDK.wallet.AddAdmin(randomAddress);
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
                var result = await ThirdwebManager.Instance.SDK.wallet.RemoveAdmin(randomAddress);
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
                var result = await ThirdwebManager.Instance.SDK.wallet.CreateSessionKey(
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

        public async void PreSignSessionKeyTxAsUserOpForLaterBroadcastingThroughThirdwebEngine()
        {
            try
            {
                var accountAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
                var accountContract = ThirdwebManager.Instance.SDK.GetContract(
                    accountAddress,
                    "[{\"type\": \"function\",\"name\": \"setPermissionsForSigner\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_req\",\"components\": [{\"type\": \"address\",\"name\": \"signer\",\"internalType\": \"address\"},{\"type\": \"uint8\",\"name\": \"isAdmin\",\"internalType\": \"uint8\"},{\"type\": \"address[]\",\"name\": \"approvedTargets\",\"internalType\": \"address[]\"},{\"type\": \"uint256\",\"name\": \"nativeTokenLimitPerTransaction\",\"internalType\": \"uint256\"},{\"type\": \"uint128\",\"name\": \"permissionStartTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"uint128\",\"name\": \"permissionEndTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"uint128\",\"name\": \"reqValidityStartTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"uint128\",\"name\": \"reqValidityEndTimestamp\",\"internalType\": \"uint128\"},{\"type\": \"bytes32\",\"name\": \"uid\",\"internalType\": \"bytes32\"}],\"internalType\": \"struct IAccountPermissions.SignerPermissionRequest\"},{\"type\": \"bytes\",\"name\": \"_signature\",\"internalType\": \"bytes\"}],\"outputs\": [],\"stateMutability\": \"nonpayable\"}]"
                );
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
                var signature = await EIP712.GenerateSignature_SmartAccount(
                    "Account",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                    request
                );
                var tx = await accountContract.Prepare("setPermissionsForSigner", request, signature.HexStringToByteArray());
                var signedTx = await tx.Sign();
                Debugger.Instance.Log("[Pre-Sign Session Key User Op] Sucess", signedTx.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Pre-Sign Session Key User Op] Error", e.Message);
            }
        }
    }
}
