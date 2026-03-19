using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;


namespace Fire_Pixel.Networking
{
    public class PingDisplayer : SmartNetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI pingtext;

        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool halveRTT = true;

        private float updateGlobalTime = 0f;

        private UnityTransport transport;
        private ulong serverClientId;


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            transport = NetworkManager.NetworkConfig.NetworkTransport as UnityTransport;
            serverClientId = NetworkManager.NetworkConfig.NetworkTransport.ServerClientId;

            if (IsHost)
            {
                pingtext.text = "[Host]";
                updateGlobalTime = Mathf.Infinity;
                Destroy(this);
            }
        }

        protected override void OnNetworkTick()
        {
            if (updateGlobalTime > Time.time) return;

            updateGlobalTime += updateInterval;

            ulong pingMs = transport.GetCurrentRtt(serverClientId);

            if (halveRTT)
            {
                pingMs = (ulong)Mathf.CeilToInt(pingMs * 0.5f);
            }

            pingtext.text = pingMs + "ms";
        }
    }
}