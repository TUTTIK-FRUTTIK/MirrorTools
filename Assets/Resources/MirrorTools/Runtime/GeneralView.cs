using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class GeneralView : MonoBehaviour
    {
        public TextMeshProUGUI generalText;

        public void UpdateGeneralInfo(GeneralInfoResponse response)
        {
            string traffic = response.incomingTraffic == -1
                ? "<size=24>(Traffic data is available only when the NetworkManager has NetworkStatistics component)</scale>"
                : $"outgoing traffic: {Mirror.Utils.PrettyBytes(response.outgoingTraffic)} \nincoming traffic: {Mirror.Utils.PrettyBytes(response.incomingTraffic)}";
            
            string text = $"uptime: {Mirror.Utils.PrettySeconds(response.uptime)} \nconnection count: {response.connectionCount}/{response.maxConnectionCount} \nplayer count: {response.playerCount} \nnetwork object count: {response.netObjectCount} \nmain object count: {response.mainObjectCount} \nmemory usage: {response.memoryUsage:F1}GB/{response.totalMemory:F1}GB \ntickrate: {response.actualTickrate} Hz / {response.tickrate} Hz \n{traffic}";
            generalText.text = text;
        }
    }
}


