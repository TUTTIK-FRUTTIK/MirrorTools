using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class GeneralView : MonoBehaviour
    {
        public TextMeshProUGUI generalText;

        public void UpdateGeneralInfo(GeneralInfoResponse response)
        {
            string text = $"connection count: {response.connectionCount}/{response.maxConnectionCount} \nplayer count: {response.playerCount} \nnetwork object count: {response.netObjectCount} \nmain object count: {response.mainObjectCount} \nmemory usage: {response.memoryUsage:F1}GB/{response.totalMemory:F1}GB \ncurrent fps: {response.fps}";
            generalText.text = text;
        }
    }
}


