using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Unity
{
    public class HotbarButton
    {
        private readonly GameObject gameObject;

        public readonly Button button;
        public readonly Image image;
        public readonly GameObject selectedImage;
        public readonly TMP_Text text;

        public UnityAction onSelected;
        public UnityAction onUnSelected;
        public UnityAction onClick;

        public HotbarButton(GameObject gameObject)
        {
            this.gameObject = gameObject;
            button = gameObject.GetComponent<Button>();
            image = gameObject.transform.Find("Image").GetComponent<Image>();
            selectedImage = gameObject.transform.Find("SelectedImage").gameObject;
            text = gameObject.transform.Find("Text (TMP)").GetComponent<TMP_Text>();

            button.onClick.AddListener(() =>
            {
                // perform onclick if this item was already selected
                //if (selectedImage.activeInHierarchy)
                //    Click();
                //else
                //    Selected();
                Selected();
                Click();
            });
        }

        public void Click()
        {
            onClick?.Invoke();
            gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);
            GameController.Instance.playerController.StartCoroutine(this.ResetColor());
        }

        public void Selected() // selection by scroll/click
        {
            selectedImage.SetActive(true);

            onSelected?.Invoke();

            gameObject.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.2f);
            GameController.Instance.playerController.StartCoroutine(this.ResetColor());
        }

        public void UnSelected()
        {
            selectedImage.SetActive(false);

            onUnSelected?.Invoke();
        }

        IEnumerator ResetColor()
        {
            yield return new WaitForSeconds(0.2f);
            gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
    }
}