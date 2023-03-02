using UnityEngine;

public class NativeAccountUtil : MonoBehaviour
{
    async void Start()
    {
        GetComponent<TMPro.TMP_Text>().text = $"Native Account: {await ThirdwebManager.Instance.SDK.wallet.GetAddress()}";
    }
}
