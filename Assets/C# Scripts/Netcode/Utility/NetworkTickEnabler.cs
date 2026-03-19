using Fire_Pixel.Utility;
using Unity.Netcode;


public class NetworkTickEnabler : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        CallbackScheduler.EnableNetworkTickEvents();
        Destroy(this);
    }
}