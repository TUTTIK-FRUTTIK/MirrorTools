using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;

namespace MirrorTools
{
    public static class GeneralManager
    {
        private static Process currentProcess;
        
        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<GeneralInfoResponse>(OnGeneralInfoResponse);
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<GeneralInfoRequest>(OnGeneralInfoRequest);
            currentProcess = Process.GetCurrentProcess();
        }
        
        public static void ResetClient()
        {
            NetworkClient.UnregisterHandler<GeneralInfoRequest>();
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<GeneralInfoRequest>();
        }
        
        private static void OnGeneralInfoResponse(GeneralInfoResponse response)
        {
            MainPanel.singleton.interfaceLinker.generalView.UpdateGeneralInfo(response);
        }

        private static void OnGeneralInfoRequest(NetworkConnectionToClient conn, GeneralInfoRequest request)
        {
            if (!SecurityManager.IsAuthenticated(conn)) return;
            
            GeneralInfoResponse response = new GeneralInfoResponse()
            {
                connectionCount = NetworkServer.connections.Count,
                maxConnectionCount = NetworkManager.singleton.maxConnections,
                playerCount = NetworkManager.singleton.numPlayers,
                netObjectCount = NetworkServer.spawned.Count,
                mainObjectCount = SceneManager.GetActiveScene().rootCount,
                fps = (int)(1f / Time.unscaledDeltaTime),
                memoryUsage = (long)(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576L) / 1024f,
                totalMemory = SystemInfo.systemMemorySize / 1024f
            };
            
            conn.Send(response);
        }

        public static void RequestGeneralInfo()
        {
            NetworkClient.Send(new GeneralInfoRequest());
        }
    }
    
    public struct GeneralInfoRequest : NetworkMessage { }

    public struct GeneralInfoResponse : NetworkMessage
    {
        public int connectionCount;
        public int maxConnectionCount;
        public int playerCount;
        public int netObjectCount;
        public int mainObjectCount;
        public int fps;
        public float memoryUsage;
        public float totalMemory;
    }
}

