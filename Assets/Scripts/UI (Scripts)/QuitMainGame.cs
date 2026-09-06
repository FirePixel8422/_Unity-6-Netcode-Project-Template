using Fire_Pixel.Networking;
using UnityEngine;


public class QuitMainGame : MonoBehaviour
{
    public void QuitToMainMenu()
    {
        ClientManager.Instance.ShutDownNetwork_ServerRPC();
    }
}