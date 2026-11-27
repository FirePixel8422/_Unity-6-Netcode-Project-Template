using UnityEngine;
using Unity.Netcode;
using Fire_Pixel.Networking;
using System;

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
            cursors[i].transform.position = new Vector3(0, -10000, 0);

            bool owner = ClientManager.LocalClientGameId == i;
            cursors[i].Init(cursorColors[i], owner);
        }

        canvas = GetComponent<Canvas>();
    }

    [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Unreliable)]
    public void SendMousePosition_RPC(Vector2 normalizedMousePos, RpcParams rpcParams = default)
    {
        ulong mouseId = rpcParams.Receive.SenderClientId;

        Vector2 canvasSize = canvas.pixelRect.size;
        Vector2 finalMousePos = new Vector2(
            normalizedMousePos.x * canvasSize.x,
            normalizedMousePos.y * canvasSize.y
        );

        cursors[mouseId].RecieveMousePosition_Local(finalMousePos);
    }


    private void OnValidate()
    {
        if (cursorColors == null || cursorColors.Length == GlobalGameData.MAX_PLAYERS) return;

        Array.Resize(ref cursorColors, GlobalGameData.MAX_PLAYERS);
    }
}
