using System;
using System.Linq;
using UnityEngine;
using Mirror;

namespace MirrorTools
{
    public static class NetidentitiesManager
    {
        public static Action onIdentitiesInfoResponse;
        public static string[] netIdentities;
        
        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<IdentitiesInfoResponse>(OnIdentitiesInfoResponse);
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<IdentitiesInfoRequest>(OnIdentitiesInfoRequest);
        }
        
        public static void ResetClient()
        {
            NetworkClient.UnregisterHandler<IdentitiesInfoResponse>();
            netIdentities = null;
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<IdentitiesInfoRequest>();
        }

        private static void OnIdentitiesInfoRequest(NetworkConnectionToClient conn, IdentitiesInfoRequest request)
        {
            if (!SecurityManager.IsAuthenticated(conn)) return;
            
            string[] names = NetworkServer.spawned.Select(go => go.Value.name).ToArray();
            
            conn.Send(new IdentitiesInfoResponse() { indentitiesInfo = names });
        }

        private static void OnIdentitiesInfoResponse(IdentitiesInfoResponse response)
        {
            netIdentities = response.indentitiesInfo;
            onIdentitiesInfoResponse?.Invoke();
            MainPanel.singleton.interfaceLinker.netidentitiesView.UpdateIdentitiesList(response.indentitiesInfo);
        }

        public static void RequestIdentitiesInfo()
        {
            NetworkClient.Send(new IdentitiesInfoRequest());
        }
    }
    
    public struct IdentitiesInfoResponse : NetworkMessage
    {
        public string[] indentitiesInfo;
    }

    public struct IdentitiesInfoRequest : NetworkMessage { }
}

