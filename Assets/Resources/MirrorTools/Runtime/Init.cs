using UnityEngine;
using Mirror;
using System.Linq;
using UnityEditor;

namespace MirrorTools
{
    public static class Init
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            CommandManager.Initialize();
            try
            {
                GameObject panel = Resources.Load<GameObject>("MirrorTools/MirrorToolsPanel");
                GameObject.Instantiate(panel);
            }
            finally
            {
                Resources.UnloadUnusedAssets();
            }
        }
        
        public static void InitializeServer()
        {
            SecurityManager.RegisterServerHandlers();
            GeneralManager.RegisterServerHandlers();
            PlayersManager.RegisterServerHandlers();
            NetidentitiesManager.RegisterServerHandlers();
            LogManager.RegisterServerHandlers();
            CommandManager.RegisterServerHandlers();
        }
        
        public static void InitializeClient()
        {
            SecurityManager.RegisterClientHandlers();
            GeneralManager.RegisterClientHandlers();
            PlayersManager.RegisterClientHandlers();
            NetidentitiesManager.RegisterClientHandlers();
            LogManager.RegisterClientHandlers();
            CommandManager.RegisterClientHandlers();
        }
        
        public static void ResetServer()
        {
            SecurityManager.ResetServer();
            GeneralManager.ResetServer();
            PlayersManager.ResetServer();
            NetidentitiesManager.ResetServer();
            LogManager.ResetServer();
            CommandManager.ResetServer();
        }

        public static void ResetClient()
        {
            SecurityManager.ResetClient();
            GeneralManager.ResetClient();
            PlayersManager.ResetClient();
            NetidentitiesManager.ResetClient();
            LogManager.ResetClient();
            CommandManager.ResetClient();
        }
    }
}

