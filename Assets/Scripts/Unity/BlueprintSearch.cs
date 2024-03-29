﻿using UnityEngine;

namespace Assets.Scripts.Unity
{
    public class BlueprintSearch : MonoBehaviour
    {
        public TMPro.TMP_InputField input;

        public delegate void FilterText(string text);
        public static event FilterText OnTextFilter;

        public static string searchText = "";
        public void OnTextChanged()
        {
            OnTextFilter(input.text.ToLower());
            searchText = input.text.ToLower();
        }
    }
}