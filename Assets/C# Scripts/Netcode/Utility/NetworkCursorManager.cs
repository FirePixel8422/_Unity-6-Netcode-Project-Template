using UnityEngine;
using Unity.Netcode;
using Fire_Pixel.Networking;


public class NetworkCursorManager : SmartNetworkBehaviour
{
    public static NetworkCursorManager Instance { get; private set; }


    [SerializeField] private NetworkCursorHandler cursorPrefab;
    [SerializeField] private Color[] cursorColors;

    private NetworkCursorHandler[] cursors;
    private Canvas canvas;


    protected override void OnNetworkSystemsSetup()
    {
        Instance = this;
        cursors = new NetworkCursorHandler[GlobalGameData.MAX_PLAYERS];
        for (int i = 0; i < GlobalGameData.MAX_PLAYERS; i++)
        {
            cursors[i] = Instantiate(cursorPrefab, transform);

            bool owner = ClientManager.LocalClientGameId == i;
            cursors[i].Init(cursorColors[i], owner);
        }

        canvas = GetComponent<Canvas>();
    }

    [Rpc(SendTo.NotMe)]
    public void SendMousePosition_RPC(Vector2 normalizedMousePos, RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        Vector2 canvasSize = canvas.pixelRect.size;
        Vector2 finalMousePos = new Vector2(
            normalizedMousePos.x * canvasSize.x,
            normalizedMousePos.y * canvasSize.y
        );

        cursors[senderClientId].RecieveMousePosition_Local(finalMousePos);
    }


    private void OnValidate()
    {
        if (cursorColors.Length != GlobalGameData.MAX_PLAYERS)
        {
            cursorColors = new Color[GlobalGameData.MAX_PLAYERS];
        }
    }
}
