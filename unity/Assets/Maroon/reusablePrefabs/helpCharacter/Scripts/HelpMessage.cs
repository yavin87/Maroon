﻿using GEAR.Localization;
using Maroon.UI;
using UnityEngine;

namespace HelpCharacter
{
    public class HelpMessage : MonoBehaviour
    {
        [SerializeField]
        private DialogueManager _dialogueManager;

        [SerializeField]
        private string _messageKey;

        [SerializeField]
        private bool _showMultipleTimes = true;

        private bool _alreadyDisplayed = false;

        public string MessageKey
        {
            get => _messageKey;
            set => _messageKey = value;
        }

        public void ShowMessage()
        {
            if (_dialogueManager == null)
                _dialogueManager = FindObjectOfType<DialogueManager>();

            if (_dialogueManager == null || (_alreadyDisplayed && !_showMultipleTimes))
                return;

            var message = LanguageManager.Instance.GetString(_messageKey);
            _dialogueManager.ShowMessage(message);
            _alreadyDisplayed = true;
        }
    }
}
