namespace ServerList.Shared
{
    public enum MessageType
    {
        RegisterServer,
        RemoveServer,
        UpdateServer,
        ServerAlive,
        RegisterAck,
        Query,
        QueryResponse,
        DataCheck,
        DataResponse
    }
}
