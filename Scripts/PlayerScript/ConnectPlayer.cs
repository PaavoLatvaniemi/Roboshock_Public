using MLAPI;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;

public class ConnectPlayer : NetworkBehaviour
{
    public static NetworkObject localPlayerNetworkObject { get; private set; }
    public ClientSideInterpolationWatcher ClientSideInterpolationWatcher { get => _clientSideInterpolationWatcher; }
    ClientSideInterpolationWatcher _clientSideInterpolationWatcher;

    private void Start()
    {
        if (!IsOwner) return;
        _clientSideInterpolationWatcher = GetComponent<ClientSideInterpolationWatcher>();
        localPlayerNetworkObject = GetComponent<NetworkObject>();
        PlayerInfo playerInfo = GetComponent<PlayerInfo>();
        playerInfo.SetPlayerNameServerRpc(StartMenu.nameText);
        playerInfo.SetPlayerIsHostServerRpc(IsHost);


    }
    public int GetNetworkTimeWithInputDelay()
    {
        return ServerFrameSimulator.Singleton.FrameTime.Value -
            (int)(NetworkManager.Singleton.GetComponent<UNetTransport>().GetCurrentRtt(localPlayerNetworkObject.NetworkObjectId) / 2f);
    }

}
