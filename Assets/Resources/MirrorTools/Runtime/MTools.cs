using Mirror;
using UnityEngine;
using System;

namespace MirrorTools
{
    public static class MTools
    {
        /// <summary>It is triggered when the MirrorTools panel is activated.</summary>
        public static Action onPanelActivate;
        
        /// <summary>It is triggered when the MirrorTools panel is deactivated.</summary>
        public static Action onPanelDeactivate;
        
        /// <summary>Current state of the MirrorTools panel.</summary>
        public static bool panelIsActive => MainPanel.singleton.panel.activeSelf;
        
        /// <summary>Displays a message to the command line.</summary>
        public static void ConsoleWrite(NetworkConnectionToClient conn, string message)
        {
            conn.Send(new ConsoleMessage { text = message });
        }
        
        /// <summary>Displays a message to the command line.</summary>
        public static void ConsoleWrite(NetworkConnectionToClient conn, string message, Color color)
        {
            string hexRGB = ColorUtility.ToHtmlStringRGB(color);
            conn.Send(new ConsoleMessage { text = $"<color=#{hexRGB}>{message}<color=white>" });
        }

        /// <summary>Displays a message in the log panel.</summary>
        public static void Log(string text)
        {
            string log = $"[{DateTime.Now.ToString("HH:mm:ss")}] {text}";
            LogManager.SendLog(log);
        }

        /// <summary>Displays a message in the log panel.</summary>
        public static void Log(string text, Color color)
        {
            string hexRGB = ColorUtility.ToHtmlStringRGB(color);
            string log = $"[{DateTime.Now.ToString("HH:mm:ss")}] <color=#{hexRGB}>{text}<color=white>";
            LogManager.SendLog(log);
        }
    }
}

