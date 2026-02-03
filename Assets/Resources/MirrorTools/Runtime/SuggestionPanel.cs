using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorTools
{
    public class SuggestionPanel : MonoBehaviour
    {
        public GameObject headerElement;
        public TextMeshProUGUI headerText;
        public TextMeshProUGUI scaleText;
        public Color suggestionColor;
        public Color suggestionImageColor;
        public int elementsCount => suggestionElements.Length;
        private SuggestionElement[] suggestionElements;
        private int currentIndex;
        private int activeElementsCount;
        private Color baseElementColor;

        public void Initialize()
        {
            List<SuggestionElement> elementList = new();
            for (int i = 1; i < transform.childCount; i++)
            {
                GameObject obj = transform.GetChild(i).gameObject;
                obj.SetActive(false);
                TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
                Image sprite = obj.GetComponent<Image>();
                elementList.Add(new SuggestionElement(obj, text, sprite));
            }
            suggestionElements = elementList.ToArray();
            baseElementColor = suggestionElements[1].sprite.color;
        }

        public void SetNewList(string[] input, string header)
        {
            headerElement.SetActive(header != "");
            headerText.text = header;

            foreach (var element in suggestionElements)
            {
                element.elementObject.SetActive(false);
                element.suggestionText.color = Color.white;
                element.sprite.color = baseElementColor;
            }
            
            SetScaleText(input, header);
            
            if (input == null || input.Length <= 0) return;

            for (int i = 0; i < input.Length; i++)
            {
                suggestionElements[i].elementObject.SetActive(true);
                suggestionElements[i].suggestionText.text = input[i];
            }
            
            activeElementsCount = input.Length;
            currentIndex = activeElementsCount - 1;
            suggestionElements[currentIndex].suggestionText.color = suggestionColor;
            suggestionElements[currentIndex].sprite.color = suggestionImageColor;
        }
        
        private void SetScaleText(string[] input, string header)
        {
            if (input == null)
            {
                scaleText.text = header;
                return;
            }
            
            float maxSize = 0;
            string targetInput = "";
            foreach (var str in input)
            {
                scaleText.text = str;
                if (scaleText.preferredWidth > maxSize)
                {
                    maxSize = scaleText.preferredWidth;
                    targetInput = str;
                }
            }

            scaleText.text = header;
            if (scaleText.preferredWidth > maxSize) return;
            
            scaleText.text = targetInput;
        }

        public string GetCurrentSuggestion()
        {
            string text = suggestionElements[currentIndex].suggestionText.text;
            if (text.Contains(" ")) text = $"'{text}'";
            return text;
        }

        public void NextSuggestion()
        {
            suggestionElements[currentIndex].suggestionText.color = Color.white;
            suggestionElements[currentIndex].sprite.color = baseElementColor;
            currentIndex = currentIndex - 1 < 0 ? activeElementsCount - 1 : currentIndex - 1;
            suggestionElements[currentIndex].suggestionText.color = suggestionColor;
            suggestionElements[currentIndex].sprite.color = suggestionImageColor;
        }

        public void PreviousSuggestion()
        {
            suggestionElements[currentIndex].suggestionText.color = Color.white;
            suggestionElements[currentIndex].sprite.color = baseElementColor;
            currentIndex = currentIndex + 1 >= activeElementsCount ? 0 : currentIndex + 1;
            suggestionElements[currentIndex].suggestionText.color = suggestionColor;
            suggestionElements[currentIndex].sprite.color = suggestionImageColor;
        }

        public bool IsActive()
        {
            foreach (var element in suggestionElements) if (element.elementObject.activeSelf) return true;
            return false;
        }
        
        private struct SuggestionElement
        {
            public GameObject elementObject;
            public TextMeshProUGUI suggestionText;
            public Image sprite;

            public SuggestionElement(GameObject obj, TextMeshProUGUI text, Image image)
            {
                elementObject = obj;
                suggestionText = text;
                sprite = image;
            }
        }
    }
}

