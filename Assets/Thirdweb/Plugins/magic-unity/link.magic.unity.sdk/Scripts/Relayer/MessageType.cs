namespace link.magic.unity.sdk.Relayer
{
    public enum OutboundMessageType
    {
        MAGIC_HANDLE_REQUEST
    }

    public enum InboundMessageType
    {
        MAGIC_HANDLE_RESPONSE,
        MAGIC_OVERLAY_READY,
        MAGIC_SHOW_OVERLAY,
        MAGIC_HIDE_OVERLAY,
        MAGIC_HANDLE_EVENT
    }
}