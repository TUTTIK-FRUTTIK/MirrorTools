using System;
using System.Collections.Generic;
using Mirror;

namespace MirrorTools
{
    public static class LogManager
    {
        private static List<string> logs = new List<string>();

        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<LogsResponse>(OnLogsResponse);
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<LogsRequest>(OnLogsRequest);
        }

        public static void ResetClient()
        {
            NetworkClient.UnregisterHandler<LogsRequest>();
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<LogsRequest>();
            logs.Clear();
        }

        private static void OnLogsRequest(NetworkConnectionToClient conn, LogsRequest request)
        {
            if (!SecurityManager.IsAuthenticated(conn)) return;

            string logList = "";

            foreach (var log in logs)
            {
                logList += log + "\n";
            }

            conn.Send(new LogsResponse() { logs = logList });
        }

        private static void OnLogsResponse(LogsResponse response)
        {
            MainPanel.singleton.interfaceLinker.logView.UpdateLogs(response.logs);
        }

        public static void SendLog(string log)
        {
            if (logs.Count > 50)  logs.RemoveAt(0);
            logs.Add(log);
        }

        public static void RequestLogs()
        {
            NetworkClient.Send(new LogsRequest());
        }
    }

    public struct LogsResponse : NetworkMessage
    {
        public string logs;
    }

    public struct LogsRequest : NetworkMessage { }

}