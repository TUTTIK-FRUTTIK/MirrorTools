using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;

namespace MirrorTools
{
    public static class PlayersManager
    {
        public static Action onPlayersInfoResponse;
        public static string[] players;
        
        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<PlayersInfoResponse>(OnPlayerInfoResponse);
            NetworkClient.RegisterHandler<PlayerNamesResponse>(OnPlayerNamesResponse);
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<PlayersInfoRequest>(OnPlayerInfoRequest);
            NetworkServer.RegisterHandler<PlayerNamesRequest>(OnPlayerNamesRequest);
        }
        
        public static void ResetClient()
        {
            NetworkClient.UnregisterHandler<PlayersInfoResponse>();
            NetworkClient.UnregisterHandler<PlayerNamesResponse>();
            players = null;
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<PlayersInfoRequest>();
            NetworkServer.UnregisterHandler<PlayerNamesRequest>();
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

        private static void OnPlayerNamesRequest(NetworkConnection conn, PlayerNamesRequest request)
        {
            List<string> names = NetworkServer.connections.Values
                .Where(c => c.authenticationData != null)
                .Select(c => ConnectionData.GetName(c)).ToList();

            string[] ids = NetworkServer.connections.Values
                .Where(c => c.authenticationData is not ConnectionData)
                .Select(c => c.connectionId.ToString()).ToArray();
            
            names.AddRange(ids);
            string[] namesArray = names.ToArray();
            
            conn.Send(new PlayerNamesResponse() { playerNames = namesArray });
        }

        private static void OnPlayerNamesResponse(PlayerNamesResponse response)
        {
            players = response.playerNames;
            onPlayersInfoResponse?.Invoke();
        }

        public static void RequestPlayersInfo()
        {
            NetworkClient.Send(new PlayersInfoRequest());
        }

        public static void RequestPlayerNames()
        {
            NetworkClient.Send(new PlayerNamesRequest());
        }

        public static void ClearPlayerNames() => players = null;
    }
    
    public struct PlayersInfoResponse : NetworkMessage
    {
        public string playersInfo;
    }

    public struct PlayersInfoRequest : NetworkMessage { }
    
    public struct PlayerNamesRequest : NetworkMessage {}

    public struct PlayerNamesResponse : NetworkMessage
    {
        public string[] playerNames;
    }

    
    public class ConnectionData
    {
        public string displayedInfo;
        public string name;

        public static string GetName(NetworkConnectionToClient conn)
        {
            ConnectionData data = conn.authenticationData as ConnectionData;
            return data.name;
        }
    }
}



