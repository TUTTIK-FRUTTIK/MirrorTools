using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MirrorTools
{
    public class CommandPanel : MonoBehaviour
    {
        public TextMeshProUGUI consoleText;
        public TMP_InputField inputField;
        public TextMeshProUGUI placeHolder;
        private List<string> commandHistory = new List<string>();
        private int currentCommandIndex;

        private void Start()
        {
            if (PlayerPrefs.HasKey("commandHistory"))
            {
                string[] lines = PlayerPrefs.GetString("commandHistory").Split('\n');
                commandHistory.AddRange(lines);
                if (commandHistory[^1] == "") commandHistory.RemoveAt(commandHistory.Count - 1);
            }
            
            inputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void OnInputChanged(string input)
        {
            placeHolder.text = CommandManager.GetSuggestion(input);
        }

        private void Update()
        {
            if (!inputField.isFocused) inputField.ActivateInputField();

#if  ENABLE_INPUT_SYSTEM
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) 
                EnterText();
            
            if (Keyboard.current.backspaceKey.wasReleasedThisFrame) currentCommandIndex = 0;
            
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) OpenHistoryCommand(true);
            
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) OpenHistoryCommand(false);
            
            if (Keyboard.current.tabKey.wasPressedThisFrame && !string.IsNullOrEmpty(placeHolder.text))
            {
                inputField.text = placeHolder.text;
                inputField.caretPosition = inputField.text.Length;
            }
#else
            if (Input.GetKeyDown(KeyCode.Return)) EnterText();
            
            if (Input.GetKeyDown(KeyCode.Backspace)) currentCommandIndex = 0;
            
            if (Input.GetKeyDown(KeyCode.UpArrow)) OpenHistoryCommand(true);

            if (Input.GetKeyDown(KeyCode.DownArrow)) OpenHistoryCommand(false);
            
            if (Input.GetKeyDown(KeyCode.Tab) && !string.IsNullOrEmpty(placeHolder.text))
            {
                inputField.text = placeHolder.text;
                inputField.caretPosition = inputField.text.Length;
            }
#endif
        }

        private void EnterText()
        {
            if (string.IsNullOrEmpty(inputField.text)) return;

            if (inputField.text.ToLower() == "clear")
            {
                inputField.text = "";
                consoleText.text = "";
                return;
            }
                
            consoleText.text += $">{inputField.text}\n";
            if (CommandManager.CommandHasError(inputField.text, out string description))
            {
                consoleText.text += $"{description}\n";
            }
            else
            {
                CommandManager.SendCommand(inputField.text);
            }
                
            AddCommandToHistory(inputField.text);
            consoleText.text = GetLimitedLines(consoleText.text, 200);
            inputField.text = "";
            inputField.ActivateInputField();
            currentCommandIndex = 0;
        }

        private void OpenHistoryCommand(bool up)
        {
            int commandCount = commandHistory.Count;
            
            if (commandCount == 0) return;
            if (up && currentCommandIndex >= commandCount) return;
            if (!up && currentCommandIndex <= 1) return;
            
            int currentCommand = commandCount - (up ? ++currentCommandIndex : --currentCommandIndex);
            inputField.text = commandHistory[currentCommand];
            inputField.caretPosition = inputField.text.Length;
        }
        
        private string GetLimitedLines(string input, int limit)
        {
            if (string.IsNullOrEmpty(input))
                return input;
    
            string[] lines = input.Split('\n');
    
            if (lines.Length <= limit)
                return input;
    
            string[] lastLines = lines.Skip(lines.Length - limit).ToArray();
            return string.Join("\n", lastLines);
        }

        private void AddCommandToHistory(string command)
        {
            if (commandHistory.Count > 0 && commandHistory[^1] == command) return;
            
            if (commandHistory.Count > 10) commandHistory.RemoveAt(0);
            
            commandHistory.Add(command);
            string history = "";
            foreach (var line in commandHistory) history += $"{line}\n";
            PlayerPrefs.SetString("commandHistory", history);
            PlayerPrefs.Save();
        }
        
        public void AddTextToConsole(string text)
        {
            consoleText.text += text;
            consoleText.text += "\n";
            consoleText.text = GetLimitedLines(consoleText.text, 200);
        }
    }
}

