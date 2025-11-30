using UnityEngine;
using Mirror;

namespace MirrorTools
{
    public class NetidentitiesManager : MonoBehaviour
    {
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
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<IdentitiesInfoRequest>();
        }

        private static void OnIdentitiesInfoRequest(NetworkConnectionToClient conn, IdentitiesInfoRequest request)
        {
            if (!SecurityManager.IsAuthenticated(conn)) return;

            string identitiesList = $"({NetworkServer.spawned.Count} net identities)\n";
            int identityCount = 1;

            foreach (NetworkIdentity identity in NetworkServer.spawned.Values)
            {
                identitiesList += $"{identityCount++}. {identity.name}\n";
            }
            
            conn.Send(new IdentitiesInfoResponse() { indentitiesInfo = identitiesList });
        }

        private static void OnIdentitiesInfoResponse(IdentitiesInfoResponse response)
        {
            MainPanel.singleton.interfaceLinker.netidentitiesView.UpdateIdentitiesList(response.indentitiesInfo);
        }

        public static void RequestIdentitiesInfo()
        {
            NetworkClient.Send(new IdentitiesInfoRequest());
        }
    }
    
    public struct IdentitiesInfoResponse : NetworkMessage
    {
        public string indentitiesInfo;
    }

    public struct IdentitiesInfoRequest : NetworkMessage { }
}

