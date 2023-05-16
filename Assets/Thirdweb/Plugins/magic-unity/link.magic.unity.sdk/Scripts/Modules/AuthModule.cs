using System;
using System.Threading.Tasks;
using link.magic.unity.sdk.Provider;

namespace link.magic.unity.sdk.Modules.Auth
{
    public class AuthModule : BaseModule
    {
        internal AuthModule(RpcProvider provider) : base(provider)
        {
            Provider = provider;
        }

        public async Task<string> LoginWithMagicLink(string email, bool showUI = true)
        {
            var config = new LoginWithMagicLinkConfiguration(email, showUI);
            return await SendToProviderWithConfig<LoginWithMagicLinkConfiguration, string>(config,
                nameof(AuthMethod.magic_auth_login_with_magic_link)
            );
        }

        public async Task<string> LoginWithSms(string phoneNumber)
        {
            var config = new LoginWithSmsConfiguration(phoneNumber);
            return await SendToProviderWithConfig<LoginWithSmsConfiguration, string>(config,
                nameof(AuthMethod.magic_auth_login_with_sms)
            );
        }

        public async Task<string> LoginWithEmailOtp(string email)
        {
            var config = new LoginWithEmailOtpConfiguration(email);
            return await SendToProviderWithConfig<LoginWithEmailOtpConfiguration, string>(config,
                nameof(AuthMethod.magic_auth_login_with_email_otp));
        }


        [Serializable]
        internal class LoginWithMagicLinkConfiguration : BaseConfiguration
        {
            public bool showUI;
            public string email;

            public LoginWithMagicLinkConfiguration(string email, bool showUI = true)
            {
                this.showUI = showUI;
                this.email = email;
            }
        }

        [Serializable]
        internal class LoginWithSmsConfiguration : BaseConfiguration
        {
            public bool showUI;
            public string phoneNumber;

            public LoginWithSmsConfiguration(string phoneNumber)
            {
                this.phoneNumber = phoneNumber;
            }
        }

        [Serializable]
        internal class LoginWithEmailOtpConfiguration : BaseConfiguration
        {
            public string email;

            public LoginWithEmailOtpConfiguration(string email)
            {
                this.email = email;
            }
        }
    }

    internal enum AuthMethod
    {
        magic_auth_login_with_magic_link,
        magic_auth_login_with_sms,
        magic_auth_login_with_email_otp
    }
}