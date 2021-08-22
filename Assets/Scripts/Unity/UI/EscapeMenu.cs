using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Unity
{
    public class EscapeMenu : MonoBehaviour
    {
        public Canvas canvas { get; private set; }

        public void Awake()
        {
            canvas = gameObject.GetComponent<Canvas>();
            canvas.enabled = false;

            Transform menu = gameObject.transform.Find("Menu");

            Button backbtn = menu.Find("Back").GetComponent<Button>();
            backbtn.onClick.AddListener(Toggle);

            Button helpBtn = menu.Find("Help").GetComponent<Button>();
            helpBtn.onClick.AddListener(Help);

            Button settingsBtn = menu.Find("Settings").GetComponent<Button>();
            settingsBtn.onClick.AddListener(Settings);

            Button quitBtn = menu.Find("Quit").GetComponent<Button>();
            quitBtn.onClick.AddListener(Quit);

        }

        public void Toggle()
        {
            canvas.enabled = !canvas.enabled;
            GameController.CursorFocusUI(canvas.enabled);
        }

        public void Help()
        {
            System.Diagnostics.Process.Start("https://discord.com/invite/HXFqUqF");
        }

        public void Settings()
        {

        }

        public void Quit()
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
