namespace WalletConnectSharp.Core.Models.Expirer
{
    /// <summary>
    /// A static class that holds all event ids of events the <see cref="IExpirer"/> module emits
    /// </summary>
    public static class ExpirerEvents
    {
        public static readonly string Created = "expirer_created";

        public static readonly string Deleted = "expirer_deleted";

        public static readonly string Expired = "expirer_expired";

        public static readonly string Sync = "expirer_sync";
    }
}
