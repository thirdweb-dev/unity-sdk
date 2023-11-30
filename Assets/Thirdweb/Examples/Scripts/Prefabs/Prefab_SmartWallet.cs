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
    }
}
