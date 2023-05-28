using System.Threading.Tasks;
using link.magic.unity.sdk;
using link.magic.unity.sdk.Relayer;
using Nethereum.ABI.EIP712;
using UnityEngine;

public class MagicUnity : MonoBehaviour
{
    public static MagicUnity Instance;

    private Magic _magic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize(string apikey, CustomNodeConfiguration config, string locale = "en-US")
    {
        _magic = new Magic(apikey, config, locale);
        Magic.Instance = _magic;
    }

    public async Task<string> EnableMagicAuth(string email)
    {
        return await _magic.Auth.LoginWithEmailOtp(email);
    }

    public async Task<string> GetAddress()
    {
        var metadata = await _magic.User.GetMetadata();
        return metadata.publicAddress;
    }

    public async Task<string> GetEmail()
    {
        var metadata = await _magic.User.GetMetadata();
        return metadata.email;
    }

    public async void DisableMagicAuth()
    {
        await _magic.User.Logout();
    }
}
