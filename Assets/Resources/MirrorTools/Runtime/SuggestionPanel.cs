using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class SuggestionPanel : MonoBehaviour
    {
        public TextMeshProUGUI scaleText;
        public Color suggestionColor;
        public int elementsCount => suggestionElements.Length;
        private SuggestionElement[] suggestionElements;
        private int currentIndex;
        private int activeElementsCount;

        public void Initialize()
        {
            List<SuggestionElement> elementList = new();
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject obj = transform.GetChild(i).gameObject;
                TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
                elementList.Add(new SuggestionElement(obj, text));
            }
            suggestionElements = elementList.ToArray();
        }

        public void SetNewList(string[] input)
        {
            scaleText.text = input.OrderByDescending(s => s.Length).FirstOrDefault();

            foreach (var element in suggestionElements)
            {
                element.elementObject.SetActive(false);
                element.suggestionText.color = Color.white;
            }

            for (int i = 0; i < input.Length; i++)
            {
                suggestionElements[i].elementObject.SetActive(true);
                suggestionElements[i].suggestionText.text = input[i];
            }
            
            activeElementsCount = input.Length;
            currentIndex = activeElementsCount - 1;
            suggestionElements[currentIndex].suggestionText.color = suggestionColor;
        }

        public string GetCurrentSuggestion()
        {
            return suggestionElements[currentIndex].suggestionText.text;
        }

        public void NextSuggestion()
        {
            suggestionElements[currentIndex].suggestionText.color = Color.white;
            currentIndex = currentIndex - 1 < 0 ? activeElementsCount - 1 : currentIndex - 1;
            suggestionElements[currentIndex].suggestionText.color = suggestionColor;
        }

        public void PreviousSuggestion()
        {
            suggestionElements[currentIndex].suggestionText.color = Color.white;
            currentIndex = currentIndex + 1 >= activeElementsCount ? 0 : currentIndex + 1;
            suggestionElements[currentIndex].suggestionText.color = suggestionColor;
        }
        
        private struct SuggestionElement
        {
            public GameObject elementObject;
            public TextMeshProUGUI suggestionText;

            public SuggestionElement(GameObject obj, TextMeshProUGUI text)
            {
                elementObject = obj;
                suggestionText = text;
            }
        }
    }
}

