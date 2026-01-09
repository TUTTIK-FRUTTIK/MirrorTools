using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mirror;

namespace MirrorTools
{
    public static class CommandManager
    {
        private static Dictionary<string, CommandData> commands = new Dictionary<string, CommandData>();

        public static void Initialize()
        {
            RegisterAllCommands();
        }

        public static void RegisterServerHandlers()
        {
            NetworkServer.RegisterHandler<ConsoleCommandMessage>(OnConsoleCommandMessage);
        }

        public static void RegisterClientHandlers()
        {
            NetworkClient.RegisterHandler<ConsoleMessage>(OnConsoleMessage);
        }

        public static void ResetServer()
        {
            NetworkServer.UnregisterHandler<ConsoleCommandMessage>();
        }

        public static void ResetClient()
        {
            NetworkClient.UnregisterHandler<ConsoleMessage>();
        }

        private static void RegisterAllCommands()
        {
            commands.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("Assembly-CSharp")) continue;
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        RegisterCommandsInType(type);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Register commands failed: " + ex);
                }
            }
        }

        private static void RegisterCommandsInType(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Static | BindingFlags.Instance);


            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
                if (attribute != null)
                {
                    if (!method.IsStatic && !typeof(NetworkBehaviour).IsAssignableFrom(type)) continue;
                    
                    string commandName = attribute.command.ToLower();

                    if (commands.ContainsKey(commandName))
                    {
                        Debug.LogError($"Command '{commandName}' was registered more than once.");
                        continue;
                    }

                    commands[commandName] = new CommandData(attribute.description, method, type);
                }
            }
        }

        public static bool CommandHasError(string command, out string exception)
        {
            exception = "";
            if (!commands.ContainsKey(command.Split()[0]))
            {
                exception = "<color=red>Unknown or incomplete command. Type \'help\' to see command list.<color=white>";
                return true;
            }

            return false;
        }

        private static void OnConsoleCommandMessage(NetworkConnectionToClient conn, ConsoleCommandMessage message)
        {
            if (!SecurityManager.IsAuthenticated(conn)) return;

            string command = message.command.Split()[0];

            if (!TryParseParams(message.command, commands[command].methodInfo, conn, out object[] args))
            {
                MTools.ConsoleWrite(conn, "Command has wrong parameters.", Color.red);
                return;
            }

            if (commands[command].methodInfo.IsStatic)
            {
                commands[command].methodInfo.Invoke(null, args);
            }
            else
            {
                foreach (var identity in NetworkServer.spawned.Values.ToArray())
                {
                    foreach (var netbehaviour in identity.NetworkBehaviours)
                    {
                        if (commands[command].type.FullName == netbehaviour.GetType().FullName)
                        {
                            commands[command].methodInfo.Invoke(netbehaviour, args);
                        }
                    }
                }
            }
        }

        private static bool TryParseParams(string command, MethodInfo method, NetworkConnectionToClient sender,
            out object[] args)
        {
            args = new object[] { };
            List<object> result = new List<object>();
            ParameterInfo[] parameters = method.GetParameters();
            string[] commandParts = SplitString(command);
            int offset = 1;

            if (!SameParametersCount(commandParts, parameters)) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(NetworkConnectionToClient) && parameters[i].Name == "sender")
                {
                    result.Add(sender);
                    offset--;
                    continue;
                }

                switch (parameters[i].ParameterType)
                {
                    case { } t when t == typeof(bool):
                        if (bool.TryParse(commandParts[i + offset], out bool b)) result.Add(b);
                        else return false;
                        break;
                    case { } t when t == typeof(int):
                        if (int.TryParse(commandParts[i + offset], out int _i)) result.Add(_i);
                        else return false;
                        break;
                    case { } t when t == typeof(float):
                        if (TryParseFloat(commandParts[i + offset], out float f)) result.Add(f);
                        else return false;
                        break;
                    case { } t when t == typeof(NetworkIdentity):
                        if (TryParseNetIdentity(commandParts[i + offset], out NetworkIdentity identity))
                            result.Add(identity);
                        else return false;
                        break;
                    case { } t when t == typeof(NetworkConnectionToClient):
                        if (TryParseNetworkConnection(commandParts[i + offset], out NetworkConnectionToClient conn))
                            result.Add(conn);
                        else return false;
                        break;
                    case { } t when t == typeof(string):
                        result.Add(commandParts[i + offset]);
                        break;
                }
            }

            args = result.ToArray();
            return true;
        }

        private static bool SameParametersCount(string[] commandParts, ParameterInfo[] parameters)
        {
            bool hasSenderParameter = false;

            foreach (var parameter in parameters)
            {
                if (parameter.ParameterType == typeof(NetworkConnectionToClient) && parameter.Name == "sender")
                    hasSenderParameter = true;
            }

            if ((hasSenderParameter ? commandParts.Length : commandParts.Length - 1) != parameters.Length)
            {
                return false;
            }

            return true;
        }

        private static string[] SplitString(string input, char delimiter = ' ')
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<string>();

            var parts = new List<string>();
            char? currentQuote = null;
            int startIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '"' || input[i] == '\'')
                {
                    if (currentQuote == null)
                    {
                        currentQuote = input[i];
                    }
                    else if (currentQuote == input[i])
                    {
                        currentQuote = null;
                    }
                }
                else if (input[i] == delimiter && currentQuote == null)
                {
                    if (i > startIndex)
                    {
                        string part = input.Substring(startIndex, i - startIndex);
                        parts.Add(RemoveOuterQuotes(part));
                    }

                    startIndex = i + 1;
                }
            }

            if (startIndex < input.Length)
            {
                string lastPart = input.Substring(startIndex);
                parts.Add(RemoveOuterQuotes(lastPart));
            }

            return parts.ToArray();
        }

        private static string RemoveOuterQuotes(string text)
        {
            if (text.Length >= 2 && (text[0] == '"' || text[0] == '\'') && text[0] == text[^1])
            {
                return text.Substring(1, text.Length - 2);
            }

            return text;
        }

        private static bool TryParseFloat(string input, out float result)
        {
            result = 0f;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            string normalizedInput = input.Replace(',', '.');

            if (float.TryParse(normalizedInput, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return true;

            if (float.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
                return true;

            return false;
        }

        private static bool TryParseNetIdentity(string input, out NetworkIdentity identity)
        {
            identity = null;
            NetworkIdentity target = NetworkServer.spawned.Values.FirstOrDefault(go => go.name == input);
            if (target)
            {
                identity = target;
                return true;
            }

            return false;
        }

        private static bool TryParseNetworkConnection(string input, out NetworkConnectionToClient conn)
        {
            conn = null;
            NetworkConnectionToClient[] connList = NetworkServer.connections.Values.ToArray();
            NetworkConnectionToClient target =
                connList.FirstOrDefault(c => c != null &&
                                             c.authenticationData != null &&
                                             c.authenticationData is ConnectionData data &&
                                             data.name == input);

            if (target == null)
                target =
                    connList.FirstOrDefault(c => c != null &&
                                                 (c.authenticationData == null ||
                                                  c.authenticationData is not ConnectionData) &&
                                                 c.connectionId.ToString() == input);
            if (target != null)
            {
                conn = target;
                return true;
            }

            return false;
        }

        [ConsoleCommand("help", "displays the available commands.")]
        private static void HelpCommand(NetworkConnectionToClient sender)
        {
            string commandList = "";
            int count = 1;
            foreach (var command in commands)
            {
                commandList += $"{count++}. <b>'{command.Key}'</b> {command.Value.description}";

                if (count <= commands.Count) commandList += "\n";
            }

            MTools.ConsoleWrite(sender, commandList);
        }

        public static void SendCommand(string command)
        {
            NetworkClient.Send(new ConsoleCommandMessage() { command = command });
        }

        private static void OnConsoleMessage(ConsoleMessage message)
        {
            MainPanel.singleton.interfaceLinker.commandPanel.AddTextToConsole(message.text);
        }

        public static string GetSuggestion(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string[] parts = SplitString(input);

            if (parts.Length == 1 && input[^1] != ' ')
            {
                var matches = commands.Keys.Where(cmd => cmd.StartsWith(input)).ToList();
                if (matches.Count > 0) return matches[0];
                else return string.Empty;
            }
            else if (parts.Length > 0 && input[^1] == ' ')
            {
                if (commands.ContainsKey(parts[0]))
                {
                    ParameterInfo[] parameters = commands[parts[0]].methodInfo.GetParameters();
                    bool hasSenderParameter = MethodHasSenderParameter(parameters, out int index);
                    int parametersCount = hasSenderParameter ? parameters.Length - 1 : parameters.Length;
                    if (parametersCount > 0 && parts.Length <= parametersCount)
                    {
                        int offset = hasSenderParameter && parts.Length > index ? 0 : 1;
                        return $"{input}{parameters[parts.Length - offset].Name}({GetTypeName(parameters[parts.Length - offset].ParameterType)})";
                    }
                }
            }

            return string.Empty;
        }

        private static bool MethodHasSenderParameter(ParameterInfo[] parameters, out int index)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(NetworkConnectionToClient) && parameters[i].Name == "sender")
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }

        private static string GetTypeName(Type type)
        {
            if (type.Name == "Single") return "Float";
            else if (type.Name == "Int32") return "Int";
            else if (type.Name == "Boolean") return "Bool";
            else return type.Name;
        }
    }

    public struct CommandData
    {
        public string description;
        public MethodInfo methodInfo;
        public Type type;

        public CommandData(string description, MethodInfo method, Type type)
        {
            this.description = description;
            this.methodInfo = method;
            this.type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string command { get; private set; }
        public string description { get; private set; }

        public ConsoleCommandAttribute(string command)
        {
            this.command = command;
        }

        public ConsoleCommandAttribute(string command, string description = "")
        {
            this.command = command;
            this.description = description;
        }
    }

    public struct ConsoleCommandMessage : NetworkMessage
    {
        public string command;
    }

    public struct ConsoleMessage : NetworkMessage
    {
        public string text;
    }
}