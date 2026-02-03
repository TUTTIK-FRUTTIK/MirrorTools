using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
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
        public TextMeshProUGUI spacingText;
        public SuggestionPanel suggestionPanel;
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
            NetidentitiesManager.onIdentitiesInfoResponse += OnDataResponse;
            PlayersManager.onPlayersInfoResponse += OnDataResponse;

            suggestionPanel.Initialize();
        }

        private void OnDestroy()
        {
            NetidentitiesManager.onIdentitiesInfoResponse -= OnDataResponse;
            PlayersManager.onPlayersInfoResponse -= OnDataResponse;
        }

        private void OnInputChanged(string input)
        {
            string[] suggestions = CommandManager.GetListSuggestions(input, suggestionPanel.elementsCount, out string header, true);
            spacingText.text = CommandManager.RemoveLastPart(input).Replace(" ", "/");
            suggestionPanel.gameObject.SetActive(true);
            suggestionPanel.SetNewList(suggestions, header);
            if (suggestions != null)
                placeHolder.text = CommandManager.RemoveLastPart(input) + suggestionPanel.GetCurrentSuggestion();
            else placeHolder.text = "";
            
            MainPanel.singleton.StartCoroutine(CheckOnIncorrectInput(input));
        }

        private void OnDataResponse()
        {
            if (!gameObject.activeSelf) return;
            
            string[] suggestions = CommandManager.GetListSuggestions(inputField.text, suggestionPanel.elementsCount, out string header, false);
            spacingText.text = CommandManager.RemoveLastPart(inputField.text).Replace(" ", "/");
            suggestionPanel.gameObject.SetActive(true);
            suggestionPanel.SetNewList(suggestions, header);
            placeHolder.text = CommandManager.RemoveLastPart(inputField.text) + suggestionPanel.GetCurrentSuggestion();
        }

        IEnumerator CheckOnIncorrectInput(string input)
        {
            yield return new WaitForEndOfFrame();
            
            if (!MTools.panelIsActive)
            {
                yield return new WaitUntil(() => MTools.panelIsActive);
                inputField.text = inputField.text.Remove(inputField.text.Length - 1);
            }
        }

        private void Update()
        {
            if (!inputField.isFocused) inputField.ActivateInputField();

#if  ENABLE_INPUT_SYSTEM
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) 
                EnterText();
            
            if (Keyboard.current.backspaceKey.wasReleasedThisFrame) currentCommandIndex = 0;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame) OnUpArrowPressed();

            if (Keyboard.current.downArrowKey.wasPressedThisFrame) OnDownArrowPressed();
            
            if (Keyboard.current.tabKey.wasPressedThisFrame && !string.IsNullOrEmpty(placeHolder.text))
            {
                inputField.text = placeHolder.text;
                inputField.caretPosition = inputField.text.Length;
            }
#else
            if (Input.GetKeyDown(KeyCode.Return)) EnterText();
            
            if (Input.GetKeyDown(KeyCode.Backspace)) currentCommandIndex = 0;

            if (Input.GetKeyDown(KeyCode.UpArrow)) OnUpArrowPressed();

            if (Input.GetKeyDown(KeyCode.DownArrow)) OnDownArrowPressed();
            
            if (Input.GetKeyDown(KeyCode.Tab) && !string.IsNullOrEmpty(placeHolder.text))
            {
                inputField.text = placeHolder.text;
                inputField.caretPosition = inputField.text.Length;
            }
#endif
        }

        private void OnUpArrowPressed()
        {
            if (suggestionPanel.IsActive())
            {
                suggestionPanel.NextSuggestion();
                placeHolder.text = CommandManager.RemoveLastPart(inputField.text) + suggestionPanel.GetCurrentSuggestion();
                inputField.caretPosition = inputField.text.Length;
            }
            else OpenHistoryCommand(true);
        }

        private void OnDownArrowPressed()
        {
            if (suggestionPanel.IsActive())
            { 
                suggestionPanel.PreviousSuggestion();
                placeHolder.text = CommandManager.RemoveLastPart(inputField.text) + suggestionPanel.GetCurrentSuggestion();
                inputField.caretPosition = inputField.text.Length;
            }
            else OpenHistoryCommand(false);
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

