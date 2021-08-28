using Assets.Scripts.Context;
using Assets.Scripts.Extensions;
using Assets.Scripts.Loaders;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Assets.Scripts.Model.Unity
{

    public class BlueprintButton : MonoBehaviour
    {
        public Button button;
        public TMP_Text title;
        public TMP_Text extraText;
        public Image icon;

        public BlueprintContext BlueprintContextReference;

        IEnumerator Start()
        {
            yield return null; // do it next frame
            BlueprintContextReference.LoadDescription();
            BlueprintContextReference.LoadIcon();
            Initialize();
        }

        public void Initialize()
        {
            title.text = BlueprintContextReference.Description.Name;

            long bpBytes = BlueprintContextReference.GetBlueprintSize();
            extraText.text = bpBytes.ToPrettySize(2);

            var image = BlueprintContextReference.Icon;

            icon.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f));

            BlueprintSearch.OnTextFilter += ApplySearchFilter;
            // HaCkY code:
            if (!string.IsNullOrWhiteSpace(BlueprintSearch.searchText))
                this.ApplySearchFilter(BlueprintSearch.searchText);
        }

        public void Setup(BlueprintContext blueprintContext)
        {
            BlueprintContextReference = blueprintContext;
            blueprintContext.btn = this;
        }

        /// <summary>
        /// click event that should trigger when selecting a blueprint button
        /// </summary>
        public void SelectBlueprint() // todo: make it so when creating button the blueprintmenu subscribes to a onSelectedBlueprintButton event
        {
            GameController.Instance.blueprintMenu.SelectBlueprintButton(this);
        }

        public void ApplySearchFilter(string text)
        {
            bool active = title.text.ToLower().Contains(text);
            gameObject.SetActive(active);
            if (!active && BlueprintMenu.SelectedBlueprintButton == this)
            {
                BlueprintMenu.SelectedBlueprintButton = null;

                Color color = this.button.image.color;
                color.a = 40f / 255f;
                this.button.image.color = color;
            }
        }
    }

}