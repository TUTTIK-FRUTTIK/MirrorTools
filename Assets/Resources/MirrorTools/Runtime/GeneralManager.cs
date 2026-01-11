using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirrorTools
{
    public static class GeneralManager
    {
        private static NetworkStatistics networkStatistics;
        
        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<GeneralInfoResponse>(OnGeneralInfoResponse);
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<GeneralInfoRequest>(OnGeneralInfoRequest);

            if (NetworkManager.singleton.TryGetComponent(out NetworkStatistics stats))
            {
                networkStatistics = stats;
                stats.enabled = false;
            }
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

            bool hasStats = networkStatistics != null;
            
            GeneralInfoResponse response = new GeneralInfoResponse()
            {
                connectionCount = NetworkServer.connections.Count,
                maxConnectionCount = NetworkManager.singleton.maxConnections,
                playerCount = NetworkManager.singleton.numPlayers,
                netObjectCount = NetworkServer.spawned.Count,
                mainObjectCount = SceneManager.GetActiveScene().rootCount,
                actualTickrate = NetworkServer.actualTickRate,
                tickrate = NetworkServer.tickRate,
                memoryUsage = (long)(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576L) / 1024f,
                totalMemory = SystemInfo.systemMemorySize / 1024f,
                uptime = NetworkTime.time,
                outgoingTraffic = hasStats ? networkStatistics.serverSentBytesPerSecond : -1,
                incomingTraffic = hasStats ? networkStatistics.serverReceivedBytesPerSecond : -1,
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
        public int actualTickrate;
        public int tickrate;
        public float memoryUsage;
        public float totalMemory;
        public double uptime;
        public long outgoingTraffic;
        public long incomingTraffic;
    }
}

