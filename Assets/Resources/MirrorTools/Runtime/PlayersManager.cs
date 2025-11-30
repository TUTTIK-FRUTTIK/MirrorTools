using System;
using Mirror;

namespace MirrorTools
{
    public static class PlayersManager
    {
        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<PlayersInfoResponse>(OnPlayerInfoResponse);
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<PlayersInfoRequest>(OnPlayerInfoRequest);
        }
        
        public static void ResetClient()
        {
            NetworkClient.UnregisterHandler<PlayersInfoResponse>();
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<PlayersInfoRequest>();
        }

        private static void OnPlayerInfoRequest(NetworkConnectionToClient conn, PlayersInfoRequest request)
        {
            if (!SecurityManager.IsAuthenticated(conn)) return;

            string playerList = $"({NetworkServer.connections.Count} players)\n";
            int playerNumber = 1;
            
            foreach (var player in NetworkServer.connections.Values)
            {
                if (player.authenticationData is ConnectionData data && !String.IsNullOrEmpty(data.displayedInfo))
                {
                    playerList += $"{data.displayedInfo}\n";
                    continue;
                }
                
                string playerInfo = "";
                playerInfo += $"{playerNumber++}. ";
                playerInfo += $"connId: {player.connectionId} | ";
                playerInfo += "netIdentity: ";
                playerInfo += player.identity ? player.identity.name : "<color=#ff7d7d>null<color=white>";
                playerInfo += " | ";
                playerInfo += $"ping: {Math.Round(player.rtt, 3) * 1000} mc\n";
                playerList += playerInfo;
            }
            
            conn.Send(new PlayersInfoResponse() {playersInfo = playerList});
        }

        private static void OnPlayerInfoResponse(PlayersInfoResponse response)
        {
            MainPanel.singleton.interfaceLinker.playersView.UpdatePlayersList(response.playersInfo);
        }

        public static void RequestPlayersInfo()
        {
            NetworkClient.Send(new PlayersInfoRequest());
        }
    }
    
    public struct PlayersInfoResponse : NetworkMessage
    {
        public string playersInfo;
    }

    public struct PlayersInfoRequest : NetworkMessage { }

    
    public class ConnectionData
    {
        public string displayedInfo;
        public string name;
    }
}



