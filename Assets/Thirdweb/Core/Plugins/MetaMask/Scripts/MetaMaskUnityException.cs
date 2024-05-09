using System;

namespace MetaMask.Unity
{
    public class MetaMaskUnityException : Exception
    {
        public MetaMaskUnityException(string reason, Exception cause) : base(reason, cause)
        {
        }
    }
}