using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Assets.Scripts.Unity
{
    public class MessageController : MonoBehaviour
    {
        [SerializeField] private GameObject BackGroundPanel;
        [SerializeField] private GameObject WarningMessagePanel;

        [SerializeField] private GameObject OkMessagePanel;
        [SerializeField] private GameObject OkCancelMessagePanel;
        [SerializeField] private GameObject YesNoMessagePanel;

        private Action OnOkMessageOk;
        private Action OnOkCancelMessageOk;
        private Action OnOkCancelMessageCancel;
        private Action OnYesNoMessageYes;
        private Action OnYesNoMessageNo;


        private void Awake()
        {
            BackGroundPanel.SetActive(false);
            WarningMessagePanel.SetActive(false);
            OkMessagePanel.SetActive(false);
            OkCancelMessagePanel.SetActive(false);
            YesNoMessagePanel.SetActive(false);

            var btn = OkMessagePanel.transform.Find("Button").GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                OkMessagePanel.SetActive(false);
                BackGroundPanel.SetActive(false);
                OnOkMessageOk?.Invoke();
            });

            btn = OkCancelMessagePanel.transform.Find("Panel").Find("OkButton").GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                OkCancelMessagePanel.SetActive(false);
                BackGroundPanel.SetActive(false);
                OnOkCancelMessageOk?.Invoke();
            });
            btn = OkCancelMessagePanel.transform.Find("Panel").Find("CancelButton").GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                OkCancelMessagePanel.SetActive(false);
                BackGroundPanel.SetActive(false);
                OnOkCancelMessageCancel?.Invoke();
            });

            btn = YesNoMessagePanel.transform.Find("Panel").Find("YesButton").GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                YesNoMessagePanel.SetActive(false);
                BackGroundPanel.SetActive(false);
                OnYesNoMessageYes?.Invoke();
            });
            btn = YesNoMessagePanel.transform.Find("Panel").Find("NoButton").GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                YesNoMessagePanel.SetActive(false);
                BackGroundPanel.SetActive(false);
                OnYesNoMessageNo?.Invoke();
            });
        }

        private Coroutine fadeCoroutine;
        public void WarningMessage(string message, int fade = 2)
        {
            var TMPText = WarningMessagePanel.transform.Find("Text").GetComponent<TMP_Text>();
            TMPText.text = message;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            TMPText.color = new Color(1, 1, 1, 1);
            fadeCoroutine = StartCoroutine(FadeAfter(fade));
            WarningMessagePanel.SetActive(true);
        }

        IEnumerator FadeAfter(int fadeTime)
        {
            yield return new WaitForSeconds(fadeTime);
            var text = WarningMessagePanel.transform.Find("Text").GetComponent<TMP_Text>();
            var color = text.color;
            while (text.color.a > 10)
            {
                color.a = Mathf.Max(0, color.a - Time.deltaTime);
                text.color = color;
                yield return null;
            }
            WarningMessagePanel.SetActive(false);
        }

        public void OkMessage(string message, Action okAction = null)
        {
            OnOkMessageOk = okAction;
            OkMessagePanel.transform.Find("Text").GetComponent<TMP_Text>().text = message;
            OkMessagePanel.SetActive(true);
            BackGroundPanel.SetActive(true);
        }
        public void OkCancelMessage(string message, Action okAction = null, Action cancelAction = null)
        {
            OnOkCancelMessageOk = okAction;
            OnOkCancelMessageCancel = cancelAction;

            OkCancelMessagePanel.transform.Find("Text").GetComponent<TMP_Text>().text = message;
            OkCancelMessagePanel.SetActive(true);
            BackGroundPanel.SetActive(true);

        }
        public void YesNoMessage(string message, Action yesAction = null, Action noAction = null)
        {
            OnYesNoMessageYes = yesAction;
            OnYesNoMessageNo = noAction;

            YesNoMessagePanel.transform.Find("Text").GetComponent<TMP_Text>().text = message;
            YesNoMessagePanel.SetActive(true);
            BackGroundPanel.SetActive(true);
        }
    }
}