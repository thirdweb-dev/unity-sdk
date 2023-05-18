using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using link.magic.unity.sdk.Provider;

namespace link.magic.unity.sdk.Modules.User
{
    public class UserModule : BaseModule
    {
        internal UserModule(RpcProvider provider) : base(provider)
        {
            Provider = provider;
        }

        public async Task<string> GetIdToken(int lifespan = 900)
        {
            var config = new GetIdTokenConfiguration(lifespan);
            return await SendToProviderWithConfig<GetIdTokenConfiguration, string>(config,
                nameof(UserMethod.magic_auth_get_id_token));
        }

        public async Task<string> GenerateIdToken(int lifespan = 900, string attachment = "none")
        {
            var config = new GenerateIdTokenConfiguration(lifespan, attachment);
            return await SendToProviderWithConfig<GenerateIdTokenConfiguration, string>(config,
                nameof(UserMethod.magic_auth_generate_id_token));
        }

        public async Task<UserMetadata> GetMetadata()
        {
            return await SendToProvider<UserMetadata>(nameof(UserMethod.magic_auth_get_metadata));
        }

        public async Task<bool> UpdateEmail()
        {
            return await SendToProvider<bool>(nameof(UserMethod.magic_auth_update_email));
        }

        public async Task<bool> IsLoggedIn()
        {
            return await SendToProvider<bool>(nameof(UserMethod.magic_auth_is_logged_in));
        }

        public async Task<bool> Logout()
        {
            return await SendToProvider<bool>(nameof(UserMethod.magic_auth_logout));
        }
    }

    /// <summary>
    ///     User Configuration
    /// </summary>
    [Serializable]
    internal class GetIdTokenConfiguration : BaseConfiguration
    {
        public int lifespan;

        internal GetIdTokenConfiguration(int lifespan)
        {
            this.lifespan = lifespan;
        }
    }

    [Serializable]
    internal class GenerateIdTokenConfiguration : BaseConfiguration
    {
        public string attachment;
        public int lifespan;

        internal GenerateIdTokenConfiguration(int lifespan, string attachment)
        {
            this.attachment = attachment;
            this.lifespan = lifespan;
        }
    }

    [Serializable]
    internal class UpdateEmailConfiguration : BaseConfiguration
    {
        public string email;
        public bool showUI;

        internal UpdateEmailConfiguration(string email, bool showUI)
        {
            this.email = email;
            this.showUI = showUI;
        }
    }

    internal enum UserMethod
    {
        magic_auth_get_id_token,
        magic_auth_generate_id_token,
        magic_auth_get_metadata,
        magic_auth_is_logged_in,
        magic_auth_update_email,
        magic_auth_logout
    }

    [Serializable]
    public sealed class UserMetadata
    {
        [CanBeNull] public string issuer;
        [CanBeNull] public string publicAddress;
        [CanBeNull] public string email;
    }
}