using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


namespace  MirrorTools
{
    public static class SecurityManager
    {
        public static List<NetworkConnectionToClient> authenticatedConnections =  new List<NetworkConnectionToClient>();
        public static bool isAuthenticated;
        
        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<OpenAdminRequest>(OnOpenAdminRequest);
            NetworkServer.OnDisconnectedEvent += OnClientDisconntected;
        }
        
        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<OpenAdminResponse>(OnOpenAdminResponse);
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<OpenAdminRequest>();
            NetworkServer.OnDisconnectedEvent -= OnClientDisconntected;
            authenticatedConnections.Clear();
        }
        
        public static void ResetClient()
        {
            NetworkServer.UnregisterHandler<OpenAdminRequest>();
        }

        private static void OnOpenAdminRequest(NetworkConnectionToClient conn, OpenAdminRequest request)
        {
            OpenAdminResponse response = new OpenAdminResponse();
            if (String.IsNullOrEmpty(MainPanel.singleton.config.password))
            {
                response.success = true;
                authenticatedConnections.Add(conn);
            }
            else
            {
                response.success = MainPanel.singleton.config.password == request.key;
                if (response.success) authenticatedConnections.Add(conn);
            }
            response.key = request.key;
            
            conn.Send(response);
        }

        private static void OnOpenAdminResponse(OpenAdminResponse response)
        {
            if (response.success)
            {
                PlayerPrefs.SetString("MirrorToolsAdminKey", response.key);
                PlayerPrefs.Save();
                MainPanel.singleton.interfaceLinker.passwordPanel.SetActive(false);
                MainPanel.singleton.interfaceLinker.infoPanel.SetActive(true);
                isAuthenticated = true;
            }
            else
            {
                MainPanel.singleton.interfaceLinker.passwordPanel.SetActive(true);
                MainPanel.singleton.interfaceLinker.infoPanel.SetActive(false);
                isAuthenticated = false;
            }
        }

        private static void OnClientDisconntected(NetworkConnectionToClient conn)
        {
            if (authenticatedConnections.Contains(conn))  authenticatedConnections.Remove(conn);
        }

        public static void TryOpenAdminPanel(bool panelActive)
        {
            string key = null;
            if (PlayerPrefs.HasKey("MirrorToolsAdminKey")) key = PlayerPrefs.GetString("MirrorToolsAdminKey");
            if (panelActive)
            {
                if (!NetworkClient.isConnected) return;
                NetworkClient.Send(new OpenAdminRequest() { key = key });
            }
        }
        
        public static void EnterPassword(string key)
        {
            NetworkClient.Send(new OpenAdminRequest() { key = key });
        }
        
        public static bool IsAuthenticated(NetworkConnectionToClient conn)
        {
            return authenticatedConnections.Contains(conn);
        }
    }
    
    public struct OpenAdminRequest : NetworkMessage
    {
        public string key;
    }

    public struct OpenAdminResponse : NetworkMessage
    {
        public bool success;
        public string key;
    }
}



